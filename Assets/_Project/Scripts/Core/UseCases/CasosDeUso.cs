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
    }

    public struct ResultadoDaVitoria
    {
        public double Ouro;
        public DefinicaoDeMonstro Monstro;   // null quando a luta não veio de um monstro do catálogo
        public int MaterialGanho;
        public bool AtingiuMarcoDoBestiario;
        public int Derrotas;
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
        public ResultadoDaVitoria Executar(string tipoDoMonstro, double ouro)
        {
            var p = repositorio.Carregar();
            var monstro = tipoDoMonstro == null ? null : catalogo.Monstro(tipoDoMonstro);
            p.ReceberOuro(ouro);
            bool marco = p.RegistrarVitoria(monstro, catalogo.Bestiario);
            repositorio.Salvar(p);
            return new ResultadoDaVitoria
            {
                Ouro = ouro,
                Monstro = monstro,
                MaterialGanho = monstro?.MaterialPorVitoria ?? 0,
                AtingiuMarcoDoBestiario = marco,
                Derrotas = monstro == null ? 0 : p.Derrotas(monstro.Id),
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
            return new AtributosDeCombate
            {
                DanoDoClick = Valor(IdsDosUpgrades.Forca) * Mult(AlvoDoItem.Click),
                DanoAutomatico = Valor(IdsDosUpgrades.Automatico) * Mult(AlvoDoItem.Automatico),
                ChanceCritica = Valor(IdsDosUpgrades.Precisao),
            };
        }
    }
}
