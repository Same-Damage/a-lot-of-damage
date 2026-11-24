using System;

namespace ALotOfDamage.Core
{
    // Recompensa AFK: quantas lutas o herói venceria sozinho no tempo fora, só com auto-ataque e follower
    public static class RegrasDoAfk
    {
        public static double SegundosQueContam(ParametrosDaCampanha p, double segundosFora) =>
            Math.Max(0, Math.Min(segundosFora, p.LimiteDoAfkEmHoras * 3600));

        // Vitórias (com fração): tempo ÷ (duração da luta + pausa entre lutas), vezes a fração do AFK
        public static double Vitorias(ParametrosDaCampanha p, double segundos, double danoPorSegundo, double vidaDoInimigo)
        {
            if (danoPorSegundo <= 0 || segundos <= 0) return 0;
            double porLuta = vidaDoInimigo / danoPorSegundo + p.SegundosEntreLutasNoAfk;
            return segundos / porLuta * p.FracaoDoAfk;
        }

        public static int Itens(double vitorias, double chanceDeDrop) => (int)Math.Floor(Math.Max(0, vitorias) * chanceDeDrop);
    }
}
