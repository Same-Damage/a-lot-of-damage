using System;

namespace ALotOfDamage.Core
{
    public enum TipoDeUpgrade { ComMarcos, Linear }

    [Serializable]
    public class DefinicaoDeUpgrade
    {
        public string Id;
        public string Nome;
        public string Descricao;
        public double CustoBase = 10;
        public double CrescimentoDoCusto = 1.07;
        public double ValorBase = 1;
        public TipoDeUpgrade Tipo = TipoDeUpgrade.ComMarcos;

        [UnityEngine.Tooltip("Só para upgrades lineares")]
        public double ValorPorNivel;
        [UnityEngine.Tooltip("Só para upgrades lineares; 0 = sem teto")]
        public double Teto;
    }

    public struct ResultadoDaCompra
    {
        public bool Comprou;
        public int NovoNivel;
        public double OuroRestante;
        public bool AtingiuMarco;
    }

    // Fórmulas puras dos upgrades: custo geométrico, valor com marcos multiplicativos
    public static class RegrasDeUpgrade
    {
        public const int Maximo = int.MaxValue;

        // Custo para ir do nível atual para o próximo (o nível começa em 1)
        public static double Custo(DefinicaoDeUpgrade d, int nivelAtual) =>
            d.CustoBase * Math.Pow(d.CrescimentoDoCusto, nivelAtual - 1);

        public static double CustoDeVarios(DefinicaoDeUpgrade d, int nivelAtual, int quantidade)
        {
            double r = d.CrescimentoDoCusto;
            return Custo(d, nivelAtual) * (Math.Pow(r, quantidade) - 1) / (r - 1);
        }

        public static int QuantosCabem(DefinicaoDeUpgrade d, int nivelAtual, double ouro)
        {
            double r = d.CrescimentoDoCusto;
            double primeiro = Custo(d, nivelAtual);
            if (ouro < primeiro) return 0;

            int k = (int)Math.Floor(Math.Log(ouro * (r - 1) / primeiro + 1) / Math.Log(r));
            // Ajuste fino contra erro de ponto flutuante
            while (CustoDeVarios(d, nivelAtual, k + 1) <= ouro) k++;
            while (k > 0 && CustoDeVarios(d, nivelAtual, k) > ouro) k--;
            return k;
        }

        // ×2 a cada 10 níveis, ×4 a cada 25, ×10 a cada 100 (acumulam)
        public static double MultiplicadorDeMarcos(int nivel) =>
            Math.Pow(2, nivel / 10) * Math.Pow(4, nivel / 25) * Math.Pow(10, nivel / 100);

        public static int ProximoMarco(int nivel) =>
            Math.Min(10 * (nivel / 10 + 1), 25 * (nivel / 25 + 1));

        // Último marco atingido (0 se nenhum)
        public static int MarcoAnterior(int nivel) =>
            Math.Max(10 * (nivel / 10), 25 * (nivel / 25));

        public static double Valor(DefinicaoDeUpgrade d, int nivel)
        {
            if (d.Tipo == TipoDeUpgrade.Linear)
            {
                double v = d.ValorBase + d.ValorPorNivel * (nivel - 1);
                return d.Teto > 0 ? Math.Min(v, d.Teto) : v;
            }
            return d.ValorBase * nivel * MultiplicadorDeMarcos(nivel);
        }

        public static double GanhoRelativo(DefinicaoDeUpgrade d, int nivel)
        {
            double atual = Valor(d, nivel);
            return atual <= 0 ? 0 : Valor(d, nivel + 1) / atual - 1;
        }

        public static bool NoMaximo(DefinicaoDeUpgrade d, int nivel) =>
            NivelMaximo(d) is int max && nivel >= max;

        static int? NivelMaximo(DefinicaoDeUpgrade d)
        {
            if (d.Tipo != TipoDeUpgrade.Linear || d.Teto <= 0 || d.ValorPorNivel <= 0) return null;
            return (int)Math.Ceiling((d.Teto - d.ValorBase) / d.ValorPorNivel - 1e-9) + 1;
        }

        public static ResultadoDaCompra Comprar(DefinicaoDeUpgrade d, int nivelAtual, double ouro, int quantidade)
        {
            var nada = new ResultadoDaCompra { NovoNivel = nivelAtual, OuroRestante = ouro };

            int limite = NivelMaximo(d) is int max ? Math.Max(0, max - nivelAtual) : int.MaxValue;
            int cabem = Math.Min(QuantosCabem(d, nivelAtual, ouro), limite);

            int q = quantidade == Maximo ? cabem : quantidade;
            if (q <= 0 || q > cabem) return nada;

            int novo = nivelAtual + q;
            return new ResultadoDaCompra
            {
                Comprou = true,
                NovoNivel = novo,
                OuroRestante = ouro - CustoDeVarios(d, nivelAtual, q),
                AtingiuMarco = d.Tipo == TipoDeUpgrade.ComMarcos && ProximoMarco(nivelAtual) <= novo,
            };
        }
    }
}
