using System;

namespace ALotOfDamage.Core
{
    [Serializable]
    public class DefinicaoDeClasse
    {
        public string Id;
        public string Nome;
        public string NomeDaPassiva;
        public double BonusDeFuriaPorAcumulo = 0.05;
        public int CombosPorAcumulo = 10;      // 0 = classe sem fúria
        public int MaximoDeAcumulos = 10;
        public string NomeDaUltimate;
        public double MultiplicadorDaUltimate = 50;
    }

    [Serializable]
    public class DefinicaoDeFollower
    {
        public string Id;
        public string Nome;
        public string Descricao;
        public Raridade Raridade;
        public double FracaoDoAutoAtaque = 0.2;   // parte do auto-ataque que ele causa a cada ataque
        public float IntervaloDeAtaque = 1.5f;
        public EfeitoDoFollower Efeito = EfeitoDoFollower.PontoFraco;
        public double ValorDoEfeito = 0.3;         // com 1 estrela; RegrasDoGacha.Efeito aplica as estrelas

        // Modelo provisório gerado por código
        public FamiliaDoFollower Familia = FamiliaDoFollower.Ave;
        public string Cor = "#8A6A4A";
        public string CorSecundaria = "#E8D8B0";
        public float Escala = 1;
    }

    // Regras da classe (passiva de fúria) e do follower de suporte
    public static class RegrasDePersonagem
    {
        public static int AcumulosDeFuria(DefinicaoDeClasse c, int combo) =>
            c == null || c.CombosPorAcumulo <= 0 ? 0 : Math.Min(combo / c.CombosPorAcumulo, c.MaximoDeAcumulos);

        public static double MultiplicadorDeFuria(DefinicaoDeClasse c, int combo) =>
            1 + (c?.BonusDeFuriaPorAcumulo ?? 0) * AcumulosDeFuria(c, combo);

        public static double DanoDoFollower(DefinicaoDeFollower f, double danoAutomatico) =>
            f == null ? 0 : danoAutomatico * f.FracaoDoAutoAtaque;

        // bonus vem do efeito PontoFraco do follower ativo (AtributosDeCombate.BonusNoPontoFraco)
        public static float DuracaoDoPontoFraco(double bonus, float duracaoBase) => (float)(duracaoBase * (1 + Math.Max(0, bonus)));
    }
}
