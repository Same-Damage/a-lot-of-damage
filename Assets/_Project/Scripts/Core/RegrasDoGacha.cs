using System;

namespace ALotOfDamage.Core
{
    public enum EfeitoDoFollower { DanoExtra, PontoFraco, Ouro, Afk, Critico, Drop }

    // Aparência provisória do follower (modelo low-poly gerado por código até chegar a arte)
    public enum FamiliaDoFollower { Humanoide, Besta, Ave, Dragao, Mitico, Espirito, Constructo }

    // Gacha de followers: só com cristais ganhos jogando, taxas públicas e pity
    [Serializable]
    public class ParametrosDoGacha
    {
        public int CustoPorInvocacao = 100;
        public int CustoDeDez = 900;
        public double[] Taxas = { 50, 30, 14, 5, 1 };   // % por raridade, na ordem do enum Raridade
        public int PityEpico = 30;                     // a 30ª seguida sem Épico ou melhor é Épico
        public int PityLendario = 90;
        public double BonusPorEstrela = 0.2;           // cada estrela acima da 1ª soma 20% no efeito e no dano
        public int MaximoDeEstrelas = 5;
        public int CristaisPorEstrelaExtra = 50;       // repetido com estrelas no máximo devolve cristais

        // De onde vêm os cristais (além das conquistas)
        public int CristaisPorChefe = 25;
        public int CristaisDaAscensao = 200;
        public int CristaisPorAlma = 5;
        public int VitoriasAfkPorCristal = 20;
    }

    public static class RegrasDoGacha
    {
        // sorteio em [0, 1); contadores = invocações seguidas sem Épico (ou melhor) e sem Lendário
        public static Raridade SortearRaridade(ParametrosDoGacha p, double sorteio, int semEpico, int semLendario)
        {
            if (semLendario + 1 >= p.PityLendario) return Raridade.Lendario;
            double total = 0;
            foreach (var t in p.Taxas) total += t;
            double alvo = sorteio * total, acumulado = 0;
            var r = (Raridade)(p.Taxas.Length - 1);
            for (int i = 0; i < p.Taxas.Length; i++)
            {
                acumulado += p.Taxas[i];
                if (alvo < acumulado) { r = (Raridade)i; break; }
            }
            if (r < Raridade.Epico && semEpico + 1 >= p.PityEpico) r = Raridade.Epico;
            return r;
        }

        public static (int semEpico, int semLendario) AposInvocar(Raridade r, int semEpico, int semLendario) =>
            (r >= Raridade.Epico ? 0 : semEpico + 1, r == Raridade.Lendario ? 0 : semLendario + 1);

        public static double MultiplicadorDasEstrelas(ParametrosDoGacha p, int estrelas) =>
            1 + p.BonusPorEstrela * (Math.Min(Math.Max(1, estrelas), p.MaximoDeEstrelas) - 1);

        // Efeito do follower, se for do tipo pedido; 0 sem follower ou de outro tipo
        public static double Efeito(ParametrosDoGacha p, DefinicaoDeFollower f, int estrelas, EfeitoDoFollower tipo) =>
            f == null || f.Efeito != tipo || tipo == EfeitoDoFollower.DanoExtra ? 0 : f.ValorDoEfeito * MultiplicadorDasEstrelas(p, estrelas);

        // Parte do auto-ataque que o follower causa a cada ataque: cresce com as estrelas; o efeito DanoExtra soma por cima
        public static double FracaoDoAutoAtaque(ParametrosDoGacha p, DefinicaoDeFollower f, int estrelas)
        {
            if (f == null) return 0;
            double extra = f.Efeito == EfeitoDoFollower.DanoExtra ? f.ValorDoEfeito : 0;
            return (f.FracaoDoAutoAtaque + extra) * MultiplicadorDasEstrelas(p, estrelas);
        }
    }
}
