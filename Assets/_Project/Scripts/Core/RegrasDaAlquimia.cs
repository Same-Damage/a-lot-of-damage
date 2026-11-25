using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    public enum EfeitoDaPocao { Dano, Ouro, Drop, Critico, Afk }

    // Poção da alquimia: ingredientes da coleta e dos monstros, mais ouro; bebida na hora, com efeito por um tempo
    [Serializable]
    public class DefinicaoDePocao
    {
        public string Id;
        public string Nome;
        public string Descricao;
        public EfeitoDaPocao Efeito;
        public double Valor = 1;                 // Dano: +100% (×2); crítico: pontos de chance; os outros: +%
        public double DuracaoEmMinutos = 10;     // 0 = vale até ser usada (a do AFK acaba na próxima coleta)
        public double CustoEmVitorias = 4;       // ouro = esse tanto de vitórias do estágio atual
        public List<ParDeValor> Ingredientes = new List<ParDeValor>();
    }

    public static class RegrasDaAlquimia
    {
        public static bool Ativa(long fimEmTicks, DateTime agoraUtc) => fimEmTicks > agoraUtc.Ticks;

        // Beber de novo soma o tempo ao que faltava
        public static long NovoFim(DefinicaoDePocao p, long fimAtual, DateTime agoraUtc)
        {
            if (p.DuracaoEmMinutos <= 0) return long.MaxValue;
            long inicio = Math.Max(fimAtual, agoraUtc.Ticks);
            return inicio + TimeSpan.FromMinutes(p.DuracaoEmMinutos).Ticks;
        }
    }
}
