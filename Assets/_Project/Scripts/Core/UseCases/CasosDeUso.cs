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

    public class ForjarItem : CasoDeUso
    {
        readonly ISorteio sorteio;

        public ForjarItem(IRepositorioDeProgresso r, ICatalogo c, ISorteio sorteio) : base(r, c) => this.sorteio = sorteio;

        public ResultadoDaForja Executar(string idDoItem)
        {
            var d = catalogo.Item(idDoItem);
            var p = repositorio.Carregar();
            if (d == null) return new ResultadoDaForja { OuroRestante = p.Ouro };
            var r = p.Forjar(d, sorteio.Proximo());
            if (r.Forjou) repositorio.Salvar(p);
            return r;
        }
    }

    public class RegistrarVitoria : CasoDeUso
    {
        public RegistrarVitoria(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        // tipoDoMonstro pode ser null (ex.: cena de Batalha aberta direto, sem encontro)
        public ResultadoDaVitoria Executar(string tipoDoMonstro, bool chefe)
        {
            var p = repositorio.Carregar();
            var monstro = tipoDoMonstro == null ? null : catalogo.Monstro(tipoDoMonstro);
            double ouro = CalcularInimigo.Para(p, catalogo, monstro, chefe).Ouro;
            p.ReceberOuro(ouro);
            bool marco = p.RegistrarVitoria(monstro, catalogo.Bestiario);
            var campanha = p.AvancarCampanha(catalogo.Campanha, chefe);
            repositorio.Salvar(p);
            return new ResultadoDaVitoria
            {
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

    // Dano e crítico do jogador contra um tipo de monstro: upgrades × forja × bestiário
    public class CalcularAtributos : CasoDeUso
    {
        public CalcularAtributos(IRepositorioDeProgresso r, ICatalogo c) : base(r, c) { }

        public AtributosDeCombate Executar(string tipoDoMonstro)
        {
            var p = repositorio.Carregar();
            double Valor(string id) => catalogo.Upgrade(id) is DefinicaoDeUpgrade d ? p.Valor(d) : 0;
            double Mult(AlvoDoItem alvo) => p.MultiplicadorDeDano(alvo, tipoDoMonstro, catalogo.Itens, catalogo.Bestiario);
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

            foreach (var i in catalogo.Itens)
            {
                int nivel = p.NivelDoItem(i.Id);
                if (RegrasDaForja.CustoEmOuro(i, nivel) > p.Ouro || RegrasDaForja.CustoEmMaterial(i, nivel) > p.Material(i.MaterialId)) continue;
                saida.Add(new Pendencia { Tipo = TipoDePendencia.Forja, Texto = $"Forja pronta: {i.Nome} +{nivel + 1}" });
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
