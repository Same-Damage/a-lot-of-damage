using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Ids dos upgrades que o combate usa
    public static class IdsDosUpgrades
    {
        public const string Forca = "forca_do_golpe";
        public const string Automatico = "ataque_automatico";
        public const string Precisao = "precisao";
    }

    public struct AtributosDeCombate
    {
        public double DanoDoClick;
        public double DanoAutomatico;
        public double ChanceCritica;
        public DefinicaoDeClasse Classe;        // null se o catálogo não tiver a classe do jogador
        public DefinicaoDeFollower Follower;    // null sem follower ativo
        public double DanoDoFollower;
    }

    public struct ResultadoDaVitoria
    {
        public double Ouro;
        public DefinicaoDeMonstro Monstro;   // null quando a luta não veio de um monstro do catálogo
        public int MaterialGanho;
        public bool AtingiuMarcoDoBestiario;
        public int Derrotas;
        public bool AvancouEstagio;
        public int Estagio;
        public ItemDoJogador ItemGanho;            // null se não caiu item
        public string NomeDoItem;
        public bool ItemVendidoPorFaltaDeEspaco;   // inventário cheio: o item virou ouro na hora
    }

    public struct ResultadoDaFabricacao
    {
        public bool Fabricou;
        public ItemDoJogador Item;
    }

    public struct InimigoDaBatalha
    {
        public string Nome;
        public double Vida;
        public double Ouro;
        public bool Chefe;
        public FormaDoMonstro Forma;
        public string Cor;
    }

    public struct SituacaoDaCampanha
    {
        public int Estagio;
        public int VitoriasNoEstagio;
        public int VitoriasParaAvancar;
        public bool EstagioDeChefe;
        public DefinicaoDeRegiao Regiao;
    }

    // Base dos casos de uso: carregar o progresso, aplicar a regra, salvar
    public abstract class CasoDeUso
    {
        protected readonly IRepositorioDeProgresso repositorio;
        protected readonly ICatalogo catalogo;

        protected CasoDeUso(IRepositorioDeProgresso repositorio, ICatalogo catalogo)
        {
            this.repositorio = repositorio;
            this.catalogo = catalogo;
        }
    }

    // Qual herói aparece no mapa e na batalha, entre os que o jogo tem
    public class EscolhaDeHeroi : CasoDeUso
    {
        public EscolhaDeHeroi(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public string Atual(IReadOnlyList<string> herois)
        {
            string id = repositorio.Carregar().HeroiId;
            for (int i = 0; i < herois.Count; i++) if (herois[i] == id) return id;
            return herois.Count > 0 ? herois[0] : "";
        }

        // Passa para o próximo da lista, voltando ao primeiro no fim
        public string Trocar(IReadOnlyList<string> herois)
        {
            if (herois.Count == 0) return "";
            int i = 0;
            string atual = Atual(herois);
            while (herois[i] != atual) i++;
            string proximo = herois[(i + 1) % herois.Count];
            var p = repositorio.Carregar();
            p.TrocarHeroi(proximo);
            repositorio.Salvar(p);
            return proximo;
        }
    }

    public class ComprarUpgrade : CasoDeUso
    {
        public ComprarUpgrade(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public ResultadoDaCompra Executar(string idDoUpgrade, int quantidade)
        {
            var d = catalogo.Upgrade(idDoUpgrade);
            var p = repositorio.Carregar();
            if (d == null) return new ResultadoDaCompra { OuroRestante = p.Ouro };
            var r = p.Comprar(d, quantidade);
            if (r.Comprou) repositorio.Salvar(p);
            return r;
        }
    }

    // Refino na forja: sobe o +N de um item do jogador
    public class RefinarItem : CasoDeUso
    {
        readonly ISorteio sorteio;

        public RefinarItem(IRepositorioDeProgresso r, ICatalogo c, ISorteio sorteio) : base(r, c) => this.sorteio = sorteio;

        public ResultadoDaForja Executar(int idDoItem)
        {
            var p = repositorio.Carregar();
            var item = p.BuscarItem(idDoItem);
            var tipo = item == null ? null : catalogo.Item(item.TipoId);
            if (tipo == null) return new ResultadoDaForja { OuroRestante = p.Ouro };
            var r = p.Refinar(idDoItem, tipo, sorteio.Proximo());
            if (r.Forjou) repositorio.Salvar(p);
            return r;
        }
    }

    // Forja fabrica um item novo do tipo escolhido, com raridade sorteada, no estágio atual
    public class FabricarItem : CasoDeUso
    {
        readonly ISorteio sorteio;

        public FabricarItem(IRepositorioDeProgresso r, ICatalogo c, ISorteio sorteio) : base(r, c) => this.sorteio = sorteio;

        public ResultadoDaFabricacao Executar(string tipoId)
        {
            var tipo = catalogo.Item(tipoId);
            var p = repositorio.Carregar();
            var regras = catalogo.ParametrosDosItens;
            if (tipo == null || p.Inventario.Count >= regras.LimiteDoInventario) return default;
            if (!p.Gastar(RegrasDosItens.CustoDeFabricar(regras, p.Estagio), tipo.MaterialId, regras.MaterialParaFabricar)) return default;
            var raridade = RegrasDosItens.SortearRaridade(regras, sorteio.Proximo(), false);
            var item = p.ReceberItem(tipo.Id, tipo.Slot, raridade, p.Estagio, regras.LimiteDoInventario);
            p.ContarFabricacao();
            repositorio.Salvar(p);
            return new ResultadoDaFabricacao { Fabricou = true, Item = item };
        }
    }

    // Vende um item guardado; devolve o ouro recebido (0 se não vendeu)
    public class VenderItem : CasoDeUso
    {
        public VenderItem(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public double Executar(int idDoItem)
        {
            var p = repositorio.Carregar();
            var item = p.RemoverDoInventario(idDoItem);
            if (item == null) return 0;
            double ouro = RegrasDosItens.PrecoDeVenda(catalogo.ParametrosDosItens, item);
            p.ReceberOuro(ouro);
            repositorio.Salvar(p);
            return ouro;
        }
    }

    // Desmonta um item guardado em material da forja do tipo dele; devolve a quantidade (0 se não desmontou)
    public class DesmontarItem : CasoDeUso
    {
        public DesmontarItem(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public int Executar(int idDoItem)
        {
            var p = repositorio.Carregar();
            var item = p.BuscarItem(idDoItem);
            var tipo = item == null ? null : catalogo.Item(item.TipoId);
            if (tipo == null || p.RemoverDoInventario(idDoItem) == null) return 0;
            int material = RegrasDosItens.MaterialAoDesmontar(catalogo.ParametrosDosItens, item);
            p.AdicionarMaterial(tipo.MaterialId, material);
            repositorio.Salvar(p);
            return material;
        }
    }

    public struct DetalhesDoItem
    {
        public string Nome;              // null se o item não existe
        public string NomeDaRaridade;
        public Raridade Raridade;
        public SlotDeItem Slot;
        public AlvoDoItem Alvo;
        public int Nivel;
        public double Multiplicador;
        public double PrecoDeVenda;
        public int MaterialAoDesmontar;
        public string Material;
        public bool Equipado;
    }

    // O que a tela mostra de um item: nome, raridade, bônus e quanto vale vender ou desmontar
    public class ConsultarItem : CasoDeUso
    {
        public ConsultarItem(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public DetalhesDoItem Executar(int idDoItem)
        {
            var p = repositorio.Carregar();
            var item = p.BuscarItem(idDoItem);
            var tipo = item == null ? null : catalogo.Item(item.TipoId);
            if (tipo == null) return default;
            var regras = catalogo.ParametrosDosItens;
            return new DetalhesDoItem
            {
                Nome = tipo.Nome,
                NomeDaRaridade = RegrasDosItens.Nome(item.Raridade),
                Raridade = item.Raridade,
                Slot = item.Slot,
                Alvo = tipo.Alvo,
                Nivel = item.Nivel,
                Multiplicador = RegrasDosItens.Multiplicador(regras, tipo, item),
                PrecoDeVenda = RegrasDosItens.PrecoDeVenda(regras, item),
                MaterialAoDesmontar = RegrasDosItens.MaterialAoDesmontar(regras, item),
                Material = catalogo.NomeDoMaterial(tipo.MaterialId),
                Equipado = p.Equipado(item.Slot) == item,
            };
        }
    }

    public class OrganizarInventario : CasoDeUso
    {
        public OrganizarInventario(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public void Executar()
        {
            var p = repositorio.Carregar();
            p.OrganizarInventario();
            repositorio.Salvar(p);
        }
    }

    public class EquiparItem : CasoDeUso
    {
        public EquiparItem(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public bool Executar(int idDoItem)
        {
            var p = repositorio.Carregar();
            if (!p.Equipar(idDoItem)) return false;
            repositorio.Salvar(p);
            return true;
        }
    }

    public class DesequiparItem : CasoDeUso
    {
        public DesequiparItem(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public bool Executar(SlotDeItem slot)
        {
            var p = repositorio.Carregar();
            if (!p.Desequipar(slot, catalogo.ParametrosDosItens.LimiteDoInventario)) return false;
            repositorio.Salvar(p);
            return true;
        }
    }

    public class RegistrarVitoria : CasoDeUso
    {
        readonly ISorteio sorteio;

        public RegistrarVitoria(IRepositorioDeProgresso r, ICatalogo c, ISorteio sorteio) : base(r, c) => this.sorteio = sorteio;

        // tipoDoMonstro pode ser null (ex.: cena de Batalha aberta direto, sem encontro)
        public ResultadoDaVitoria Executar(string tipoDoMonstro, bool chefe)
        {
            var p = repositorio.Carregar();
            var monstro = tipoDoMonstro == null ? null : catalogo.Monstro(tipoDoMonstro);
            double ouro = CalcularInimigo.Para(p, catalogo, monstro, chefe).Ouro;
            p.ReceberOuro(ouro);
            bool marco = p.RegistrarVitoria(monstro, catalogo.Bestiario);
            if (chefe) p.ContarChefe();
            int estagioDaLuta = p.Estagio;
            var campanha = p.AvancarCampanha(catalogo.Campanha, chefe);
            var item = DropDeItens.Talvez(p, catalogo, sorteio, chefe, estagioDaLuta, out bool vendido);
            repositorio.Salvar(p);
            return new ResultadoDaVitoria
            {
                ItemGanho = item,
                NomeDoItem = item == null ? null : catalogo.Item(item.TipoId)?.Nome,
                ItemVendidoPorFaltaDeEspaco = vendido,
                Ouro = ouro,
                Monstro = monstro,
                MaterialGanho = monstro?.MaterialPorVitoria ?? 0,
                AtingiuMarcoDoBestiario = marco,
                Derrotas = monstro == null ? 0 : p.Derrotas(monstro.Id),
                AvancouEstagio = campanha.Avancou,
                Estagio = campanha.Estagio,
            };
        }

    }

    // Sorteio e entrega de itens: vitória na arena e recompensa AFK
    static class DropDeItens
    {
        // Talvez caia um item (chefe sempre); devolve null se não caiu
        public static ItemDoJogador Talvez(Progresso p, ICatalogo catalogo, ISorteio sorteio, bool chefe, int estagio, out bool vendido)
        {
            vendido = false;
            if (catalogo.Itens.Count == 0 || !RegrasDosItens.CaiItem(catalogo.ParametrosDosItens, sorteio.Proximo(), chefe)) return null;
            return Entregar(p, catalogo, sorteio, chefe, estagio, out vendido);
        }

        // O item é do estágio em que caiu; com o inventário cheio ele é vendido na hora
        public static ItemDoJogador Entregar(Progresso p, ICatalogo catalogo, ISorteio sorteio, bool chefe, int estagio, out bool vendido)
        {
            vendido = false;
            var regras = catalogo.ParametrosDosItens;
            var tipo = catalogo.Itens[RegrasDosItens.SortearTipo(catalogo.Itens.Count, sorteio.Proximo())];
            var raridade = RegrasDosItens.SortearRaridade(regras, sorteio.Proximo(), chefe);
            var item = p.ReceberItem(tipo.Id, tipo.Slot, raridade, estagio, regras.LimiteDoInventario);
            if (item != null) return item;
            vendido = true;
            item = new ItemDoJogador { TipoId = tipo.Id, Slot = tipo.Slot, Raridade = raridade, Estagio = estagio };
            p.ReceberOuro(RegrasDosItens.PrecoDeVenda(regras, item));
            return item;
        }
    }

    public struct RecompensaAfk
    {
        public bool TemRecompensa;     // tempo fora passou do mínimo e há o que coletar
        public double Segundos;        // tempo que conta (até o limite)
        public long Vitorias;
        public double Ouro;
        public List<ParDeValor> Materiais;
        public int Itens;
    }

    // Marca que o jogo está aberto agora: o tempo fora (AFK) conta a partir daqui
    public class RegistrarPresenca : CasoDeUso
    {
        readonly IRelogio relogio;

        public RegistrarPresenca(IRepositorioDeProgresso r, ICatalogo c, IRelogio relogio) : base(r, c) => this.relogio = relogio;

        public void Executar()
        {
            var p = repositorio.Carregar();
            p.MarcarPresenca(relogio.AgoraUtc);
            repositorio.Salvar(p);
        }
    }

    // O que o herói ganhou lutando sozinho desde a última presença (sem aplicar nada)
    public class ConsultarRecompensaAfk : CasoDeUso
    {
        readonly IRelogio relogio;
        readonly CalcularAtributos atributos;

        public ConsultarRecompensaAfk(IRepositorioDeProgresso r, ICatalogo c, IRelogio relogio, CalcularAtributos atributos) : base(r, c)
        {
            this.relogio = relogio;
            this.atributos = atributos;
        }

        public RecompensaAfk Executar() => Calcular(repositorio.Carregar(), catalogo, relogio, atributos);

        internal static RecompensaAfk Calcular(Progresso p, ICatalogo catalogo, IRelogio relogio, CalcularAtributos atributos)
        {
            var c = catalogo.Campanha;
            if (p.UltimaPresencaUtc <= 0) return new RecompensaAfk { Materiais = new List<ParDeValor>() };
            double fora = (relogio.AgoraUtc - new DateTime(p.UltimaPresencaUtc, DateTimeKind.Utc)).TotalSeconds;
            double segundos = RegrasDoAfk.SegundosQueContam(c, fora);

            var a = atributos.Executar(null);
            double porSegundo = a.DanoAutomatico + (a.Follower == null ? 0 : a.DanoDoFollower / Math.Max(0.1, a.Follower.IntervaloDeAtaque));
            var inimigo = CalcularInimigo.Para(p, catalogo, null, false);
            long vitorias = (long)Math.Floor(RegrasDoAfk.Vitorias(c, segundos, porSegundo, inimigo.Vida));

            // o material vem dos monstros da região atual, em partes iguais
            var materiais = new List<ParDeValor>();
            var regiao = RegrasDaCampanha.Regiao(c, catalogo.Regioes, p.Estagio);
            if (regiao != null && regiao.Monstros.Count > 0)
                for (int i = 0; i < regiao.Monstros.Count; i++)
                {
                    var m = catalogo.Monstro(regiao.Monstros[i]);
                    if (m == null || string.IsNullOrEmpty(m.MaterialId)) continue;
                    long lutas = vitorias / regiao.Monstros.Count + (i < vitorias % regiao.Monstros.Count ? 1 : 0);
                    if (lutas > 0) materiais.Add(new ParDeValor { Id = m.MaterialId, Valor = (int)Math.Min(int.MaxValue, lutas * m.MaterialPorVitoria) });
                }

            return new RecompensaAfk
            {
                TemRecompensa = fora >= c.MinimoDoAfkEmSegundos && vitorias > 0,
                Segundos = segundos,
                Vitorias = vitorias,
                Ouro = vitorias * inimigo.Ouro,
                Materiais = materiais,
                Itens = RegrasDoAfk.Itens(vitorias, catalogo.ParametrosDosItens.ChanceDeDrop),
            };
        }
    }

    // Entrega a recompensa AFK (ouro, material e itens sorteados) e recomeça a contar o tempo fora
    public class ColetarRecompensaAfk : CasoDeUso
    {
        // teto de sorteios por coleta: o resto do que cairia vira ouro pela média, sem laço gigante
        const int MaximoDeItensSorteados = 200;

        readonly IRelogio relogio;
        readonly CalcularAtributos atributos;
        readonly ISorteio sorteio;

        public ColetarRecompensaAfk(IRepositorioDeProgresso r, ICatalogo c, IRelogio relogio, CalcularAtributos atributos, ISorteio sorteio) : base(r, c)
        {
            this.relogio = relogio;
            this.atributos = atributos;
            this.sorteio = sorteio;
        }

        public RecompensaAfk Executar()
        {
            var p = repositorio.Carregar();
            var r = ConsultarRecompensaAfk.Calcular(p, catalogo, relogio, atributos);
            if (r.TemRecompensa)
            {
                p.ReceberOuro(r.Ouro);
                foreach (var m in r.Materiais) p.AdicionarMaterial(m.Id, m.Valor);
                if (catalogo.Itens.Count > 0)
                    for (int i = 0; i < Math.Min(r.Itens, MaximoDeItensSorteados); i++)
                        DropDeItens.Entregar(p, catalogo, sorteio, false, p.Estagio, out _);
            }
            p.MarcarPresenca(relogio.AgoraUtc);
            repositorio.Salvar(p);
            return r;
        }
    }

    public struct SituacaoDaAscensao
    {
        public bool Pode;
        public int EstagioMinimo;
        public long Almas;                  // já acumuladas
        public long AlmasGanhas;            // se ascender agora
        public double MultiplicadorAtual;
        public double MultiplicadorDepois;
        public int Ascensoes;
    }

    public class ConsultarAscensao : CasoDeUso
    {
        public ConsultarAscensao(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public SituacaoDaAscensao Executar()
        {
            var p = repositorio.Carregar();
            var c = catalogo.Campanha;
            long ganhas = RegrasDaAscensao.AlmasGanhas(c, p.Estagio);
            return new SituacaoDaAscensao
            {
                Pode = RegrasDaAscensao.PodeAscender(c, p.Estagio),
                EstagioMinimo = c.EstagioMinimoDaAscensao,
                Almas = p.Almas,
                AlmasGanhas = ganhas,
                MultiplicadorAtual = RegrasDaAscensao.Multiplicador(c, p.Almas),
                MultiplicadorDepois = RegrasDaAscensao.Multiplicador(c, p.Almas + ganhas),
                Ascensoes = p.Ascensoes,
            };
        }
    }

    // Troca estágio, upgrades e ouro por Almas; false se ainda não chegou ao estágio mínimo
    public class Ascender : CasoDeUso
    {
        public Ascender(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public bool Executar()
        {
            var p = repositorio.Carregar();
            if (!RegrasDaAscensao.PodeAscender(catalogo.Campanha, p.Estagio)) return false;
            p.Ascender(RegrasDaAscensao.AlmasGanhas(catalogo.Campanha, p.Estagio));
            repositorio.Salvar(p);
            return true;
        }
    }

    public class SituacaoDaConquista
    {
        public string Id;
        public string Nome;
        public string Texto;         // a descrição com o próximo degrau (ou o último, se completa)
        public double Valor;
        public double ProximaMeta;   // NaN se completa
        public int Coletadas;
        public int Prontas;          // degraus atingidos e ainda não coletados
        public int TotalDeDegraus;
        public bool Completa;
        public int CristaisProntos;
    }

    public class ListarConquistas : CasoDeUso
    {
        public ListarConquistas(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public List<SituacaoDaConquista> Executar()
        {
            var p = repositorio.Carregar();
            var lista = new List<SituacaoDaConquista>(catalogo.Conquistas.Count);
            foreach (var c in catalogo.Conquistas) lista.Add(Situacao(c, p));
            return lista;
        }

        public SituacaoDaConquista Situacao(DefinicaoDeConquista c, Progresso p)
        {
            double valor = RegrasDasConquistas.Valor(c.Meta, p, catalogo.Bestiario);
            int atingidos = RegrasDasConquistas.DegrausAtingidos(c, valor);
            int coletadas = Math.Min(p.ConquistasColetadas(c.Id), c.Degraus.Length);
            int cristais = 0;
            for (int i = coletadas; i < atingidos; i++) cristais += RegrasDasConquistas.Cristais(catalogo.ParametrosDasConquistas, i);
            double proxima = RegrasDasConquistas.ProximaMeta(c, coletadas);
            double mostrada = double.IsNaN(proxima) ? c.Degraus[c.Degraus.Length - 1] : proxima;
            return new SituacaoDaConquista
            {
                Id = c.Id,
                Nome = c.Nome,
                Texto = string.Format(c.Descricao ?? "{0}", FormatadorDeNumeros.Formatar(mostrada)),
                Valor = valor,
                ProximaMeta = proxima,
                Coletadas = coletadas,
                Prontas = Math.Max(0, atingidos - coletadas),
                TotalDeDegraus = c.Degraus.Length,
                Completa = coletadas >= c.Degraus.Length,
                CristaisProntos = cristais,
            };
        }
    }

    // Coleta os degraus atingidos de uma conquista: cristais agora e +1% de dano por degrau; devolve os cristais
    public class ColetarConquista : CasoDeUso
    {
        public ColetarConquista(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public int Executar(string id)
        {
            DefinicaoDeConquista c = null;
            foreach (var x in catalogo.Conquistas) if (x.Id == id) c = x;
            if (c == null) return 0;
            var p = repositorio.Carregar();
            var s = new ListarConquistas(repositorio, catalogo).Situacao(c, p);
            if (s.Prontas <= 0) return 0;
            p.ReceberCristais(s.CristaisProntos);
            p.MarcarConquistaColetada(c.Id, s.Coletadas + s.Prontas);
            repositorio.Salvar(p);
            return s.CristaisProntos;
        }
    }

    // Vida e recompensa do inimigo conforme o estágio atual da campanha
    public class CalcularInimigo : CasoDeUso
    {
        public CalcularInimigo(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public InimigoDaBatalha Executar(string tipoDoMonstro, bool chefe) =>
            Para(repositorio.Carregar(), catalogo, tipoDoMonstro == null ? null : catalogo.Monstro(tipoDoMonstro), chefe);

        internal static InimigoDaBatalha Para(Progresso p, ICatalogo c, DefinicaoDeMonstro m, bool chefe)
        {
            double vida = RegrasDaCampanha.VidaDoInimigo(c.Campanha, p.Estagio, m?.MultiplicadorDeVida ?? 1, chefe);
            return new InimigoDaBatalha
            {
                Nome = (chefe ? "Chefe " : "") + (m?.Nome ?? "Inimigo"),
                Vida = vida,
                Ouro = RegrasDaCampanha.Ouro(c.Campanha, vida),
                Chefe = chefe,
                Forma = m?.Forma ?? FormaDoMonstro.Gosma,
                Cor = m?.Cor ?? "#4CE64C",
            };
        }
    }

    public struct VisualDoHeroi
    {
        public bool TemArma;
        public Raridade RaridadeDaArma;
        public float Brilho;   // RegrasDeAparencia.BrilhoDoItem da arma; 0 sem arma
    }

    // O que o herói mostra: a arma do slot Arma, com a cor da raridade e o brilho do refino
    public class ConsultarVisualDoHeroi : CasoDeUso
    {
        public ConsultarVisualDoHeroi(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public VisualDoHeroi Executar()
        {
            var arma = repositorio.Carregar().Equipado(SlotDeItem.Arma);
            if (arma == null) return default;
            return new VisualDoHeroi
            {
                TemArma = true,
                RaridadeDaArma = arma.Raridade,
                Brilho = RegrasDeAparencia.BrilhoDoItem(arma.Raridade, arma.Nivel),
            };
        }
    }

    public class ConsultarCampanha : CasoDeUso
    {
        public ConsultarCampanha(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public SituacaoDaCampanha Executar()
        {
            var p = repositorio.Carregar();
            return new SituacaoDaCampanha
            {
                Estagio = p.Estagio,
                VitoriasNoEstagio = p.VitoriasNoEstagio,
                VitoriasParaAvancar = catalogo.Campanha.VitoriasParaAvancar,
                EstagioDeChefe = RegrasDaCampanha.EhEstagioDeChefe(catalogo.Campanha, p.Estagio),
                Regiao = RegrasDaCampanha.Regiao(catalogo.Campanha, catalogo.Regioes, p.Estagio),
            };
        }
    }

    // Poder Total do jogador, sem bônus contra um tipo específico de monstro
    public class CalcularPoder
    {
        readonly CalcularAtributos calcularAtributos;

        public CalcularPoder(CalcularAtributos calcularAtributos) => this.calcularAtributos = calcularAtributos;

        public double Executar() =>
            RegrasDePoder.Poder(calcularAtributos.Executar(null), RegrasDePoder.MultiplicadorCriticoPadrao);
    }

    // Dano e crítico do jogador contra um tipo de monstro: upgrades × itens equipados × bestiário × Almas × conquistas
    public class CalcularAtributos : CasoDeUso
    {
        public CalcularAtributos(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public AtributosDeCombate Executar(string tipoDoMonstro)
        {
            var p = repositorio.Carregar();
            double Valor(string id) => catalogo.Upgrade(id) is DefinicaoDeUpgrade d ? p.Valor(d) : 0;
            double Mult(AlvoDoItem alvo) => p.MultiplicadorDeDano(alvo, tipoDoMonstro, catalogo.Itens, catalogo.ParametrosDosItens, catalogo.Bestiario);
            double almas = RegrasDaAscensao.Multiplicador(catalogo.Campanha, p.Almas)
                * RegrasDasConquistas.Multiplicador(catalogo.ParametrosDasConquistas, p.TotalDeConquistasColetadas);
            double automatico = Valor(IdsDosUpgrades.Automatico) * Mult(AlvoDoItem.Automatico) * almas;
            var follower = catalogo.Follower(p.FollowerAtivoId);
            return new AtributosDeCombate
            {
                DanoDoClick = Valor(IdsDosUpgrades.Forca) * Mult(AlvoDoItem.Click) * almas,
                DanoAutomatico = automatico,
                ChanceCritica = Valor(IdsDosUpgrades.Precisao),
                Classe = catalogo.Classe(p.ClasseId),
                Follower = follower,
                DanoDoFollower = RegrasDePersonagem.DanoDoFollower(follower, automatico),
            };
        }
    }
}

namespace ALotOfDamage.Core
{
    public enum TipoDePendencia { Upgrade, Forja, Bestiario, Chefe, Ascensao, Conquista }

    public struct Pendencia
    {
        public TipoDePendencia Tipo;
        public string Texto;
    }

    // Coisas prontas para o jogador fazer agora: o painel lateral que puxa de volta para o jogo
    public class ListarPendencias : CasoDeUso
    {
        const double PertoDoMarco = 0.8;

        public ListarPendencias(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public void Executar(List<Pendencia> saida)
        {
            saida.Clear();
            var p = repositorio.Carregar();

            foreach (var c in catalogo.Conquistas)
            {
                int atingidos = RegrasDasConquistas.DegrausAtingidos(c, RegrasDasConquistas.Valor(c.Meta, p, catalogo.Bestiario));
                if (atingidos > p.ConquistasColetadas(c.Id))
                    saida.Add(new Pendencia { Tipo = TipoDePendencia.Conquista, Texto = $"Conquista pronta: {c.Nome}" });
            }

            if (RegrasDaAscensao.PodeAscender(catalogo.Campanha, p.Estagio))
                saida.Add(new Pendencia
                {
                    Tipo = TipoDePendencia.Ascensao,
                    Texto = $"Ascensão pronta: +{RegrasDaAscensao.AlmasGanhas(catalogo.Campanha, p.Estagio)} Almas",
                });

            if (RegrasDaCampanha.EhEstagioDeChefe(catalogo.Campanha, p.Estagio))
                saida.Add(new Pendencia { Tipo = TipoDePendencia.Chefe, Texto = $"Chefe do estágio {p.Estagio} apareceu!" });

            foreach (var u in catalogo.Upgrades)
            {
                int nivel = p.Nivel(u.Id);
                if (RegrasDeUpgrade.NoMaximo(u, nivel) || RegrasDeUpgrade.Custo(u, nivel) > p.Ouro) continue;
                saida.Add(new Pendencia { Tipo = TipoDePendencia.Upgrade, Texto = $"Upgrade pronto: {u.Nome}" });
            }

            foreach (var item in p.Equipados)
            {
                var t = catalogo.Item(item.TipoId);
                if (t == null || RegrasDaForja.CustoEmOuro(t, item.Nivel) > p.Ouro || RegrasDaForja.CustoEmMaterial(t, item.Nivel) > p.Material(t.MaterialId)) continue;
                saida.Add(new Pendencia { Tipo = TipoDePendencia.Forja, Texto = $"Forja pronta: {t.Nome} +{item.Nivel + 1}" });
            }

            foreach (var m in catalogo.Monstros)
            {
                int derrotas = p.Derrotas(m.Id);
                long marco = RegrasDoBestiario.ProximoMarco(catalogo.Bestiario, derrotas);
                if (derrotas == 0 || derrotas < marco * PertoDoMarco) continue;
                saida.Add(new Pendencia { Tipo = TipoDePendencia.Bestiario, Texto = $"Bestiário: {m.Nome} {derrotas}/{marco}" });
            }
        }
    }
}
