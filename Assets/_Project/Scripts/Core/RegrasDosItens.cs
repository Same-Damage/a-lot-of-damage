using System;

namespace ALotOfDamage.Core
{
    public enum SlotDeItem { Arma, Elmo, Peitoral, Luvas, Botas, Cinto, Anel, Colar }

    public enum Raridade { Comum, Incomum, Raro, Epico, Lendario }

    // Um item que o jogador tem (no inventário ou equipado). O tipo (nome, slot, refino) vem da DefinicaoDeItem do catálogo.
    [Serializable]
    public class ItemDoJogador
    {
        public int Id;
        public string TipoId;
        public SlotDeItem Slot;
        public Raridade Raridade;
        public int Estagio = 1;   // estágio em que caiu ou foi fabricado: itens de estágios altos são mais fortes
        public int Nivel;         // refino (+N), começa em +0
    }

    // Balanceamento dos itens (asset BalanceamentoDaForja)
    [Serializable]
    public class ParametrosDosItens
    {
        public double ChanceDeDrop = 0.25;
        // peso de cada raridade no sorteio, na ordem do enum Raridade
        public double[] PesosDaRaridade = { 60, 25, 10, 4, 1 };
        public Raridade RaridadeMinimaDoChefe = Raridade.Incomum;
        // bônus de dano do item no estágio 1, por raridade (0,25 = +25%)
        public double[] BonusDaRaridade = { 0.1, 0.25, 0.5, 1, 2 };
        public double CrescimentoPorEstagio = 1.05;
        public double CrescimentoDoPreco = 1.15;
        public int LimiteDoInventario = 48;
        public double CustoDeFabricarEmOuro = 200;
        public int MaterialParaFabricar = 5;
        public double[] PrecoDeVenda = { 10, 30, 90, 300, 1000 };
        public int[] MaterialAoDesmontar = { 1, 2, 4, 8, 16 };
    }

    public static class RegrasDosItens
    {
        public static readonly string[] NomesDasRaridades = { "Comum", "Incomum", "Raro", "Épico", "Lendário" };

        static T Da<T>(T[] tabela, Raridade r) => tabela[Math.Min((int)r, tabela.Length - 1)];

        // sorteio em [0, 1)
        public static bool CaiItem(ParametrosDosItens p, double sorteio, bool chefe) => chefe || sorteio < p.ChanceDeDrop;

        public static Raridade SortearRaridade(ParametrosDosItens p, double sorteio, bool chefe)
        {
            double total = 0;
            foreach (var peso in p.PesosDaRaridade) total += peso;
            double alvo = sorteio * total, acumulado = 0;
            var r = (Raridade)(p.PesosDaRaridade.Length - 1);
            for (int i = 0; i < p.PesosDaRaridade.Length; i++)
            {
                acumulado += p.PesosDaRaridade[i];
                if (alvo < acumulado) { r = (Raridade)i; break; }
            }
            return chefe && r < p.RaridadeMinimaDoChefe ? p.RaridadeMinimaDoChefe : r;
        }

        public static int SortearTipo(int quantidade, double sorteio) =>
            Math.Max(0, Math.Min(quantidade - 1, (int)(sorteio * quantidade)));

        // Raridade e estágio dão o bônus base; o refino multiplica por cima, como a forja antiga
        public static double Multiplicador(ParametrosDosItens p, DefinicaoDeItem tipo, ItemDoJogador item)
        {
            double baseDoItem = 1 + Da(p.BonusDaRaridade, item.Raridade) * Math.Pow(p.CrescimentoPorEstagio, Math.Max(0, item.Estagio - 1));
            return baseDoItem * RegrasDaForja.Multiplicador(tipo, item.Nivel);
        }

        public static double PrecoDeVenda(ParametrosDosItens p, ItemDoJogador item) =>
            Da(p.PrecoDeVenda, item.Raridade) * Math.Pow(p.CrescimentoDoPreco, Math.Max(0, item.Estagio - 1)) * (1 + 0.5 * item.Nivel);

        public static int MaterialAoDesmontar(ParametrosDosItens p, ItemDoJogador item) =>
            Da(p.MaterialAoDesmontar, item.Raridade) + item.Nivel;

        public static double CustoDeFabricar(ParametrosDosItens p, int estagio) =>
            p.CustoDeFabricarEmOuro * Math.Pow(p.CrescimentoDoPreco, Math.Max(0, estagio - 1));

        public static string Nome(Raridade r) => NomesDasRaridades[(int)r];
    }
}
