using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using Unity.Services.Vivox;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Conversa por texto no Vivox; entra na primeira vez que um canal é aberto
    public class ChatDaUnity : ITransporteDeChat
    {
        public event Action<string, string, string, string> Recebida;
        readonly IContaOnline conta;
        string jogadorLogado;
        bool ouvindo;

        public ChatDaUnity(IContaOnline conta) => this.conta = conta;

        async Task Preparar(string nome)
        {
            if (!conta.Conectada) throw new FalhaOnline("Sem conexão com a conversa.");
            var v = VivoxService.Instance;
            if (!ouvindo)
            {
                await v.InitializeAsync();
                v.ChannelMessageReceived += m => Recebida?.Invoke(m.ChannelName, m.SenderPlayerId, m.SenderDisplayName, m.MessageText);
                ouvindo = true;
            }
            if (v.IsLoggedIn && jogadorLogado == conta.JogadorId) return;
            if (v.IsLoggedIn) await v.LogoutAsync();   // a conta mudou
            // o apelido vai com a mensagem; o Vivox limita o tamanho dele
            await v.LoginAsync(new LoginOptions { DisplayName = string.IsNullOrEmpty(nome) ? "Sem nome" : nome.Length <= 60 ? nome : nome.Substring(0, 60) });
            jogadorLogado = conta.JogadorId;
        }

        public Task EntrarNoCanal(string canal, string nome) => ChamadaOnline.Fazer(async () =>
        {
            await Preparar(nome);
            await VivoxService.Instance.JoinGroupChannelAsync(canal, ChatCapability.TextOnly);
        }, _ => "A conversa está indisponível.");

        public Task SairDoCanal(string canal) => ChamadaOnline.Fazer(() => VivoxService.Instance.LeaveChannelAsync(canal));

        public Task Enviar(string canal, string texto) => ChamadaOnline.Fazer(() => VivoxService.Instance.SendChannelTextMessageAsync(canal, texto), _ => "A mensagem não foi enviada.");
    }

    // Bloqueados deste aparelho
    public class BloqueadosNoAparelho : IBloqueados
    {
        const string Chave = "chat-bloqueados";
        const int Maximo = 200;
        readonly List<string> ids;

        public BloqueadosNoAparelho() => ids = new List<string>(PlayerPrefs.GetString(Chave, "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        public bool Esta(string jogadorId) => ids.Contains(jogadorId);

        public void Bloquear(string jogadorId)
        {
            if (Esta(jogadorId)) return;
            ids.Add(jogadorId);
            if (ids.Count > Maximo) ids.RemoveAt(0);
            PlayerPrefs.SetString(Chave, string.Join(",", ids));
            PlayerPrefs.Save();
        }
    }

    public class DenunciasDaUnity : IServicoDeDenuncias
    {
        public Task Denunciar(Denuncia d) => NuvemDaUnity.ChamarSemResposta("Denunciar", "denuncia", JsonUtility.ToJson(d));
    }
}
