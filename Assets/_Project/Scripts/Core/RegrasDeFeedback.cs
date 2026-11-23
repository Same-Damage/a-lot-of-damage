namespace ALotOfDamage.Core
{
    public enum TipoDeGolpe { Automatico, Follower, Click, PontoFraco, Critico, Ultimate }

    // Intensidade do feedback de cada golpe: quanto mais forte, mais pausa, partícula e tremor
    public static class RegrasDeFeedback
    {
        // Hit stop: micro pausa do jogo no impacto, em segundos de tempo real
        public static float HitStop(TipoDeGolpe g)
        {
            switch (g)
            {
                case TipoDeGolpe.Ultimate: return 0.12f;
                case TipoDeGolpe.Critico: return 0.05f;
                case TipoDeGolpe.PontoFraco: return 0.03f;
                default: return 0f;
            }
        }

        public static int Particulas(TipoDeGolpe g)
        {
            switch (g)
            {
                case TipoDeGolpe.Ultimate: return 60;
                case TipoDeGolpe.Critico: return 25;
                case TipoDeGolpe.PontoFraco: return 18;
                case TipoDeGolpe.Click: return 8;
                default: return 3;
            }
        }

        public static float Tremor(TipoDeGolpe g)
        {
            switch (g)
            {
                case TipoDeGolpe.Ultimate: return 0.3f;
                case TipoDeGolpe.Critico: return 0.15f;
                case TipoDeGolpe.PontoFraco: return 0.12f;
                case TipoDeGolpe.Click: return 0.08f;
                default: return 0f;
            }
        }

        // Um golpe mais forte não é cortado por um mais fraco que chega durante a pausa
        public static TipoDeGolpe MaisForte(TipoDeGolpe a, TipoDeGolpe b) => a >= b ? a : b;
    }
}
