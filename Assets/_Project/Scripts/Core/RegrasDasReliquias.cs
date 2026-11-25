using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    public enum AlvoDaReliquia { Click, Automatico, Follower, Ouro, Afk, Critico, Drop, PontoFraco }

    // Relíquia única, montada com fragmentos de chefe e melhorada até o nível máximo
    [Serializable]
    public class DefinicaoDeReliquia
    {
        public string Id;
        public string Nome;
        public string Descricao;
        public AlvoDaReliquia Alvo;
        public double BonusPorNivel = 0.5;   // multiplicadores somam por nível (+50% por nível); crítico soma em pontos
        public int CustoParaMontar = 20;
    }

    [Serializable]
    public class ParametrosDasReliquias
    {
        public int FragmentosPorChefe = 3;
        public int EstagiosPorFragmentoExtra = 10;   // +1 fragmento por chefe a cada 10 estágios
        public int NivelMaximo = 10;
        public int CustoBaseDeMelhoria = 10;          // do nível n para n + 1 custa base × n
    }

    public static class RegrasDasReliquias
    {
        public static int FragmentosDoChefe(ParametrosDasReliquias p, int estagio) =>
            p.FragmentosPorChefe + (p.EstagiosPorFragmentoExtra > 0 ? estagio / p.EstagiosPorFragmentoExtra : 0);

        // Custo para ir do nível atual ao próximo (0 → 1 é montar); −1 no nível máximo
        public static int Custo(ParametrosDasReliquias p, DefinicaoDeReliquia r, int nivel)
        {
            if (nivel >= p.NivelMaximo) return -1;
            return nivel <= 0 ? r.CustoParaMontar : p.CustoBaseDeMelhoria * nivel;
        }

        public static double Bonus(DefinicaoDeReliquia r, int nivel) => r.BonusPorNivel * Math.Max(0, nivel);

        // Relíquias do mesmo alvo se multiplicam (fontes de poder se multiplicam)
        public static double Multiplicador(IEnumerable<DefinicaoDeReliquia> reliquias, Func<string, int> nivel, AlvoDaReliquia alvo)
        {
            double m = 1;
            foreach (var r in reliquias) if (r.Alvo == alvo) m *= 1 + Bonus(r, nivel(r.Id));
            return m;
        }

        // Para os bônus que somam (crítico em pontos; ouro, AFK, drop e ponto fraco em %): soma direta
        public static double Soma(IEnumerable<DefinicaoDeReliquia> reliquias, Func<string, int> nivel, AlvoDaReliquia alvo)
        {
            double s = 0;
            foreach (var r in reliquias) if (r.Alvo == alvo) s += Bonus(r, nivel(r.Id));
            return s;
        }
    }
}
