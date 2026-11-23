using System;
using System.Globalization;

namespace ALotOfDamage.Core
{
    // Poder Total: um número que resume a força do jogador, e como mostrar quanto ele cresceu
    public static class RegrasDePoder
    {
        public const double MultiplicadorCriticoPadrao = 5;

        static readonly CultureInfo Br = new CultureInfo("pt-BR");

        // Dano por segundo de referência: um click + o auto-ataque + o follower, com o crítico médio
        public static double Poder(AtributosDeCombate a, double multiplicadorCritico)
        {
            double fatorCritico = 1 + a.ChanceCritica * (multiplicadorCritico - 1);
            double follower = a.Follower != null && a.Follower.IntervaloDeAtaque > 0
                ? a.DanoDoFollower / a.Follower.IntervaloDeAtaque : 0;
            return (a.DanoDoClick + a.DanoAutomatico + follower) * fatorCritico;
        }

        // "+38%" para ganhos pequenos, "×4" a partir do dobro; null quando não cresceu
        public static string TextoDoGanho(double antes, double depois)
        {
            if (antes <= 0 || depois <= antes) return null;
            double razao = depois / antes;
            if (razao < 2) return "+" + ((razao - 1) * 100).ToString(razao < 1.1 ? "0.#" : "0", Br) + "%";
            return "×" + (razao < 1000 ? razao.ToString("0.#", Br) : FormatadorDeNumeros.Formatar(razao));
        }

        // Aproxima o valor mostrado do alvo em escala logarítmica, para subir de 1 K a 1 T sem demorar.
        // rapidez: fração da distância (em log) percorrida por segundo é 1 - e^(-rapidez·dt).
        public static double ContadorAnimado(double atual, double alvo, double dt, double rapidez)
        {
            double fracao = 1 - Math.Exp(-rapidez * dt);
            double novo;
            if (atual <= 0 || alvo <= 0)
                novo = atual + (alvo - atual) * fracao;
            else
                novo = Math.Exp(Math.Log(atual) + (Math.Log(alvo) - Math.Log(atual)) * fracao);

            // Encosta no alvo quando falta menos de 0,1%
            return Math.Abs(alvo - novo) <= Math.Abs(alvo) * 1e-3 ? alvo : novo;
        }
    }
}
