using System;

namespace ALotOfDamage.Core
{
    public enum EfeitoDoPredio { OuroPorSegundo, Dano, Afk, Ouro }

    // Prédio da taverna própria: sobe de nível com ouro e dá um bônus permanente
    [Serializable]
    public class DefinicaoDePredio
    {
        public string Id;
        public string Nome;
        public string Descricao;
        public EfeitoDoPredio Efeito;
        // OuroPorSegundo: fração do ouro de uma vitória do estágio atual, por segundo e por nível; os outros: bônus por nível
        public double BonusPorNivel = 0.02;
        public double CustoBase = 100;
        public double CrescimentoDoCusto = 1.5;
    }

    public static class RegrasDaTaverna
    {
        public static double Custo(DefinicaoDePredio d, int nivel) => d.CustoBase * Math.Pow(d.CrescimentoDoCusto, Math.Max(0, nivel));

        // O ganho acompanha o ouro do estágio atual, para a taverna não ficar irrelevante com os números gigantes
        public static double OuroPorSegundo(DefinicaoDePredio d, int nivel, double ouroDaVitoria) =>
            d.Efeito == EfeitoDoPredio.OuroPorSegundo ? ouroDaVitoria * d.BonusPorNivel * Math.Max(0, nivel) : 0;

        public static double Acumulado(double ouroPorSegundo, double segundos, double limiteEmHoras) =>
            ouroPorSegundo * Math.Max(0, Math.Min(segundos, limiteEmHoras * 3600));

        public static double Bonus(DefinicaoDePredio d, int nivel) => d.BonusPorNivel * Math.Max(0, nivel);
    }
}
