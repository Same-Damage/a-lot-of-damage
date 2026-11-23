using System;

namespace ALotOfDamage.Core
{
    public enum AlvoDoItem { Click, Automatico }

    [Serializable]
    public class DefinicaoDeItem
    {
        public string Id;
        public string Nome;
        public string Descricao;
        public AlvoDoItem Alvo;
        public double BonusPorNivel = 0.25;
        public double CustoBaseEmOuro = 100;
        public double CrescimentoDoCusto = 1.5;
        public string MaterialId;
        public int MaterialPorNivel = 2;
        public double ChanceDeSucessoCritico = 0.1;
    }

    public struct ResultadoDaForja
    {
        public bool Forjou;
        public bool SucessoCritico;
        public int NovoNivel;
        public double OuroRestante;
        public int MaterialRestante;
    }

    // Forja: cada nível do item multiplica o dano; o item começa em +0
    public static class RegrasDaForja
    {
        public static double Multiplicador(DefinicaoDeItem d, int nivel) => Math.Pow(1 + d.BonusPorNivel, nivel);

        public static double CustoEmOuro(DefinicaoDeItem d, int nivel) => d.CustoBaseEmOuro * Math.Pow(d.CrescimentoDoCusto, nivel);

        public static int CustoEmMaterial(DefinicaoDeItem d, int nivel) => d.MaterialPorNivel * (nivel + 1);

        // sorteio em [0, 1): abaixo da chance, o sucesso crítico sobe dois níveis
        public static ResultadoDaForja Forjar(DefinicaoDeItem d, int nivel, double ouro, int material, double sorteio)
        {
            double custo = CustoEmOuro(d, nivel);
            int custoMaterial = CustoEmMaterial(d, nivel);
            if (ouro < custo || material < custoMaterial)
                return new ResultadoDaForja { NovoNivel = nivel, OuroRestante = ouro, MaterialRestante = material };

            bool critico = sorteio < d.ChanceDeSucessoCritico;
            return new ResultadoDaForja
            {
                Forjou = true,
                SucessoCritico = critico,
                NovoNivel = nivel + (critico ? 2 : 1),
                OuroRestante = ouro - custo,
                MaterialRestante = material - custoMaterial,
            };
        }
    }
}
