using System;

namespace ALotOfDamage.Core
{
    // Como o poder aparece no visual, sem modelo novo
    public static class RegrasDeAparencia
    {
        // Nível de refino em que o item ganha metade do brilho que ainda falta
        public const int NivelDeMeioBrilho = 10;

        // Brilho de cada raridade sem refino, na ordem do enum Raridade
        static readonly float[] BrilhoDaRaridade = { 0f, 0.2f, 0.4f, 0.6f, 0.8f };

        // Brilho do item de 0 a quase 1: a raridade dá a base e o refino (+N) soma por cima, sem nunca estourar
        public static float BrilhoDoItem(Raridade raridade, int nivel)
        {
            float baseDaRaridade = BrilhoDaRaridade[Math.Min((int)raridade, BrilhoDaRaridade.Length - 1)];
            int n = Math.Max(0, nivel);
            return baseDaRaridade + (1 - baseDaRaridade) * n / (n + NivelDeMeioBrilho);
        }
    }
}
