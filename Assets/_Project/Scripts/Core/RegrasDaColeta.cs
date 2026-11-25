using System;

namespace ALotOfDamage.Core
{
    public enum FormaDoRecurso { Erva, Cogumelo, Minerio }

    // Ponto de coleta no mapa: golpear algumas vezes (mini-clicker) dá material para a alquimia
    [Serializable]
    public class DefinicaoDeRecurso
    {
        public string Id;
        public string Nome;
        public string MaterialId;
        public string NomeDoMaterial;
        public FormaDoRecurso Forma;
        public string Cor = "#7AD88A";
        public int Golpes = 6;
        public int QuantidadeBase = 2;
        public float SegundosParaRebrotar = 45;
    }

    public static class RegrasDaColeta
    {
        public static int Quantidade(DefinicaoDeRecurso r, int estagio) => r.QuantidadeBase + Math.Max(0, estagio) / 10;

        public static bool Esgotou(DefinicaoDeRecurso r, int golpesDados) => golpesDados >= Math.Max(1, r.Golpes);
    }
}
