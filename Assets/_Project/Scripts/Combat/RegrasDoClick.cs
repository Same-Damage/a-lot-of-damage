using System;
using System.Collections.Generic;

namespace ALotOfDamage.Combat
{
    public struct ParametrosDoClick
    {
        public double DanoBase;
        public double BonusDeComboPorHit;
        public double MultiplicadorCritico;
        public double MultiplicadorPontoFraco;
    }

    // Fórmulas puras do combate clicker
    public static class RegrasDoClick
    {
        public static double Dano(ParametrosDoClick p, int combo, bool critico, bool pontoFraco)
        {
            double dano = p.DanoBase * (1 + combo * p.BonusDeComboPorHit);
            if (pontoFraco) dano *= p.MultiplicadorPontoFraco;
            if (critico) dano *= p.MultiplicadorCritico;
            return dano;
        }

        public static double VidaDoInimigo(double vidaBase, double crescimento, int estagio) =>
            vidaBase * Math.Pow(crescimento, estagio - 1);
    }

    // Limite de clicks por segundo (anti-autoclicker)
    public class LimitadorDeCliques
    {
        readonly int maximoPorSegundo;
        readonly Queue<float> tempos = new Queue<float>();

        public LimitadorDeCliques(int maximoPorSegundo) => this.maximoPorSegundo = maximoPorSegundo;

        public bool Registrar(float agora)
        {
            while (tempos.Count > 0 && agora - tempos.Peek() > 1f) tempos.Dequeue();
            if (tempos.Count >= maximoPorSegundo) return false;
            tempos.Enqueue(agora);
            return true;
        }
    }
}
