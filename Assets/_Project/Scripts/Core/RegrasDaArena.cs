using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    [Serializable]
    public class LigaDaArena
    {
        public string Nome = "";
        public int Pontos;              // a partir de quantos pontos
        public long Cristais;           // recompensa de quem termina a temporada nela
    }

    [Serializable]
    public class ParametrosDaArena
    {
        public int LutasPorDia = 5;
        public int Adversarios = 3;
        public int PontosIniciais = 1000;
        public int PontosPorVitoria = 20;
        public int BonusPorSuperior = 10;       // a mais contra quem tem mais pontos, a menos contra quem tem menos
        public long CristaisPorVitoria = 5;
        public int Rodadas = 5;
        public double PesoDoPoder = 0.5;        // expoente: Poder 10× maior não é vitória certa
        public double ChanceMinima = 0.05;

        public List<LigaDaArena> Ligas = new List<LigaDaArena>
        {
            new LigaDaArena { Nome = "Bronze", Pontos = 0, Cristais = 50 },
            new LigaDaArena { Nome = "Prata", Pontos = 1200, Cristais = 100 },
            new LigaDaArena { Nome = "Ouro", Pontos = 1500, Cristais = 200 },
            new LigaDaArena { Nome = "Platina", Pontos = 2000, Cristais = 350 },
            new LigaDaArena { Nome = "Lendária", Pontos = 2800, Cristais = 600 },
        };
    }

    [Serializable]
    public class AdversarioDaArena
    {
        public string JogadorId = "", Nome = "", HeroiId = "";
        public double Poder;
        public int Pontos, Estagio;
    }

    // Guardado pelo servidor
    [Serializable]
    public class JogadorNaArena
    {
        public int Temporada, Pontos;
        public int LutasNaTemporada;
        public int Dia, LutasNoDia;
        public List<AdversarioDaArena> Ofertas = new List<AdversarioDaArena>();
        public long CristaisTotais;             // tudo que a arena já entregou, como na guilda
    }

    [Serializable]
    public class ResultadoDaArena
    {
        public bool Lutou, Venceu;
        public string Adversario = "";
        public List<bool> Rodadas = new List<bool>();   // true: rodada do jogador
        public int PontosGanhos;
    }

    public enum AcaoDaArena { Ver, OutrosAdversarios, Lutar }

    [Serializable]
    public class PedidoDaArena
    {
        public AcaoDaArena Acao;
        public string Texto = "";
    }

    [Serializable]
    public class VistaDaArena
    {
        public string Erro = "";
        public int Pontos, LutasRestantes;
        public string Liga = "";
        public long CristaisDaLiga, CristaisTotais;
        public double MeuPoder, SegundosParaVirar;
        public List<AdversarioDaArena> Adversarios = new List<AdversarioDaArena>();
        public ResultadoDaArena Resultado = new ResultadoDaArena();
        public long RecompensaDaTemporada;      // entregue agora, pela temporada que acabou
    }

    public static class RegrasDaArena
    {
        // Um mês por temporada
        public static int Temporada(DateTime utc) => utc.Year * 12 + utc.Month - 1;
        public static double SegundosParaVirar(DateTime utc) =>
            (new DateTime(utc.Year, utc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1) - utc).TotalSeconds;

        public static LigaDaArena Liga(ParametrosDaArena p, int pontos)
        {
            var liga = p.Ligas[0];
            foreach (var l in p.Ligas) if (pontos >= l.Pontos) liga = l;
            return liga;
        }

        // Abre a temporada atual; se lutou na anterior, entrega a recompensa da liga em que terminou
        public static long Virar(ParametrosDaArena p, JogadorNaArena j, DateTime utc)
        {
            int t = Temporada(utc);
            if (j.Temporada == t) return 0;
            long premio = j.LutasNaTemporada > 0 ? Liga(p, j.Pontos).Cristais : 0;
            j.CristaisTotais += premio;
            j.Temporada = t;
            j.Pontos = p.PontosIniciais;
            j.LutasNaTemporada = 0;
            j.Ofertas.Clear();
            return premio;
        }

        public static int LutasRestantes(ParametrosDaArena p, JogadorNaArena j, DateTime utc) =>
            Math.Max(0, p.LutasPorDia - (j.Dia == RegrasDaGuilda.Dia(utc) ? j.LutasNoDia : 0));

        public static double Chance(ParametrosDaArena p, double meu, double dele)
        {
            double a = Math.Pow(Math.Max(1, meu), p.PesoDoPoder), b = Math.Pow(Math.Max(1, dele), p.PesoDoPoder);
            if (double.IsInfinity(a) || double.IsInfinity(b)) return 0.5;
            return Math.Max(p.ChanceMinima, Math.Min(1 - p.ChanceMinima, a / (a + b)));
        }

        // Melhor de N rodadas; para quando alguém fecha a maioria
        public static ResultadoDaArena Lutar(ParametrosDaArena p, double meu, double dele, ISorteio sorteio)
        {
            double chance = Chance(p, meu, dele);
            int precisa = p.Rodadas / 2 + 1, eu = 0, ele = 0;
            var r = new ResultadoDaArena { Lutou = true };
            while (eu < precisa && ele < precisa)
            {
                bool minha = sorteio.Proximo() < chance;
                r.Rodadas.Add(minha);
                if (minha) eu++; else ele++;
            }
            r.Venceu = eu > ele;
            return r;
        }

        // Só quem ataca muda de pontos, e perder não tira nada
        public static int PontosDaVitoria(ParametrosDaArena p, int meus, int dele) =>
            p.PontosPorVitoria + (dele > meus ? p.BonusPorSuperior : dele < meus - 100 ? -p.BonusPorSuperior : 0);

        // Gasta a luta, aplica o resultado e troca os adversários; null se não pode lutar com este
        public static ResultadoDaArena Enfrentar(ParametrosDaArena p, JogadorNaArena j, string adversarioId, double meuPoder, ISorteio sorteio, DateTime utc)
        {
            var alvo = j.Ofertas.Find(o => o.JogadorId == adversarioId);
            if (alvo == null || LutasRestantes(p, j, utc) <= 0) return null;
            int dia = RegrasDaGuilda.Dia(utc);
            if (j.Dia != dia) { j.Dia = dia; j.LutasNoDia = 0; }
            j.LutasNoDia++;
            j.LutasNaTemporada++;
            var r = Lutar(p, meuPoder, alvo.Poder, sorteio);
            r.Adversario = alvo.Nome;
            if (r.Venceu)
            {
                r.PontosGanhos = PontosDaVitoria(p, j.Pontos, alvo.Pontos);
                j.Pontos += r.PontosGanhos;
                j.CristaisTotais += p.CristaisPorVitoria;
            }
            j.Ofertas.Clear();
            return r;
        }
    }

    // Conversa com o servidor da arena e entrega os cristais no progresso
    public class ArenaDoJogador
    {
        readonly IRepositorioDeProgresso repositorio;
        readonly IServicoDaArena servico;

        public ArenaDoJogador(IRepositorioDeProgresso repositorio, IServicoDaArena servico)
        {
            this.repositorio = repositorio;
            this.servico = servico;
        }

        public VistaDaArena Vista { get; private set; }

        // Devolve os cristais que entraram agora
        public async System.Threading.Tasks.Task<long> Pedir(AcaoDaArena acao, string texto = "")
        {
            var v = await servico.Pedir(new PedidoDaArena { Acao = acao, Texto = texto });
            if (!string.IsNullOrEmpty(v.Erro)) throw new FalhaOnline(v.Erro);
            Vista = v;
            var p = repositorio.Carregar();
            long ganho = p.ReceberDaArena(v.CristaisTotais);
            if (ganho > 0) repositorio.Salvar(p);
            return ganho;
        }
    }
}
