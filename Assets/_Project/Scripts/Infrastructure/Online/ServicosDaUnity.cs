using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    static class ChamadaOnline
    {
        // Toda falha dos serviços vira FalhaOnline; a causa completa fica no log, sem dados digitados pelo jogador
        public static async Task<T> Fazer<T>(Func<Task<T>> acao, Func<RequestFailedException, string> mensagem = null)
        {
            try { return await acao(); }
            catch (FalhaOnline) { throw; }
            catch (RequestFailedException e)
            {
                Debug.LogWarning($"Online: erro {e.ErrorCode}");
                throw new FalhaOnline(mensagem?.Invoke(e) ?? "Sem conexão com o servidor.", e);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Online: {e.GetType().Name}");
                throw new FalhaOnline("Sem conexão com o servidor.", e);
            }
        }

        public static Task Fazer(Func<Task> acao, Func<RequestFailedException, string> mensagem = null) =>
            Fazer(async () => { await acao(); return true; }, mensagem);
    }

    // Conta pelo Unity Authentication: convidado por padrão, com usuário e senha opcionais para entrar em outro aparelho
    public class ContaDaUnity : IContaOnline
    {
        const string AmbienteDeTeste = "desenvolvimento", AmbienteDoJogo = "production";

        public bool Disponivel => true;
        public bool Conectada => UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn;
        public string JogadorId => Conectada ? AuthenticationService.Instance.PlayerId : "";
        public string Apelido { get; private set; } = "";
        public bool Vinculada { get; private set; }

        public Task Entrar() => ChamadaOnline.Fazer(async () =>
        {
            await Iniciar();
            var a = AuthenticationService.Instance;
            if (!a.IsSignedIn) await a.SignInAnonymouslyAsync();   // com sessão guardada, volta para a mesma conta
            await LerPerfil();
        });

        static async Task Iniciar()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized) return;
            bool teste = Application.isEditor || Debug.isDebugBuild;
            await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(teste ? AmbienteDeTeste : AmbienteDoJogo));
        }

        // Apelido e vínculo são detalhe: sem eles, a conta continua valendo
        async Task LerPerfil()
        {
            var a = AuthenticationService.Instance;
            try
            {
                var info = await a.GetPlayerInfoAsync();
                Vinculada = !string.IsNullOrEmpty(info?.Username);
                Apelido = await a.GetPlayerNameAsync() ?? "";
            }
            catch (RequestFailedException e) { Debug.LogWarning($"Online: perfil não lido ({e.ErrorCode})"); }
        }

        public Task Vincular(string usuario, string senha) => ChamadaOnline.Fazer(async () =>
        {
            Exigir(RegrasDaConta.ProblemaDoUsuario(usuario) ?? RegrasDaConta.ProblemaDaSenha(senha));
            await AuthenticationService.Instance.AddUsernamePasswordAsync(usuario, senha);
            Vinculada = true;
        }, e => e.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked ? "Este usuário já está em uso."
            : e.ErrorCode == AuthenticationErrorCodes.InvalidParameters ? "Usuário ou senha não aceitos, ou já em uso." : null);

        public Task EntrarComConta(string usuario, string senha) => ChamadaOnline.Fazer(async () =>
        {
            Exigir(RegrasDaConta.ProblemaDoUsuario(usuario) ?? RegrasDaConta.ProblemaDaSenha(senha));
            await Iniciar();
            var a = AuthenticationService.Instance;
            // a sessão atual fica guardada até a nova entrar: senha errada volta para ela
            if (a.IsSignedIn) a.SignOut();
            try { await a.SignInWithUsernamePasswordAsync(usuario, senha); }
            catch (RequestFailedException)
            {
                if (a.SessionTokenExists) await a.SignInAnonymouslyAsync();
                throw;
            }
            await LerPerfil();
        }, e => e.ErrorCode == AuthenticationErrorCodes.InvalidParameters || e.ErrorCode == CommonErrorCodes.InvalidRequest ? "Usuário ou senha incorretos." : null);

        public Task TrocarApelido(string apelido) => ChamadaOnline.Fazer(async () =>
        {
            Exigir(RegrasDaConta.ProblemaDoApelido(apelido));
            Apelido = await AuthenticationService.Instance.UpdatePlayerNameAsync(apelido);
        });

        public Task Apagar() => ChamadaOnline.Fazer(async () =>
        {
            // o nome pode continuar no ranking por um tempo
            try { await AuthenticationService.Instance.UpdatePlayerNameAsync("Apagado"); }
            catch (RequestFailedException e) { Debug.LogWarning($"Online: apelido não trocado ({e.ErrorCode})"); }
            await AuthenticationService.Instance.DeleteAccountAsync();
            Apelido = "";
            Vinculada = false;
        });

        static void Exigir(string problema) { if (problema != null) throw new FalhaOnline(problema); }
    }

    // Progresso pelo módulo C# do Cloud Code (Server~/Nuvem), que confere e guarda
    public class NuvemDaUnity : INuvemDeProgresso
    {
        const string Modulo = "Nuvem";

        static Task<T> Chamar<T>(string funcao, Dictionary<string, object> dados = null) where T : class => ChamadaOnline.Fazer(async () =>
        {
            string json = await CloudCodeService.Instance.CallModuleEndpointAsync<string>(Modulo, funcao, dados);
            return JsonUtility.FromJson<T>(json) ?? throw new FalhaOnline("Resposta inesperada do servidor.");
        });

        public Task<SaveNaNuvem> Carregar() => Chamar<SaveNaNuvem>("CarregarProgresso");

        public Task<RespostaDoEnvio> Enviar(PedidoDeEnvio pedido) =>
            Chamar<RespostaDoEnvio>("SalvarProgresso", new Dictionary<string, object> { ["pedido"] = JsonUtility.ToJson(pedido) });

        public Task Apagar() => ChamadaOnline.Fazer(() => CloudCodeService.Instance.CallModuleEndpointAsync<bool>(Modulo, "ApagarDados"));
    }

    public class RankingDaUnity : IRanking
    {
        static string Id(TipoDeRanking tipo) => tipo == TipoDeRanking.Poder ? "poder" : "estagio";

        public Task<PaginaDoRanking> Buscar(TipoDeRanking tipo, int quantos) => ChamadaOnline.Fazer(async () =>
        {
            var s = LeaderboardsService.Instance;
            var pagina = new PaginaDoRanking();
            var r = await s.GetScoresAsync(Id(tipo), new GetScoresOptions { Limit = quantos });
            foreach (var e in r.Results) if (e.Score > 0) pagina.Primeiros.Add(Linha(e));   // conta apagada fica com zero
            try { pagina.Minha = Linha(await s.GetPlayerScoreAsync(Id(tipo))); }
            catch (LeaderboardsException e) when (e.Reason == LeaderboardsExceptionReason.EntryNotFound) { }   // ainda não pontuou
            return pagina;
        });

        static LinhaDoRanking Linha(Unity.Services.Leaderboards.Models.LeaderboardEntry e) =>
            new LinhaDoRanking { Posicao = e.Rank + 1, JogadorId = e.PlayerId, Nome = e.PlayerName, Pontos = e.Score };
    }
}
