using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using Unity.Services.CloudCode.Apis.Extensions;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;
using Unity.Services.Leaderboards.Model;

namespace ALotOfDamage.Nuvem
{
    public class Configuracao : ICloudCodeSetup
    {
        public void Setup(ICloudCodeConfig config)
        {
            config.AddGameApiClient();
        }
    }

    // Funções chamadas pelo jogo. O jogador é sempre o do token (contexto), nunca um id enviado pelo cliente.
    // O save fica nos dados protegidos: o cliente lê, mas só o servidor grava
    public class ModuloDaNuvem
    {
        public const string Chave = "progresso";
        const string RankingDeEstagio = "estagio", RankingDePoder = "poder";

        [CloudCodeFunction("CarregarProgresso")]
        public async Task<string> Carregar(IExecutionContext ctx, IGameApiClient api) =>
            Recepcao.Escrever((await LerSave(ctx, api)).save ?? new SaveNaNuvem());

        [CloudCodeFunction("SalvarProgresso")]
        public async Task<string> Salvar(IExecutionContext ctx, IGameApiClient api, string pedido)
        {
            var (atual, trava) = await LerSave(ctx, api);
            var r = Recepcao.Receber(atual, Recepcao.Ler<PedidoDeEnvio>(pedido), DateTime.UtcNow);
            if (r.Resultado != ResultadoDoEnvio.Salvo) return Recepcao.Escrever(r);

            try
            {
                await api.CloudSaveData.SetProtectedItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, ctx.PlayerId,
                    new SetItemBody(Chave, Recepcao.Escrever(r.Nuvem), trava));
            }
            catch (ApiException e) when (e.Response != null && (int)e.Response.StatusCode == 409)
            {
                // outro aparelho salvou entre a leitura e a gravação
                return Recepcao.Escrever(new RespostaDoEnvio
                {
                    Resultado = ResultadoDoEnvio.Conflito, Motivo = "a nuvem mudou em outro aparelho", Nuvem = (await LerSave(ctx, api)).save ?? new SaveNaNuvem(),
                });
            }

            // o ranking não pode derrubar o save
            try { await Pontuar(ctx, api, Recepcao.Ler<DadosDoProgresso>(r.Nuvem.Progresso).EstagioMaximo, r.Nuvem.Poder); }
            catch (ApiException) { }
            // o cliente já tem o próprio progresso: não precisa voltar
            r.Nuvem.Progresso = "";
            return Recepcao.Escrever(r);
        }

        [CloudCodeFunction("Guilda")]
        public async Task<string> Guilda(IExecutionContext ctx, IGameApiClient api, string pedido)
        {
            string nome = "";
            try { nome = (await api.PlayerNamesApi.GetNameAsync(ctx, ctx.AccessToken, ctx.PlayerId, true)).Data?.Name ?? ""; }
            catch (ApiException) { }   // sem nome, o jogo mostra "Sem nome"
            return Recepcao.Escrever(await ServicoDe(ctx, api, nome).Executar(Recepcao.Ler<PedidoDeGuilda>(pedido)));
        }

        [CloudCodeFunction("Arena")]
        public async Task<string> Arena(IExecutionContext ctx, IGameApiClient api, string pedido) =>
            Recepcao.Escrever(await new ServicoDaArena(new ArmazemDaNuvem(ctx, api), new PosicoesDaNuvem(ctx, api), Recepcao.Arena, new SorteioSeguro(), ctx.PlayerId, DateTime.UtcNow)
                .Executar(Recepcao.Ler<PedidoDaArena>(pedido)));

        [CloudCodeFunction("Denunciar")]
        public Task<bool> Denunciar(IExecutionContext ctx, IGameApiClient api, string denuncia) =>
            new ServicoDeDenuncias(new ArmazemDaNuvem(ctx, api), ctx.PlayerId, DateTime.UtcNow).Registrar(Recepcao.Ler<Denuncia>(denuncia));

        static ServicoDaGuilda ServicoDe(IExecutionContext ctx, IGameApiClient api, string nome = "") =>
            new ServicoDaGuilda(new ArmazemDaNuvem(ctx, api), Recepcao.Guilda, ctx.PlayerId, nome, DateTime.UtcNow);

        [CloudCodeFunction("ApagarDados")]
        public async Task<bool> Apagar(IExecutionContext ctx, IGameApiClient api)
        {
            // remover a linha do ranking exige uma chave de administrador; com zero pontos, o jogo não a mostra
            await Pontuar(ctx, api, 0, 0);
            await new PosicoesDaNuvem(ctx, api).Pontuar(0);
            // sai da guilda para não ficar um membro fantasma; sem guilda, a resposta é só um erro ignorado
            await ServicoDe(ctx, api).Executar(new PedidoDeGuilda { Acao = AcaoDaGuilda.Sair });
            await api.CloudSaveData.DeleteProtectedItemsAsync(ctx, ctx.ServiceToken, ctx.ProjectId, ctx.PlayerId);
            return true;
        }

        static async Task Pontuar(IExecutionContext ctx, IGameApiClient api, double estagio, double poder)
        {
            var projeto = Guid.Parse(ctx.ProjectId);
            await api.Leaderboards.AddLeaderboardPlayerScoreAsync(ctx, ctx.ServiceToken, projeto, RankingDeEstagio, ctx.PlayerId, new AddLeaderboardScore(estagio));
            await api.Leaderboards.AddLeaderboardPlayerScoreAsync(ctx, ctx.ServiceToken, projeto, RankingDePoder, ctx.PlayerId, new AddLeaderboardScore(poder));
        }

        static async Task<(SaveNaNuvem save, string trava)> LerSave(IExecutionContext ctx, IGameApiClient api)
        {
            var r = await api.CloudSaveData.GetProtectedItemsAsync(ctx, ctx.ServiceToken, ctx.ProjectId, ctx.PlayerId, new List<string> { Chave });
            foreach (var item in r.Data.Results)
            {
                if (item.Key != Chave) continue;
                // um save que existe mas não se lê não pode ser tratado como conta nova e sobrescrito
                var save = Recepcao.Ler<SaveNaNuvem>(item.Value?.ToString()) ?? throw new InvalidOperationException("save guardado ilegível");
                return (save, item.WriteLock);
            }
            return (null, null);
        }
    }
}
