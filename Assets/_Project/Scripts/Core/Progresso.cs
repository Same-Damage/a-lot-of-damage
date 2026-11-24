using System;
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
        // Modelo do herói no mapa e na batalha; só aparência, não muda a classe. Vazio = o padrão do jogo
        public string HeroiId { get; private set; } = "";

        public int Estagio { get; private set; } = 1;
        public int VitoriasNoEstagio { get; private set; }

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

        public void TrocarHeroi(string id) => HeroiId = id ?? "";

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

        public ProgressoDaCampanha AvancarCampanha(ParametrosDaCampanha p, bool chefe)
        {
            var r = RegrasDaCampanha.AposVitoria(p, Estagio, VitoriasNoEstagio, chefe);
            Estagio = r.Estagio;
            VitoriasNoEstagio = r.VitoriasNoEstagio;
            return r;
        }

        // Cópia simples de tudo, para salvar em arquivo ou mandar ao servidor
        public DadosDoProgresso Exportar() => new DadosDoProgresso
        {
            Ouro = Ouro, Vitorias = Vitorias, ClasseId = ClasseId, FollowerAtivoId = FollowerAtivoId, HeroiId = HeroiId,
            Estagio = Estagio, VitoriasNoEstagio = VitoriasNoEstagio,
            Niveis = Pares(niveis), NiveisDosItens = Pares(niveisDosItens), Materiais = Pares(materiais), Derrotas = Pares(derrotas),
        };

        public static Progresso Importar(DadosDoProgresso d)
        {
            var p = new Progresso();
            if (d == null) return p;
            p.Ouro = Math.Max(0, d.Ouro);
            p.Vitorias = Math.Max(0, d.Vitorias);
            if (!string.IsNullOrEmpty(d.ClasseId)) p.ClasseId = d.ClasseId;
            if (!string.IsNullOrEmpty(d.FollowerAtivoId)) p.FollowerAtivoId = d.FollowerAtivoId;
            p.HeroiId = d.HeroiId ?? "";
            p.Estagio = Math.Max(1, d.Estagio);
            p.VitoriasNoEstagio = Math.Max(0, d.VitoriasNoEstagio);
            Preencher(p.niveis, d.Niveis);
            Preencher(p.niveisDosItens, d.NiveisDosItens);
            Preencher(p.materiais, d.Materiais);
            Preencher(p.derrotas, d.Derrotas);
            return p;
        }

        static List<ParDeValor> Pares(Dictionary<string, int> d)
        {
            var lista = new List<ParDeValor>(d.Count);
            foreach (var kv in d) lista.Add(new ParDeValor { Id = kv.Key, Valor = kv.Value });
            return lista;
        }

        static void Preencher(Dictionary<string, int> d, List<ParDeValor> pares)
        {
            if (pares == null) return;
            foreach (var par in pares) if (!string.IsNullOrEmpty(par.Id)) d[par.Id] = par.Valor;
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

namespace ALotOfDamage.Core
{
    [Serializable]
    public class ParDeValor
    {
        public string Id;
        public int Valor;
    }

    // Formato salvo do progresso (campos públicos para o serializador de JSON)
    [Serializable]
    public class DadosDoProgresso
    {
        public double Ouro;
        public int Vitorias;
        public string ClasseId;
        public string FollowerAtivoId;
        public string HeroiId;
        public int Estagio = 1;
        public int VitoriasNoEstagio;
        public List<ParDeValor> Niveis = new List<ParDeValor>();
        public List<ParDeValor> NiveisDosItens = new List<ParDeValor>();
        public List<ParDeValor> Materiais = new List<ParDeValor>();
        public List<ParDeValor> Derrotas = new List<ParDeValor>();
    }
}
