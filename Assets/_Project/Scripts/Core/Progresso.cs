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
        readonly List<ItemDoJogador> inventario = new List<ItemDoJogador>();
        readonly Dictionary<SlotDeItem, ItemDoJogador> equipados = new Dictionary<SlotDeItem, ItemDoJogador>();
        int proximoIdDeItem = 1;
        readonly Dictionary<string, int> materiais = new Dictionary<string, int>();
        readonly Dictionary<string, int> derrotas = new Dictionary<string, int>();

        static int Ler(Dictionary<string, int> d, string id, int padrao = 0) =>
            id != null && d.TryGetValue(id, out var n) ? n : padrao;

        public int Nivel(string idDoUpgrade) => Ler(niveis, idDoUpgrade, 1);
        // Itens guardados (sem os equipados)
        public IReadOnlyList<ItemDoJogador> Inventario => inventario;
        public IEnumerable<ItemDoJogador> Equipados => equipados.Values;
        public ItemDoJogador Equipado(SlotDeItem slot) => equipados.TryGetValue(slot, out var i) ? i : null;
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

        // Tira ouro e material juntos, só se tiver os dois
        public bool Gastar(double ouro, string materialId, int material)
        {
            if (ouro > Ouro || material > Material(materialId)) return false;
            Ouro -= ouro;
            if (material > 0) materiais[materialId] = Material(materialId) - material;
            return true;
        }

        // Item novo no inventário (+0); null se o inventário estiver cheio
        public ItemDoJogador ReceberItem(string tipoId, SlotDeItem slot, Raridade raridade, int estagio, int limite)
        {
            if (inventario.Count >= limite) return null;
            var item = new ItemDoJogador { Id = proximoIdDeItem++, TipoId = tipoId, Slot = slot, Raridade = raridade, Estagio = Math.Max(1, estagio) };
            inventario.Add(item);
            return item;
        }

        public ItemDoJogador BuscarItem(int id)
        {
            foreach (var i in inventario) if (i.Id == id) return i;
            foreach (var i in equipados.Values) if (i.Id == id) return i;
            return null;
        }

        // Tira um item guardado (para vender ou desmontar); item equipado não sai por aqui
        public ItemDoJogador RemoverDoInventario(int id)
        {
            int i = inventario.FindIndex(x => x.Id == id);
            if (i < 0) return null;
            var item = inventario[i];
            inventario.RemoveAt(i);
            return item;
        }

        // Equipa um item guardado no slot dele; o que estava no slot volta para o inventário
        public bool Equipar(int id)
        {
            var item = RemoverDoInventario(id);
            if (item == null) return false;
            if (equipados.TryGetValue(item.Slot, out var antigo)) inventario.Add(antigo);
            equipados[item.Slot] = item;
            return true;
        }

        public bool Desequipar(SlotDeItem slot, int limite)
        {
            if (!equipados.TryGetValue(slot, out var item) || inventario.Count >= limite) return false;
            equipados.Remove(slot);
            inventario.Add(item);
            return true;
        }

        // Refino na forja: sobe o +N do próprio item, esteja guardado ou equipado
        public ResultadoDaForja Refinar(int id, DefinicaoDeItem tipo, double sorteio)
        {
            var item = BuscarItem(id);
            if (item == null || tipo == null) return new ResultadoDaForja { OuroRestante = Ouro };
            var r = RegrasDaForja.Forjar(tipo, item.Nivel, Ouro, Material(tipo.MaterialId), sorteio);
            if (!r.Forjou) return r;
            item.Nivel = r.NovoNivel;
            Ouro = r.OuroRestante;
            materiais[tipo.MaterialId] = r.MaterialRestante;
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
            Niveis = Pares(niveis), Materiais = Pares(materiais), Derrotas = Pares(derrotas),
            Inventario = new List<ItemDoJogador>(inventario), Equipados = new List<ItemDoJogador>(equipados.Values),
            ProximoIdDeItem = proximoIdDeItem,
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
            Preencher(p.materiais, d.Materiais);
            Preencher(p.derrotas, d.Derrotas);
            ImportarItens(p, d);
            return p;
        }

        // Tipos da forja antiga (um nível por tipo) e o slot em que viram item
        static readonly Dictionary<string, SlotDeItem> SlotsDaForjaAntiga = new Dictionary<string, SlotDeItem>
        {
            ["espada"] = SlotDeItem.Arma, ["manopla"] = SlotDeItem.Luvas, ["anel"] = SlotDeItem.Anel,
        };

        static void ImportarItens(Progresso p, DadosDoProgresso d)
        {
            int maiorId = 0;
            if (d.Inventario != null)
                foreach (var i in d.Inventario) if (i != null) { p.inventario.Add(i); maiorId = Math.Max(maiorId, i.Id); }
            if (d.Equipados != null)
                foreach (var i in d.Equipados) if (i != null) { p.equipados[i.Slot] = i; maiorId = Math.Max(maiorId, i.Id); }
            p.proximoIdDeItem = Math.Max(d.ProximoIdDeItem, maiorId + 1);

            // save de antes dos itens: cada nível da forja vira um item comum equipado com o mesmo +N
            if (d.NiveisDosItens == null || maiorId > 0) return;
            foreach (var par in d.NiveisDosItens)
            {
                if (par.Valor <= 0 || !SlotsDaForjaAntiga.TryGetValue(par.Id ?? "", out var slot)) continue;
                var item = p.ReceberItem(par.Id, slot, Raridade.Comum, 1, int.MaxValue);
                item.Nivel = par.Valor;
                p.Equipar(item.Id);
            }
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

        public static DefinicaoDeItem Tipo(IEnumerable<DefinicaoDeItem> tipos, string id)
        {
            foreach (var t in tipos) if (t.Id == id) return t;
            return null;
        }

        // Itens equipados × bestiário contra o tipo × bestiário geral: as fontes se multiplicam
        public double MultiplicadorDeDano(AlvoDoItem alvo, string tipo, IEnumerable<DefinicaoDeItem> tipos, ParametrosDosItens itens, ParametrosDoBestiario p)
        {
            double m = 1;
            foreach (var item in equipados.Values)
            {
                var t = Tipo(tipos, item.TipoId);
                if (t != null && t.Alvo == alvo) m *= RegrasDosItens.Multiplicador(itens, t, item);
            }
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
        // forja antiga (um nível por tipo), lida só para converter saves de antes dos itens
        public List<ParDeValor> NiveisDosItens = new List<ParDeValor>();
        public List<ItemDoJogador> Inventario = new List<ItemDoJogador>();
        public List<ItemDoJogador> Equipados = new List<ItemDoJogador>();
        public int ProximoIdDeItem = 1;
        public List<ParDeValor> Materiais = new List<ParDeValor>();
        public List<ParDeValor> Derrotas = new List<ParDeValor>();
    }
}
