using System;

namespace ALotOfDamage.Core
{
    // Missão de expedição: um follower fora de uso sai por algumas horas e volta com recompensa
    [Serializable]
    public class DefinicaoDeMissao
    {
        public string Id;
        public string Nome;
        public double Horas = 1;
        public int Cristais = 5;
        public int Lutas = 10;               // material como se vencesse esse tanto de lutas na região atual
        public double ChanceDeItem = 0.2;    // acima de 1: itens garantidos (parte inteira) + chance do resto
    }

    [Serializable]
    public class ExpedicaoEmAndamento
    {
        public string MissaoId;
        public string FollowerId;
        public long InicioUtc;   // ticks
    }

    public static class RegrasDasExpedicoes
    {
        // Raridade (+50% por degrau acima de Comum) e estrelas aumentam tudo o que a expedição traz
        public static double Fator(ParametrosDoGacha g, DefinicaoDeFollower f, int estrelas) =>
            (1 + 0.5 * (int)f.Raridade) * RegrasDoGacha.MultiplicadorDasEstrelas(g, estrelas);

        public static double SegundosRestantes(DefinicaoDeMissao m, double segundosPassados) =>
            Math.Max(0, m.Horas * 3600 - Math.Max(0, segundosPassados));

        public static int Itens(double chance, double sorteio) => (int)Math.Floor(chance) + (sorteio < chance - Math.Floor(chance) ? 1 : 0);
    }
}
