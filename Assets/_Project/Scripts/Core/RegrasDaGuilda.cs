using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    [Serializable]
    public class ParametrosDaGuilda
    {
        public int MaximoDeMembros = 20;
        public int LutasPorDia = 3;
        public double SegundosDaLuta = 60;
        public double FolgaParaEnviar = 120;        // segundos além da luta para o resultado chegar ao servidor

        // Pontos da luta: o dano é relativo, então vale a habilidade e não o Poder
        public double PontosPorClique = 1;
        public double PontosPorPontoFraco = 4;
        public double PontosPorUltimate = 15;
        public double PontosPorCombo = 0.2;         // por golpe do maior combo
        public int CliquesPorSegundo = 15;

        public double VidaDaPrimeiraFase = 3000;
        public double CrescimentoDaFase = 1.5;
        public int Fases = 10;
        public long CristaisPorFase = 30;
        public long FragmentosPorFase = 2;

        public string NomeDoChefe = "Colosso das Marés";
    }

    // O que o jogador fez na luta; o servidor transforma em pontos
    [Serializable]
    public class ContagemDaLuta
    {
        public int Cliques, PontosFracos, Ultimates, MaiorCombo;
    }

    [Serializable]
    public class PontosDaSemana
    {
        public int Semana;
        public double Pontos;
        public int Lutas;
    }

    [Serializable]
    public class MembroDaGuilda
    {
        public string JogadorId = "";
        public string Nome = "";
        public long EntrouEmUtc;
    }

    [Serializable]
    public class DadosDaGuilda
    {
        public string Id = "", Nome = "", Codigo = "", LiderId = "";
        public List<MembroDaGuilda> Membros = new List<MembroDaGuilda>();
        public List<PontosDaSemana> DeQuemSaiu = new List<PontosDaSemana>();   // o dano já dado continua valendo

        public MembroDaGuilda Membro(string id) => Membros.Find(m => m.JogadorId == id);
    }

    [Serializable]
    public class FasesColetadas
    {
        public int Semana;
        public int Fases;
    }

    // O que o servidor guarda de cada jogador sobre guildas; vale mesmo trocando de guilda
    [Serializable]
    public class JogadorNaGuilda
    {
        public string GuildaId = "";
        public int Dia;
        public int LutasNoDia;
        public string LutaId = "";
        public long LutaInicioUtc;
        public List<FasesColetadas> Coletas = new List<FasesColetadas>();
        // Tudo que já foi entregue: o jogo soma só a diferença, então repetir o pedido não duplica nem perde
        public long CristaisTotais, FragmentosTotais;
    }

    public struct SituacaoDoChefe
    {
        public int FasesDerrubadas;
        public double PontosNaFase, VidaDaFase;
        public bool Derrotado;
    }

    public static class RegrasDaGuilda
    {
        public const int TamanhoDoCodigo = 6;
        // sem letras e números que se confundem
        public const string LetrasDoCodigo = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        // 1/1/0001 foi segunda-feira: a semana vira toda segunda à meia-noite UTC
        public static int Dia(DateTime utc) => (int)(utc.Ticks / TimeSpan.TicksPerDay);
        public static int Semana(DateTime utc) => Dia(utc) / 7;
        public static double SegundosParaVirar(DateTime utc) => (((Semana(utc) + 1) * 7L) * TimeSpan.TicksPerDay - utc.Ticks) / (double)TimeSpan.TicksPerSecond;

        public static int LutasRestantes(ParametrosDaGuilda p, JogadorNaGuilda j, DateTime utc) =>
            Math.Max(0, p.LutasPorDia - (j.Dia == Dia(utc) ? j.LutasNoDia : 0));

        // A luta conta ao começar: fechar o jogo no meio não devolve a tentativa
        public static bool Comecar(ParametrosDaGuilda p, JogadorNaGuilda j, string lutaId, DateTime utc)
        {
            if (LutasRestantes(p, j, utc) <= 0) return false;
            if (j.Dia != Dia(utc)) { j.Dia = Dia(utc); j.LutasNoDia = 0; }
            j.LutasNoDia++;
            j.LutaId = lutaId;
            j.LutaInicioUtc = utc.Ticks;
            return true;
        }

        // Pontos da luta aberta, ou null se não há luta para terminar. Encerra a luta
        public static double? Terminar(ParametrosDaGuilda p, JogadorNaGuilda j, string lutaId, ContagemDaLuta c, DateTime utc)
        {
            if (string.IsNullOrEmpty(j.LutaId) || j.LutaId != lutaId || c == null) return null;
            double segundos = (utc.Ticks - j.LutaInicioUtc) / (double)TimeSpan.TicksPerSecond;
            j.LutaId = "";
            if (segundos < 0 || segundos > p.SegundosDaLuta + p.FolgaParaEnviar) return null;
            return Pontos(p, c, Math.Min(segundos, p.SegundosDaLuta));
        }

        // Cada contagem tem um teto: não dá para ter mais do que os cliques possíveis no tempo permitem
        public static double Pontos(ParametrosDaGuilda p, ContagemDaLuta c, double segundos)
        {
            int cliques = Limitar(c.Cliques, (int)Math.Ceiling(Math.Max(0, segundos) * p.CliquesPorSegundo));
            int fracos = Limitar(c.PontosFracos, cliques);
            int combo = Limitar(c.MaiorCombo, cliques);
            // carga da ultimate no combate: 2 por clique e mais 10 no ponto fraco, até 100
            int ultimates = Limitar(c.Ultimates, (cliques * 2 + fracos * 10) / 100);
            return Math.Floor(cliques * p.PontosPorClique + fracos * p.PontosPorPontoFraco + ultimates * p.PontosPorUltimate + combo * p.PontosPorCombo);
        }

        static int Limitar(int v, int maximo) => Math.Max(0, Math.Min(v, maximo));

        public static double VidaDaFase(ParametrosDaGuilda p, int fase) => Math.Floor(p.VidaDaPrimeiraFase * Math.Pow(p.CrescimentoDaFase, fase));

        public static SituacaoDoChefe Chefe(ParametrosDaGuilda p, double pontos)
        {
            int fase = 0;
            while (fase < p.Fases && pontos >= VidaDaFase(p, fase)) pontos -= VidaDaFase(p, fase++);
            bool fim = fase >= p.Fases;
            double vida = VidaDaFase(p, fim ? p.Fases - 1 : fase);
            return new SituacaoDoChefe { FasesDerrubadas = fase, VidaDaFase = vida, PontosNaFase = fim ? vida : pontos, Derrotado = fim };
        }

        public static PontosDaSemana NaSemana(List<PontosDaSemana> lista, int semana) => lista.Find(x => x.Semana == semana) ?? new PontosDaSemana { Semana = semana };

        // Soma na semana e esquece as que não dão mais recompensa (só a atual e a anterior)
        public static void Somar(List<PontosDaSemana> lista, int semana, double pontos, int lutas, int semanaAtual)
        {
            lista.RemoveAll(x => x.Semana < semanaAtual - 1);
            var s = lista.Find(x => x.Semana == semana);
            if (s == null) lista.Add(s = new PontosDaSemana { Semana = semana });
            s.Pontos += pontos;
            s.Lutas += lutas;
        }

        public static int FasesAColetar(JogadorNaGuilda j, int semana, int derrubadas, bool lutou) =>
            lutou ? Math.Max(0, derrubadas - (j.Coletas.Find(x => x.Semana == semana)?.Fases ?? 0)) : 0;

        // Entrega as fases novas de uma semana; devolve quantas foram
        public static int Coletar(ParametrosDaGuilda p, JogadorNaGuilda j, int semana, int derrubadas, bool lutou, int semanaAtual)
        {
            int novas = FasesAColetar(j, semana, derrubadas, lutou);
            if (novas <= 0) return 0;
            j.Coletas.RemoveAll(x => x.Semana < semanaAtual - 1);
            var c = j.Coletas.Find(x => x.Semana == semana);
            if (c == null) j.Coletas.Add(c = new FasesColetadas { Semana = semana });
            c.Fases = derrubadas;
            j.CristaisTotais += novas * p.CristaisPorFase;
            j.FragmentosTotais += novas * p.FragmentosPorFase;
            return novas;
        }

        // Tira o membro; sem o líder, quem está há mais tempo assume
        public static void Remover(DadosDaGuilda g, string jogadorId, List<PontosDaSemana> contribuicao, int semanaAtual)
        {
            if (g.Membros.RemoveAll(m => m.JogadorId == jogadorId) == 0) return;
            foreach (var s in contribuicao) if (s.Semana >= semanaAtual - 1) Somar(g.DeQuemSaiu, s.Semana, s.Pontos, 0, semanaAtual);
            if (g.LiderId != jogadorId) return;
            MembroDaGuilda novo = null;
            foreach (var m in g.Membros) if (novo == null || m.EntrouEmUtc < novo.EntrouEmUtc) novo = m;
            g.LiderId = novo?.JogadorId ?? "";
        }

        public static string NomeLimpo(string nome)
        {
            if (nome == null) return "";
            var sb = new System.Text.StringBuilder();
            foreach (char c in nome) if (!char.IsControl(c) && c != '<' && c != '>') sb.Append(c);
            return sb.ToString().Trim();
        }

        public static string ProblemaDoNome(string nome)
        {
            if (nome == null || nome.Trim().Length < 3 || nome.Trim().Length > 24) return "O nome da guilda precisa ter de 3 a 24 letras.";
            return NomeLimpo(nome) == nome.Trim() ? null : "O nome da guilda tem caracteres que não podem ser usados.";
        }

        public static string CodigoLimpo(string codigo) => (codigo ?? "").Trim().ToUpperInvariant();

        public static bool CodigoValido(string codigo)
        {
            if (codigo == null || codigo.Length != TamanhoDoCodigo) return false;
            foreach (char c in codigo) if (LetrasDoCodigo.IndexOf(c) < 0) return false;
            return true;
        }
    }

    public enum AcaoDaGuilda { Ver, Criar, Entrar, Sair, Expulsar, TrocarCodigo, ComecarLuta, TerminarLuta, Coletar }

    [Serializable]
    public class PedidoDeGuilda
    {
        public AcaoDaGuilda Acao;
        public string Texto = "";       // nome, código, jogador a expulsar ou id da luta, conforme a ação
        public ContagemDaLuta Contagem = new ContagemDaLuta();
    }

    [Serializable]
    public class MembroNaLista
    {
        public string JogadorId = "", Nome = "";
        public bool Lider;
        public double Pontos;
        public int Lutas;
    }

    // Resposta do servidor: o que aconteceu e como a guilda está
    [Serializable]
    public class VistaDaGuilda
    {
        public string Erro = "";
        public bool TemGuilda, SouLider;
        public string Nome = "", Codigo = "", Canal = "";   // canal: conversa da guilda
        public List<MembroNaLista> Membros = new List<MembroNaLista>();
        public double SegundosParaVirar;
        public int FasesDerrubadas;
        public double PontosNaFase, VidaDaFase;
        public bool ChefeDerrotado;
        public int LutasRestantes;
        public int FasesAColetar;
        public string LutaId = "";
        public double PontosDaLuta;     // da luta que acabou de terminar
        public long CristaisTotais, FragmentosTotais;
    }

    // O que a arena precisa para a luta contra o chefe da guilda
    public class DesafioDaGuilda
    {
        public string LutaId;
        public int Fase;
        public double PontosNaFase, VidaDaFase;
    }
}
