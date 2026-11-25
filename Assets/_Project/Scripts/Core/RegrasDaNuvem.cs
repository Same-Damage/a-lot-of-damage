using System;

namespace ALotOfDamage.Core
{
    // O que fica guardado na nuvem; o progresso vai em JSON para o servidor não depender do formato ao guardar
    [Serializable]
    public class SaveNaNuvem
    {
        public int Revisao;             // 0 = não há save na nuvem
        public int Versao;
        public string Progresso = "";
        public string Dispositivo = "";
        public long SalvoEmUtc;
        public double Poder;

        // De onde a conta partiu: limita o avanço total ao tempo que passou desde então
        public long CriadoEmUtc;
        public int VitoriasIniciais;
        public int EstagioInicial;
        public double OuroInicial;

        public bool Existe => Revisao > 0;
    }

    [Serializable]
    public class PedidoDeEnvio
    {
        public string Progresso = "";
        public int RevisaoBase;         // revisão da nuvem que este aparelho conhece
        public int Versao;
        public string Dispositivo = "";
        public bool Forcar;             // o jogador escolheu este aparelho num conflito
    }

    public enum ResultadoDoEnvio { Salvo, Conflito, Recusado, JogoAntigo }

    [Serializable]
    public class RespostaDoEnvio
    {
        public ResultadoDoEnvio Resultado;
        public string Motivo = "";
        public SaveNaNuvem Nuvem = new SaveNaNuvem();
    }

    public enum AcaoDaSincronia { Nada, Enviar, Baixar, Conflito }

    [Serializable]
    public class ParametrosDaNuvem
    {
        public double SegundosPorVitoria = 0.5;         // mais rápido que isso, sem parar, é impossível
        public int FolgaDeVitorias = 60;
        public double FolgaDoOuro = 1000;               // os bônus se multiplicam: o teto do ouro é bem folgado
        public double HorasDeOuroAcumulado = 24;        // AFK e taverna guardados antes do envio
    }

    // Sincronia do progresso com a nuvem. As mesmas regras rodam no jogo e no servidor
    public static class RegrasDaNuvem
    {
        // Muda quando o formato do save muda de um jeito que um jogo antigo não pode sobrescrever
        public const int VersaoDoSave = 1;

        public static AcaoDaSincronia Decidir(int revisaoLocal, int revisaoDaNuvem, bool localMudou, bool localVazio)
        {
            if (revisaoDaNuvem <= 0) return AcaoDaSincronia.Enviar;
            if (revisaoDaNuvem == revisaoLocal) return localMudou ? AcaoDaSincronia.Enviar : AcaoDaSincronia.Nada;
            if (localVazio) return AcaoDaSincronia.Baixar;
            return localMudou || revisaoDaNuvem < revisaoLocal ? AcaoDaSincronia.Conflito : AcaoDaSincronia.Baixar;
        }

        public static bool Vazio(DadosDoProgresso d) =>
            d.Vitorias == 0 && d.Ouro <= 0 && d.Estagio <= 1 && d.EstagioMaximo <= 1 && d.Ascensoes == 0;

        // Identifica o conteúdo do progresso; a marca de presença muda sozinha e não conta como mudança
        public static string Impressao(DadosDoProgresso d, Func<DadosDoProgresso, string> texto)
        {
            long presenca = d.UltimaPresencaUtc;
            d.UltimaPresencaUtc = 0;
            string s = texto(d);
            d.UltimaPresencaUtc = presenca;
            ulong h = 14695981039346656037UL;   // FNV-1a
            foreach (char c in s) { h ^= c; h = unchecked(h * 1099511628211UL); }
            return h.ToString("x16");
        }

        // null se os números fazem sentido sozinhos
        public static string ProblemaDosDados(DadosDoProgresso d)
        {
            if (d == null) return "progresso vazio";
            if (!Numero(d.Ouro) || !Numero(d.OuroTotalGanho)) return "ouro inválido";
            if (d.Vitorias < 0 || d.Cristais < 0 || d.Almas < 0 || d.Ascensoes < 0 || d.Fragmentos < 0) return "contador negativo";
            if (d.Estagio < 1 || d.EstagioMaximo < d.Estagio) return "estágio inválido";
            if (d.Ouro > d.OuroTotalGanho * 1.0001 + 1) return "ouro maior que o total ganho";
            return null;
        }

        static bool Numero(double v) => !double.IsNaN(v) && !double.IsInfinity(v) && v >= 0;

        // null se dá para sair daquele ponto e chegar a este progresso no tempo que passou
        public static string ProblemaDoAvanco(ParametrosDaNuvem p, ParametrosDaCampanha c, int vitorias, int estagio, double ouro, DadosDoProgresso novo, double segundos)
        {
            segundos = Math.Max(0, segundos);
            double ganhas = novo.Vitorias - (double)vitorias;
            if (ganhas > segundos / p.SegundosPorVitoria + p.FolgaDeVitorias) return "vitórias demais para o tempo";
            if (novo.EstagioMaximo - estagio > Math.Max(0, ganhas)) return "estágios demais para as vitórias";
            double porLuta = RegrasDaCampanha.Ouro(c, RegrasDaCampanha.VidaDoInimigo(c, novo.EstagioMaximo, 1, true));
            double teto = porLuta * p.FolgaDoOuro * (segundos + p.HorasDeOuroAcumulado * 3600);
            return novo.OuroTotalGanho - ouro > teto ? "ouro demais para o tempo" : null;
        }

        // Decisão do servidor: salva por cima da revisão que o aparelho conhece, se o avanço for possível
        public static RespostaDoEnvio Receber(ParametrosDaNuvem p, ParametrosDaCampanha c, SaveNaNuvem atual, DadosDoProgresso anterior,
            PedidoDeEnvio pedido, DadosDoProgresso novo, double poder, DateTime agoraUtc)
        {
            atual = atual ?? new SaveNaNuvem();
            RespostaDoEnvio Nao(ResultadoDoEnvio r, string motivo) => new RespostaDoEnvio { Resultado = r, Motivo = motivo, Nuvem = atual };

            if (pedido.Versao < atual.Versao) return Nao(ResultadoDoEnvio.JogoAntigo, "há uma versão mais nova do jogo");
            if (pedido.RevisaoBase != atual.Revisao) return Nao(ResultadoDoEnvio.Conflito, "a nuvem mudou em outro aparelho");
            string problema = ProblemaDosDados(novo);
            if (problema == null && atual.Existe)
            {
                double Desde(long ticks) => (agoraUtc.Ticks - ticks) / (double)TimeSpan.TicksPerSecond;
                problema = ProblemaDoAvanco(p, c, atual.VitoriasIniciais, atual.EstagioInicial, atual.OuroInicial, novo, Desde(atual.CriadoEmUtc));
                // no conflito resolvido, o progresso escolhido não veio do que está na nuvem
                if (problema == null && !pedido.Forcar && anterior != null)
                    problema = ProblemaDoAvanco(p, c, anterior.Vitorias, anterior.EstagioMaximo, anterior.OuroTotalGanho, novo, Desde(atual.SalvoEmUtc));
            }
            if (problema != null) return Nao(ResultadoDoEnvio.Recusado, problema);

            return new RespostaDoEnvio
            {
                Resultado = ResultadoDoEnvio.Salvo,
                Nuvem = new SaveNaNuvem
                {
                    Revisao = atual.Revisao + 1, Versao = pedido.Versao, Progresso = pedido.Progresso, Dispositivo = pedido.Dispositivo,
                    SalvoEmUtc = agoraUtc.Ticks, Poder = poder,
                    CriadoEmUtc = atual.Existe ? atual.CriadoEmUtc : agoraUtc.Ticks,
                    VitoriasIniciais = atual.Existe ? atual.VitoriasIniciais : novo.Vitorias,
                    EstagioInicial = atual.Existe ? atual.EstagioInicial : novo.EstagioMaximo,
                    OuroInicial = atual.Existe ? atual.OuroInicial : novo.OuroTotalGanho,
                },
            };
        }
    }

    // O que o servidor da Unity aceita no usuário e na senha, para avisar antes de enviar
    public static class RegrasDaConta
    {
        public static string ProblemaDoUsuario(string u)
        {
            if (u == null || u.Length < 3 || u.Length > 20) return "O usuário precisa ter de 3 a 20 caracteres.";
            foreach (char c in u)
                if (!(c < 128 && char.IsLetterOrDigit(c)) && c != '.' && c != '-' && c != '_' && c != '@') return "Use só letras sem acento, números e . - _ @ no usuário.";
            return null;
        }

        public static string ProblemaDaSenha(string s)
        {
            if (s == null || s.Length < 8 || s.Length > 30) return "A senha precisa ter de 8 a 30 caracteres.";
            bool maiuscula = false, minuscula = false, numero = false, simbolo = false;
            foreach (char c in s)
            {
                if (char.IsUpper(c)) maiuscula = true;
                else if (char.IsLower(c)) minuscula = true;
                else if (char.IsDigit(c)) numero = true;
                else simbolo = true;
            }
            return maiuscula && minuscula && numero && simbolo ? null : "A senha precisa de maiúscula, minúscula, número e símbolo.";
        }

        public static string ProblemaDoApelido(string a)
        {
            if (a == null || a.Length < 3 || a.Length > 20) return "O apelido precisa ter de 3 a 20 caracteres.";
            foreach (char c in a) if (char.IsWhiteSpace(c) || char.IsControl(c)) return "O apelido não pode ter espaços.";
            return null;
        }
    }
}
