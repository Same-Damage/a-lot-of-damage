using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    [Serializable]
    public class ParametrosDoBestiario
    {
        public int PrimeiroMarco = 10;
        public int FatorEntreMarcos = 10;
        public double BonusContraTipoPorMarco = 0.25;
        public double BonusGeralPorMarco = 0.02;
    }

    [Serializable]
    public class DefinicaoDeMonstro
    {
        public string Id;
        public string Nome;
        public string MaterialId;
        public string NomeDoMaterial;
        public int MaterialPorVitoria = 1;
        public double MultiplicadorDeVida = 1;
        public string Cor = "#4CE64C";   // placeholder no mapa até chegar o sprite
        public float Escala = 1;
        public FormaDoMonstro Forma = FormaDoMonstro.Gosma;   // corpo low-poly provisório até chegar a arte
    }

    public enum FormaDoMonstro { Gosma, Quadrupede, Golem }

    // Bestiário: derrotar um tipo 10, 100, 1.000... vezes dá bônus permanente contra ele e no geral
    public static class RegrasDoBestiario
    {
        public static int MarcosAtingidos(ParametrosDoBestiario p, int derrotas)
        {
            int marcos = 0;
            for (long marco = p.PrimeiroMarco; derrotas >= marco; marco *= p.FatorEntreMarcos) marcos++;
            return marcos;
        }

        public static long ProximoMarco(ParametrosDoBestiario p, int derrotas) =>
            p.PrimeiroMarco * (long)Math.Pow(p.FatorEntreMarcos, MarcosAtingidos(p, derrotas));

        public static bool AtingiuMarco(ParametrosDoBestiario p, int antes, int depois) =>
            MarcosAtingidos(p, depois) > MarcosAtingidos(p, antes);

        public static double MultiplicadorContraTipo(ParametrosDoBestiario p, int derrotas) =>
            1 + p.BonusContraTipoPorMarco * MarcosAtingidos(p, derrotas);

        public static double MultiplicadorGeral(ParametrosDoBestiario p, IEnumerable<int> derrotasPorTipo)
        {
            int total = 0;
            foreach (int d in derrotasPorTipo) total += MarcosAtingidos(p, d);
            return 1 + p.BonusGeralPorMarco * total;
        }
    }
}
