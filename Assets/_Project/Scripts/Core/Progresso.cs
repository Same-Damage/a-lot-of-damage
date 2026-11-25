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

        public long Almas { get; private set; }
        public int Ascensoes { get; private set; }

        // Moeda do gacha, ganha só jogando
        public long Cristais { get; private set; }

        // Contadores das conquistas (nunca zeram, nem na Ascensão)
        public int ChefesDerrotados { get; private set; }
        public int EstagioMaximo { get; private set; } = 1;
        public int ItensEpicosObtidos { get; private set; }
        public int MaiorRefino { get; private set; }
        public int ItensFabricados { get; private set; }
        public double OuroTotalGanho { get; private set; }

        // Followers que o jogador tem e as estrelas de cada um; o Pip vem de início
        readonly Dictionary<string, int> followers = new Dictionary<string, int> { [FollowerPadrao] = 1 };
        public int Estrelas(string followerId) => Ler(followers, followerId);
        public IReadOnlyDictionary<string, int> Followers => followers;
        public int InvocacoesSemEpico { get; private set; }
        public int InvocacoesSemLendario { get; private set; }

        // Novo follower (1 estrela) ou +1 estrela no repetido; false se já estava no máximo
        public bool ReceberFollower(string id, int maximoDeEstrelas)
        {
            int estrelas = Estrelas(id);
            if (estrelas >= maximoDeEstrelas) return false;
            followers[id] = estrelas + 1;
            return true;
        }

        public void RegistrarPity(int semEpico, int semLendario)
        {
            InvocacoesSemEpico = Math.Max(0, semEpico);
            InvocacoesSemLendario = Math.Max(0, semLendario);
        }

        public bool EscolherFollower(string id)
        {
            if (Estrelas(id) <= 0 || EmExpedicao(id)) return false;
            FollowerAtivoId = id;
            return true;
        }

        readonly Dictionary<string, int> conquistasColetadas = new Dictionary<string, int>();
        public int ConquistasColetadas(string id) => Ler(conquistasColetadas, id);
        public int TotalDeConquistasColetadas
        {
            get { int n = 0; foreach (var v in conquistasColetadas.Values) n += v; return n; }
        }

        public void ReceberCristais(long quantidade) { if (quantidade > 0) Cristais += quantidade; }
        public bool GastarCristais(long quantidade)
        {
            if (quantidade < 0 || quantidade > Cristais) return false;
            Cristais -= quantidade;
            return true;
        }
        public void ContarChefe() => ChefesDerrotados++;
        public int RecursosColetados { get; private set; }
        public void ContarColeta() => RecursosColetados++;

        // Relíquias: fragmentos de chefe e o nível de cada relíquia (0 = não montada); ficam na Ascensão
        public long Fragmentos { get; private set; }

        // Expedições em andamento (o follower fica fora: não pode ser o ativo nem ir em outra)
        readonly List<ExpedicaoEmAndamento> expedicoes = new List<ExpedicaoEmAndamento>();
        public IReadOnlyList<ExpedicaoEmAndamento> Expedicoes => expedicoes;
        public bool EmExpedicao(string followerId) => expedicoes.Exists(e => e.FollowerId == followerId);

        public bool IniciarExpedicao(string missaoId, string followerId, DateTime agoraUtc, int vagas)
        {
            if (expedicoes.Count >= vagas || Estrelas(followerId) <= 0 || followerId == FollowerAtivoId || EmExpedicao(followerId)) return false;
            expedicoes.Add(new ExpedicaoEmAndamento { MissaoId = missaoId, FollowerId = followerId, InicioUtc = agoraUtc.Ticks });
            return true;
        }

        public ExpedicaoEmAndamento EncerrarExpedicao(int indice)
        {
            if (indice < 0 || indice >= expedicoes.Count) return null;
            var e = expedicoes[indice];
            expedicoes.RemoveAt(indice);
            return e;
        }

        // Taverna própria: nível de cada prédio e quando o ouro guardado foi coletado (ticks UTC; 0 = nunca)
        readonly Dictionary<string, int> predios = new Dictionary<string, int>();
        public int NivelDoPredio(string id) => Ler(predios, id);
        public long UltimaColetaDaTavernaUtc { get; private set; }
        public void MarcarColetaDaTaverna(DateTime agoraUtc) => UltimaColetaDaTavernaUtc = agoraUtc.Ticks;

        public bool MelhorarPredio(string id, double custo)
        {
            if (custo > Ouro) return false;
            Ouro -= custo;
            predios[id] = NivelDoPredio(id) + 1;
            return true;
        }
        readonly Dictionary<string, int> reliquias = new Dictionary<string, int>();
        public int NivelDaReliquia(string id) => Ler(reliquias, id);
        public void ReceberFragmentos(long n) { if (n > 0) Fragmentos += n; }

        // Sobe um nível pagando o custo; false se não tem fragmentos
        public bool MelhorarReliquia(string id, int custo)
        {
            if (custo < 0 || custo > Fragmentos) return false;
            Fragmentos -= custo;
            reliquias[id] = NivelDaReliquia(id) + 1;
            return true;
        }
        public void ContarFabricacao() => ItensFabricados++;
        public void MarcarConquistaColetada(string id, int degraus) => conquistasColetadas[id] = Math.Max(ConquistasColetadas(id), degraus);

        // Última vez que o jogo estava aberto (ticks UTC); 0 = nunca. É daí que conta o tempo fora (AFK)
        public long UltimaPresencaUtc { get; private set; }

        public void MarcarPresenca(DateTime agoraUtc) => UltimaPresencaUtc = agoraUtc.Ticks;

        // Recomeça estágio, upgrades e ouro; mantém itens, materiais, bestiário e o resto
        public void Ascender(long almasGanhas)
        {
            Almas += Math.Max(0, almasGanhas);
            Ascensoes++;
            Estagio = 1;
            VitoriasNoEstagio = 0;
            Ouro = 0;
            niveis.Clear();
        }
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
            if (quantidade <= 0) return;
            Ouro += quantidade;
            OuroTotalGanho += quantidade;
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
            if (raridade >= Raridade.Epico) ItensEpicosObtidos++;
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

        // Raridade maior primeiro, depois a ordem dos slots, o refino maior e a ordem em que chegou
        public void OrganizarInventario() => inventario.Sort((a, b) =>
        {
            int c = b.Raridade.CompareTo(a.Raridade);
            if (c == 0) c = a.Slot.CompareTo(b.Slot);
            if (c == 0) c = b.Nivel.CompareTo(a.Nivel);
            return c != 0 ? c : a.Id.CompareTo(b.Id);
        });

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
            MaiorRefino = Math.Max(MaiorRefino, item.Nivel);
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
            EstagioMaximo = Math.Max(EstagioMaximo, Estagio);
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
            UltimaPresencaUtc = UltimaPresencaUtc,
            Almas = Almas, Ascensoes = Ascensoes, Cristais = Cristais,
            ChefesDerrotados = ChefesDerrotados, EstagioMaximo = EstagioMaximo, ItensEpicosObtidos = ItensEpicosObtidos,
            MaiorRefino = MaiorRefino, ItensFabricados = ItensFabricados, OuroTotalGanho = OuroTotalGanho,
            ConquistasColetadas = Pares(conquistasColetadas),
            Fragmentos = Fragmentos, Reliquias = Pares(reliquias),
            Predios = Pares(predios), UltimaColetaDaTavernaUtc = UltimaColetaDaTavernaUtc,
            Expedicoes = new List<ExpedicaoEmAndamento>(expedicoes),
            RecursosColetados = RecursosColetados,
            Followers = Pares(followers), InvocacoesSemEpico = InvocacoesSemEpico, InvocacoesSemLendario = InvocacoesSemLendario,
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
            p.UltimaPresencaUtc = Math.Max(0, d.UltimaPresencaUtc);
            p.Almas = Math.Max(0, d.Almas);
            p.Ascensoes = Math.Max(0, d.Ascensoes);
            p.Cristais = Math.Max(0, d.Cristais);
            p.ChefesDerrotados = Math.Max(0, d.ChefesDerrotados);
            p.EstagioMaximo = Math.Max(p.Estagio, d.EstagioMaximo);
            p.ItensEpicosObtidos = Math.Max(0, d.ItensEpicosObtidos);
            p.MaiorRefino = Math.Max(0, d.MaiorRefino);
            p.ItensFabricados = Math.Max(0, d.ItensFabricados);
            // save de antes das conquistas: o ouro guardado é o mínimo que já foi ganho
            p.OuroTotalGanho = Math.Max(p.Ouro, d.OuroTotalGanho);
            Preencher(p.conquistasColetadas, d.ConquistasColetadas);
            Preencher(p.followers, d.Followers);
            p.Fragmentos = Math.Max(0, d.Fragmentos);
            Preencher(p.reliquias, d.Reliquias);
            Preencher(p.predios, d.Predios);
            p.RecursosColetados = Math.Max(0, d.RecursosColetados);
            if (d.Expedicoes != null) foreach (var e in d.Expedicoes) if (e != null && !string.IsNullOrEmpty(e.FollowerId)) p.expedicoes.Add(e);
            p.UltimaColetaDaTavernaUtc = Math.Max(0, d.UltimaColetaDaTavernaUtc);
            if (p.Estrelas(FollowerPadrao) <= 0) p.followers[FollowerPadrao] = 1;
            p.RegistrarPity(d.InvocacoesSemEpico, d.InvocacoesSemLendario);
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
        public long UltimaPresencaUtc;
        public long Almas;
        public int Ascensoes;
        public long Cristais;
        public int ChefesDerrotados;
        public int EstagioMaximo = 1;
        public int ItensEpicosObtidos;
        public int MaiorRefino;
        public int ItensFabricados;
        public double OuroTotalGanho;
        public List<ParDeValor> ConquistasColetadas = new List<ParDeValor>();
        public List<ParDeValor> Followers = new List<ParDeValor>();
        public long Fragmentos;
        public List<ParDeValor> Reliquias = new List<ParDeValor>();
        public List<ParDeValor> Predios = new List<ParDeValor>();
        public long UltimaColetaDaTavernaUtc;
        public List<ExpedicaoEmAndamento> Expedicoes = new List<ExpedicaoEmAndamento>();
        public int RecursosColetados;
        public int InvocacoesSemEpico;
        public int InvocacoesSemLendario;
        public List<ParDeValor> Materiais = new List<ParDeValor>();
        public List<ParDeValor> Derrotas = new List<ParDeValor>();
    }
}
