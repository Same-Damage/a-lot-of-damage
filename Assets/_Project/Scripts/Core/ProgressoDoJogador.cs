using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Progresso do jogador entre cenas (provisório até existir save/servidor)
    public static class ProgressoDoJogador
    {
        public static double Ouro;
        public static int Vitorias;

        static readonly Dictionary<string, int> niveis = new Dictionary<string, int>();

        public static int Nivel(string id) => niveis.TryGetValue(id, out var n) ? n : 1;

        public static double Valor(DefinicaoDeUpgrade d) => RegrasDeUpgrade.Valor(d, Nivel(d.Id));

        public static ResultadoDaCompra Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            var r = RegrasDeUpgrade.Comprar(d, Nivel(d.Id), Ouro, quantidade);
            if (!r.Comprou) return r;
            niveis[d.Id] = r.NovoNivel;
            Ouro = r.OuroRestante;
            return r;
        }

        public static void Reiniciar()
        {
            Ouro = 0;
            Vitorias = 0;
            niveis.Clear();
        }
    }
}
