using System;

namespace ALotOfDamage.Core
{
    public enum MetaDaConquista
    {
        Vitorias, ChefesDerrotados, EstagioMaximo, Ascensoes,
        ItensEpicosObtidos, MaiorRefino, ItensFabricados, OuroTotalGanho, MarcosDoBestiario,
    }

    // Uma meta com degraus crescentes; cada degrau passado é uma conquista para coletar
    [Serializable]
    public class DefinicaoDeConquista
    {
        public string Id;
        public string Nome;
        public string Descricao;   // "{0}" vira o valor do próximo degrau
        public MetaDaConquista Meta;
        public double[] Degraus = { };
    }

    [Serializable]
    public class ParametrosDasConquistas
    {
        public double BonusPorConquista = 0.01;   // cada conquista coletada soma 1% em todo o dano
        public int CristaisBase = 10;             // degrau n (de 0) dá CristaisBase × (n + 1)²
    }

    public static class RegrasDasConquistas
    {
        public static int DegrausAtingidos(DefinicaoDeConquista d, double valor)
        {
            int n = 0;
            while (n < d.Degraus.Length && valor >= d.Degraus[n]) n++;
            return n;
        }

        // NaN quando todos os degraus já foram feitos
        public static double ProximaMeta(DefinicaoDeConquista d, int degrausFeitos) =>
            degrausFeitos < d.Degraus.Length ? d.Degraus[degrausFeitos] : double.NaN;

        public static int Cristais(ParametrosDasConquistas p, int indiceDoDegrau) => p.CristaisBase * (indiceDoDegrau + 1) * (indiceDoDegrau + 1);

        public static double Multiplicador(ParametrosDasConquistas p, int coletadas) => 1 + p.BonusPorConquista * Math.Max(0, coletadas);

        // Valor atual da meta no progresso do jogador
        public static double Valor(MetaDaConquista meta, Progresso p, ParametrosDoBestiario bestiario)
        {
            switch (meta)
            {
                case MetaDaConquista.Vitorias: return p.Vitorias;
                case MetaDaConquista.ChefesDerrotados: return p.ChefesDerrotados;
                case MetaDaConquista.EstagioMaximo: return p.EstagioMaximo;
                case MetaDaConquista.Ascensoes: return p.Ascensoes;
                case MetaDaConquista.ItensEpicosObtidos: return p.ItensEpicosObtidos;
                case MetaDaConquista.MaiorRefino: return p.MaiorRefino;
                case MetaDaConquista.ItensFabricados: return p.ItensFabricados;
                case MetaDaConquista.OuroTotalGanho: return p.OuroTotalGanho;
                default:
                    int marcos = 0;
                    foreach (var d in p.TodasAsDerrotas) marcos += RegrasDoBestiario.MarcosAtingidos(bestiario, d);
                    return marcos;
            }
        }
    }
}
