using System;

namespace ALotOfDamage.Core
{
    // Ascensão (prestígio): a partir de um estágio, troca estágio, upgrades e ouro por Almas
    public static class RegrasDaAscensao
    {
        public static bool PodeAscender(ParametrosDaCampanha p, int estagio) => estagio >= p.EstagioMinimoDaAscensao;

        // Cresce com o quadrado da distância até a base: ir mais longe antes de ascender rende muito mais
        public static long AlmasGanhas(ParametrosDaCampanha p, int estagio)
        {
            if (!PodeAscender(p, estagio)) return 0;
            double distancia = estagio - p.EstagioBaseDasAlmas;
            return (long)Math.Floor(distancia * distancia / Math.Max(1e-9, p.DivisorDasAlmas));
        }

        public static double Multiplicador(ParametrosDaCampanha p, long almas) => 1 + p.BonusPorAlma * Math.Max(0, almas);
    }
}
