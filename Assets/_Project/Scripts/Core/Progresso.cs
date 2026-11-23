using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Entidade com tudo que o jogador conquistou. Quem guarda e carrega é o IRepositorioDeProgresso.
    public class Progresso
    {
        public const string ClassePadrao = "guerreiro";
        public const string FollowerPadrao = "pip";

        public double Ouro { get; private set; }
        public int Vitorias { get; private set; }

        // Por enquanto só existe o Guerreiro e um follower; a escolha e o gacha vêm depois
        public string ClasseId { get; private set; } = ClassePadrao;
        public string FollowerAtivoId { get; private set; } = FollowerPadrao;

        readonly Dictionary<string, int> niveis = new Dictionary<string, int>();
        readonly Dictionary<string, int> niveisDosItens = new Dictionary<string, int>();
        readonly Dictionary<string, int> materiais = new Dictionary<string, int>();
        readonly Dictionary<string, int> derrotas = new Dictionary<string, int>();

        static int Ler(Dictionary<string, int> d, string id, int padrao = 0) =>
            id != null && d.TryGetValue(id, out var n) ? n : padrao;

        public int Nivel(string idDoUpgrade) => Ler(niveis, idDoUpgrade, 1);
        public int NivelDoItem(string id) => Ler(niveisDosItens, id);
        public int Material(string id) => Ler(materiais, id);
        public int Derrotas(string tipo) => Ler(derrotas, tipo);
        public IEnumerable<int> TodasAsDerrotas => derrotas.Values;

        public double Valor(DefinicaoDeUpgrade d) => RegrasDeUpgrade.Valor(d, Nivel(d.Id));

        public void ReceberOuro(double quantidade)
        {
            if (quantidade > 0) Ouro += quantidade;
        }

        public void AdicionarMaterial(string id, int quantidade)
        {
            if (string.IsNullOrEmpty(id) || quantidade <= 0) return;
            materiais[id] = Material(id) + quantidade;
        }

        public ResultadoDaCompra Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            var r = RegrasDeUpgrade.Comprar(d, Nivel(d.Id), Ouro, quantidade);
            if (!r.Comprou) return r;
            niveis[d.Id] = r.NovoNivel;
            Ouro = r.OuroRestante;
            return r;
        }

        public ResultadoDaForja Forjar(DefinicaoDeItem d, double sorteio)
        {
            var r = RegrasDaForja.Forjar(d, NivelDoItem(d.Id), Ouro, Material(d.MaterialId), sorteio);
            if (!r.Forjou) return r;
            niveisDosItens[d.Id] = r.NovoNivel;
            Ouro = r.OuroRestante;
            materiais[d.MaterialId] = r.MaterialRestante;
            return r;
        }

        // Conta a vitória no bestiário e entrega o material do monstro. Devolve true se atingiu um marco.
        public bool RegistrarVitoria(DefinicaoDeMonstro m, ParametrosDoBestiario p)
        {
            Vitorias++;
            if (m == null) return false;
            int antes = Derrotas(m.Id);
            derrotas[m.Id] = antes + 1;
            AdicionarMaterial(m.MaterialId, m.MaterialPorVitoria);
            return RegrasDoBestiario.AtingiuMarco(p, antes, antes + 1);
        }

        // Forja × bestiário contra o tipo × bestiário geral: as fontes se multiplicam
        public double MultiplicadorDeDano(AlvoDoItem alvo, string tipo, IEnumerable<DefinicaoDeItem> itens, ParametrosDoBestiario p)
        {
            double m = 1;
            foreach (var item in itens)
                if (item.Alvo == alvo) m *= RegrasDaForja.Multiplicador(item, NivelDoItem(item.Id));
            m *= RegrasDoBestiario.MultiplicadorContraTipo(p, Derrotas(tipo));
            return m * RegrasDoBestiario.MultiplicadorGeral(p, derrotas.Values);
        }
    }
}
