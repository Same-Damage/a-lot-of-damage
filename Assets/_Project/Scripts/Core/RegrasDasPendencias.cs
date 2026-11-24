using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Como as pendências aparecem no HUD: selo com a quantidade em cada ícone e um resumo de uma linha
    public static class RegrasDasPendencias
    {
        public const int MaiorNumeroNoSelo = 9;

        public static int Quantas(List<Pendencia> lista, TipoDePendencia tipo)
        {
            int n = 0;
            for (int i = 0; i < lista.Count; i++) if (lista[i].Tipo == tipo) n++;
            return n;
        }

        // Vazio quando não há nada a mostrar
        public static string Selo(int quantas) =>
            quantas <= 0 ? "" : quantas > MaiorNumeroNoSelo ? MaiorNumeroNoSelo + "+" : quantas.ToString();

        // A pendência mais importante (a primeira) e quantas mais existem
        public static string Resumo(List<Pendencia> lista) =>
            lista.Count == 0 ? "" : lista.Count == 1 ? lista[0].Texto : $"{lista[0].Texto}  +{lista.Count - 1}";
    }
}
