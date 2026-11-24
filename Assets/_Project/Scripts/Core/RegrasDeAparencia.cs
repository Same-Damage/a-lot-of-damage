using System;

namespace ALotOfDamage.Core
{
    // Como o poder aparece no visual, sem modelo novo
    public static class RegrasDeAparencia
    {
        // Nível da forja em que a arma chega à metade do brilho máximo
        public const int NivelDeMeioBrilho = 10;

        // Brilho da arma de 0 (sem forja) até perto de 1, crescendo rápido no começo e sem nunca estourar
        public static float BrilhoDaArma(int nivelDaForja)
        {
            int n = Math.Max(0, nivelDaForja);
            return n / (float)(n + NivelDeMeioBrilho);
        }
    }
}
