using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Progresso do jogador entre cenas (provisório até existir save/servidor)
    public static class ProgressoDoJogador
    {
        public static double Ouro;
        public static int Vitorias;

        static readonly Dictionary<string, int> niveis = new Dictionary<string, int>();
        static readonly Dictionary<string, int> niveisDosItens = new Dictionary<string, int>();
        static readonly Dictionary<string, int> materiais = new Dictionary<string, int>();
        static readonly Dictionary<string, int> derrotas = new Dictionary<string, int>();

        static int Ler(Dictionary<string, int> d, string id, int padrao = 0) =>
            id != null && d.TryGetValue(id, out var n) ? n : padrao;

        public static int Nivel(string id) => Ler(niveis, id, 1);
        public static int NivelDoItem(string id) => Ler(niveisDosItens, id);
        public static int Material(string id) => Ler(materiais, id);
        public static int Derrotas(string tipo) => Ler(derrotas, tipo);

        public static double Valor(DefinicaoDeUpgrade d) => RegrasDeUpgrade.Valor(d, Nivel(d.Id));

        public static ResultadoDaCompra Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            var r = RegrasDeUpgrade.Comprar(d, Nivel(d.Id), Ouro, quantidade);
            if (!r.Comprou) return r;
            niveis[d.Id] = r.NovoNivel;
            Ouro = r.OuroRestante;
            return r;
        }

        public static void AdicionarMaterial(string id, int quantidade)
        {
            if (string.IsNullOrEmpty(id) || quantidade <= 0) return;
            materiais[id] = Material(id) + quantidade;
        }

        public static ResultadoDaForja Forjar(DefinicaoDeItem d, double sorteio)
        {
            var r = RegrasDaForja.Forjar(d, NivelDoItem(d.Id), Ouro, Material(d.MaterialId), sorteio);
            if (!r.Forjou) return r;
            niveisDosItens[d.Id] = r.NovoNivel;
            Ouro = r.OuroRestante;
            materiais[d.MaterialId] = r.MaterialRestante;
            return r;
        }

        // Conta a vitória no bestiário e entrega o material do monstro. Devolve true se atingiu um marco.
        public static bool RegistrarVitoria(DefinicaoDeMonstro m, ParametrosDoBestiario p)
        {
            int antes = Derrotas(m.Id);
            derrotas[m.Id] = antes + 1;
            AdicionarMaterial(m.MaterialId, m.MaterialPorVitoria);
            return RegrasDoBestiario.AtingiuMarco(p, antes, antes + 1);
        }

        // Forja × bestiário contra o tipo × bestiário geral: as fontes se multiplicam
        public static double MultiplicadorDeDano(AlvoDoItem alvo, string tipo, IEnumerable<DefinicaoDeItem> itens, ParametrosDoBestiario p)
        {
            double m = 1;
            foreach (var item in itens)
                if (item.Alvo == alvo) m *= RegrasDaForja.Multiplicador(item, NivelDoItem(item.Id));
            m *= RegrasDoBestiario.MultiplicadorContraTipo(p, Derrotas(tipo));
            return m * RegrasDoBestiario.MultiplicadorGeral(p, derrotas.Values);
        }

        public static void Reiniciar()
        {
            Ouro = 0;
            Vitorias = 0;
            niveis.Clear();
            niveisDosItens.Clear();
            materiais.Clear();
            derrotas.Clear();
        }
    }
}
