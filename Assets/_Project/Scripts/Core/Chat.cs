using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ALotOfDamage.Core
{
    public enum CanalDeChat { Cidade, Guilda }

    public class MensagemDeChat
    {
        public CanalDeChat Canal;
        public string JogadorId = "", Nome = "", Texto = "";
        public bool Minha;
    }

    [Serializable]
    public class Denuncia
    {
        public string JogadorId = "", Canal = "", Texto = "";
    }

    // Limites e limpeza do texto; quem fala é outra pessoa, então nada é interpretado
    public static class RegrasDoChat
    {
        public const int TamanhoMaximo = 160;
        public const int MensagensPorJanela = 5;
        public const double SegundosDaJanela = 10;

        // Começo de palavras barradas (sem acento, minúsculas); a lista cresce conforme as denúncias
        static readonly string[] Barradas = { "caralh", "porra", "merda", "buceta", "viado", "puta", "cuzao", "foder", "fodase", "arrombad", "fuck", "shit", "bitch", "nigg" };

        public static string Limpar(string texto)
        {
            if (texto == null) return "";
            var sb = new StringBuilder();
            foreach (char c in texto) if (!char.IsControl(c)) sb.Append(c);
            string s = sb.ToString().Trim();
            return s.Length <= TamanhoMaximo ? s : s.Substring(0, TamanhoMaximo);
        }

        public static string Filtrar(string texto)
        {
            var partes = texto.Split(' ');
            for (int i = 0; i < partes.Length; i++)
            {
                string simples = Simplificar(partes[i]);
                foreach (var b in Barradas)
                    if (simples.StartsWith(b, StringComparison.Ordinal)) { partes[i] = new string('*', partes[i].Length); break; }
            }
            return string.Join(" ", partes);
        }

        static string Simplificar(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s.Normalize(NormalizationForm.FormD))
            {
                if (!char.IsLetterOrDigit(c)) continue;
                char m = char.ToLowerInvariant(c);
                sb.Append(m == '0' ? 'o' : m == '1' ? 'i' : m == '3' ? 'e' : m == '4' ? 'a' : m);
            }
            return sb.ToString();
        }
    }

    // Transporte das mensagens (Vivox no jogo)
    public interface ITransporteDeChat
    {
        event Action<string, string, string, string> Recebida;     // canal, id do jogador, nome, texto
        Task EntrarNoCanal(string canal, string nome);
        Task SairDoCanal(string canal);
        Task Enviar(string canal, string texto);
    }

    public interface IBloqueados
    {
        bool Esta(string jogadorId);
        void Bloquear(string jogadorId);
    }

    public interface IServicoDeDenuncias
    {
        Task Denunciar(Denuncia d);
    }

    // Conversa da cidade e da guilda: histórico curto, limite de mensagens, filtro e bloqueio
    public class ConversaDoJogador
    {
        const int MaximoNoHistorico = 60;

        readonly ITransporteDeChat transporte;
        readonly IBloqueados bloqueados;
        readonly IServicoDeDenuncias denuncias;
        readonly IContaOnline conta;
        readonly IRelogio relogio;
        readonly Dictionary<CanalDeChat, string> canais = new Dictionary<CanalDeChat, string>();
        readonly Queue<DateTime> enviadas = new Queue<DateTime>();

        public ConversaDoJogador(ITransporteDeChat transporte, IBloqueados bloqueados, IServicoDeDenuncias denuncias, IContaOnline conta, IRelogio relogio)
        {
            this.transporte = transporte;
            this.bloqueados = bloqueados;
            this.denuncias = denuncias;
            this.conta = conta;
            this.relogio = relogio;
            transporte.Recebida += Receber;
        }

        public List<MensagemDeChat> Historico { get; } = new List<MensagemDeChat>();
        public int Versao { get; private set; }     // sobe a cada mudança do histórico

        public bool Conectado(CanalDeChat c) => canais.ContainsKey(c);

        // Trocar de sala ou de guilda sai do canal anterior; id vazio só sai
        public async Task Entrar(CanalDeChat canal, string id)
        {
            string nome = string.IsNullOrEmpty(id) ? "" : (canal == CanalDeChat.Cidade ? "cidade-" : "guilda-") + id;
            canais.TryGetValue(canal, out var atual);
            if (atual == nome || (atual == null && nome == "")) return;
            if (atual != null)
            {
                canais.Remove(canal);
                Historico.RemoveAll(m => m.Canal == canal);
                Versao++;
                try { await transporte.SairDoCanal(atual); } catch (FalhaOnline) { }
            }
            if (nome == "") return;
            await transporte.EntrarNoCanal(nome, conta.Apelido);
            canais[canal] = nome;
        }

        public async Task Enviar(CanalDeChat canal, string texto)
        {
            texto = RegrasDoChat.Limpar(texto);
            if (texto == "") return;
            if (!canais.TryGetValue(canal, out var nome)) throw new FalhaOnline("Sem conexão com a conversa.");
            while (enviadas.Count > 0 && (relogio.AgoraUtc - enviadas.Peek()).TotalSeconds > RegrasDoChat.SegundosDaJanela) enviadas.Dequeue();
            if (enviadas.Count >= RegrasDoChat.MensagensPorJanela) throw new FalhaOnline("Calma! Espere alguns segundos para escrever de novo.");
            enviadas.Enqueue(relogio.AgoraUtc);
            await transporte.Enviar(nome, texto);
        }

        void Receber(string canal, string jogadorId, string nome, string texto)
        {
            if (bloqueados.Esta(jogadorId)) return;
            foreach (var c in canais)
            {
                if (c.Value != canal) continue;
                Historico.Add(new MensagemDeChat
                {
                    Canal = c.Key, JogadorId = jogadorId, Nome = RegrasDaGuilda.NomeLimpo(nome), Minha = jogadorId == conta.JogadorId,
                    Texto = RegrasDoChat.Filtrar(RegrasDoChat.Limpar(texto)),
                });
                if (Historico.Count > MaximoNoHistorico) Historico.RemoveAt(0);
                Versao++;
                return;
            }
        }

        public void Bloquear(string jogadorId)
        {
            if (string.IsNullOrEmpty(jogadorId) || jogadorId == conta.JogadorId) return;
            bloqueados.Bloquear(jogadorId);
            Historico.RemoveAll(m => m.JogadorId == jogadorId);
            Versao++;
        }

        // Denunciar também bloqueia: quem denuncia não precisa continuar lendo
        public async Task Denunciar(MensagemDeChat m)
        {
            await denuncias.Denunciar(new Denuncia { JogadorId = m.JogadorId, Canal = m.Canal.ToString(), Texto = m.Texto });
            Bloquear(m.JogadorId);
        }
    }
}
