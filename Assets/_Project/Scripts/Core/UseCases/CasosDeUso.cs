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
            int estagioDaLuta = p.Estagio;
            var campanha = p.AvancarCampanha(catalogo.Campanha, chefe);
            var item = SortearDrop(p, chefe, estagioDaLuta, out bool vendido);
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

        // O item cai do estágio em que a luta aconteceu; com o inventário cheio ele é vendido na hora
        ItemDoJogador SortearDrop(Progresso p, bool chefe, int estagio, out bool vendido)
        {
            vendido = false;
            var regras = catalogo.ParametrosDosItens;
            if (catalogo.Itens.Count == 0 || !RegrasDosItens.CaiItem(regras, sorteio.Proximo(), chefe)) return null;
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

    // Brilho da arma do herói: segue o item de click equipado mais refinado
    public class ConsultarBrilhoDaArma : CasoDeUso
    {
        public ConsultarBrilhoDaArma(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public float Executar()
        {
            int nivel = 0;
            foreach (var item in repositorio.Carregar().Equipados)
                if (catalogo.Item(item.TipoId)?.Alvo == AlvoDoItem.Click) nivel = Math.Max(nivel, item.Nivel);
            return RegrasDeAparencia.BrilhoDaArma(nivel);
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

    // Dano e crítico do jogador contra um tipo de monstro: upgrades × itens equipados × bestiário
    public class CalcularAtributos : CasoDeUso
    {
        public CalcularAtributos(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public AtributosDeCombate Executar(string tipoDoMonstro)
        {
            var p = repositorio.Carregar();
            double Valor(string id) => catalogo.Upgrade(id) is DefinicaoDeUpgrade d ? p.Valor(d) : 0;
            double Mult(AlvoDoItem alvo) => p.MultiplicadorDeDano(alvo, tipoDoMonstro, catalogo.Itens, catalogo.ParametrosDosItens, catalogo.Bestiario);
            double automatico = Valor(IdsDosUpgrades.Automatico) * Mult(AlvoDoItem.Automatico);
            var follower = catalogo.Follower(p.FollowerAtivoId);
            return new AtributosDeCombate
            {
                DanoDoClick = Valor(IdsDosUpgrades.Forca) * Mult(AlvoDoItem.Click),
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
    public enum TipoDePendencia { Upgrade, Forja, Bestiario, Chefe }

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
