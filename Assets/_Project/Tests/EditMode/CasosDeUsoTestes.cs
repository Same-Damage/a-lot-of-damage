using System.Collections.Generic;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class CasosDeUsoTestes
    {
        class CatalogoFalso : ICatalogo
        {
            public IReadOnlyList<DefinicaoDeUpgrade> Upgrades { get; set; } = new List<DefinicaoDeUpgrade>();
            public IReadOnlyList<DefinicaoDeItem> Itens { get; set; } = new List<DefinicaoDeItem>();
            public IReadOnlyList<DefinicaoDeMonstro> Monstros { get; set; } = new List<DefinicaoDeMonstro>();
            public ParametrosDoBestiario Bestiario { get; set; } = new ParametrosDoBestiario();
            public IReadOnlyList<DefinicaoDeClasse> Classes { get; set; } = new List<DefinicaoDeClasse>();
            public IReadOnlyList<DefinicaoDeFollower> Followers { get; set; } = new List<DefinicaoDeFollower>();
        }

        class RepositorioQueConta : IRepositorioDeProgresso
        {
            public readonly Progresso Progresso = new Progresso();
            public int Salvamentos;
            public Progresso Carregar() => Progresso;
            public void Salvar(Progresso p) => Salvamentos++;
        }

        class SorteioFixo : ISorteio
        {
            public double Valor;
            public double Proximo() => Valor;
        }

        RepositorioQueConta repositorio;
        CatalogoFalso catalogo;

        [SetUp]
        public void Montar()
        {
            repositorio = new RepositorioQueConta();
            catalogo = new CatalogoFalso
            {
                Upgrades = new List<DefinicaoDeUpgrade>
                {
                    new DefinicaoDeUpgrade { Id = IdsDosUpgrades.Forca, CustoBase = 10, CrescimentoDoCusto = 2, ValorBase = 3 },
                    new DefinicaoDeUpgrade { Id = IdsDosUpgrades.Automatico, CustoBase = 10, CrescimentoDoCusto = 2, ValorBase = 5 },
                    new DefinicaoDeUpgrade { Id = IdsDosUpgrades.Precisao, Tipo = TipoDeUpgrade.Linear, ValorBase = 0.05, ValorPorNivel = 0.01 },
                },
                Itens = new List<DefinicaoDeItem>
                {
                    new DefinicaoDeItem { Id = "espada", Alvo = AlvoDoItem.Click, BonusPorNivel = 1, CustoBaseEmOuro = 10, MaterialId = "presa", MaterialPorNivel = 1, ChanceDeSucessoCritico = 0.1 },
                },
                Monstros = new List<DefinicaoDeMonstro>
                {
                    new DefinicaoDeMonstro { Id = "lobo", MaterialId = "presa", MaterialPorVitoria = 2 },
                },
            };
        }

        [Test]
        public void Comprar_upgrade_salva_quando_compra()
        {
            repositorio.Progresso.ReceberOuro(10);
            var r = new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 1);
            Assert.IsTrue(r.Comprou);
            Assert.AreEqual(2, repositorio.Progresso.Nivel(IdsDosUpgrades.Forca));
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Comprar_upgrade_sem_ouro_nao_salva()
        {
            var r = new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(0, repositorio.Salvamentos);
        }

        [Test]
        public void Upgrade_inexistente_nao_compra()
        {
            repositorio.Progresso.ReceberOuro(1000);
            Assert.IsFalse(new ComprarUpgrade(repositorio, catalogo).Executar("nao_existe", 1).Comprou);
        }

        [Test]
        public void Forjar_usa_o_sorteio_injetado()
        {
            repositorio.Progresso.ReceberOuro(10);
            repositorio.Progresso.AdicionarMaterial("presa", 1);
            var r = new ForjarItem(repositorio, catalogo, new SorteioFixo { Valor = 0.05 }).Executar("espada");
            Assert.IsTrue(r.SucessoCritico);
            Assert.AreEqual(2, repositorio.Progresso.NivelDoItem("espada"));
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Vitoria_da_ouro_material_e_conta_no_bestiario()
        {
            var r = new RegistrarVitoria(repositorio, catalogo).Executar("lobo", 50);
            Assert.AreEqual(50, repositorio.Progresso.Ouro, 1e-9);
            Assert.AreEqual(2, repositorio.Progresso.Material("presa"));
            Assert.AreEqual(1, r.Derrotas);
            Assert.AreEqual(2, r.MaterialGanho);
            Assert.AreEqual(1, repositorio.Salvamentos);
        }

        [Test]
        public void Vitoria_sem_tipo_so_da_ouro()
        {
            var r = new RegistrarVitoria(repositorio, catalogo).Executar(null, 7);
            Assert.IsNull(r.Monstro);
            Assert.AreEqual(7, repositorio.Progresso.Ouro, 1e-9);
        }

        [Test]
        public void Atributos_multiplicam_upgrade_forja_e_bestiario()
        {
            repositorio.Progresso.ReceberOuro(10);
            repositorio.Progresso.AdicionarMaterial("presa", 1);
            new ForjarItem(repositorio, catalogo, new SorteioFixo { Valor = 0.99 }).Executar("espada"); // ×2 no click

            var a = new CalcularAtributos(repositorio, catalogo).Executar("lobo");
            Assert.AreEqual(3 * 2, a.DanoDoClick, 1e-9);
            Assert.AreEqual(5, a.DanoAutomatico, 1e-9);
            Assert.AreEqual(0.05, a.ChanceCritica, 1e-9);
        }

        [Test]
        public void Atributos_trazem_a_classe_e_o_follower_do_jogador()
        {
            catalogo.Classes = new List<DefinicaoDeClasse> { new DefinicaoDeClasse { Id = Progresso.ClassePadrao } };
            catalogo.Followers = new List<DefinicaoDeFollower> { new DefinicaoDeFollower { Id = Progresso.FollowerPadrao, FracaoDoAutoAtaque = 0.2 } };

            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.AreEqual(Progresso.ClassePadrao, a.Classe.Id);
            Assert.AreEqual(Progresso.FollowerPadrao, a.Follower.Id);
            Assert.AreEqual(5 * 0.2, a.DanoDoFollower, 1e-9);
        }

        [Test]
        public void Sem_follower_no_catalogo_o_follower_nao_causa_dano()
        {
            var a = new CalcularAtributos(repositorio, catalogo).Executar(null);
            Assert.IsNull(a.Follower);
            Assert.AreEqual(0, a.DanoDoFollower, 1e-9);
        }

        [Test]
        public void Poder_total_sobe_ao_comprar_upgrade()
        {
            var poder = new CalcularPoder(new CalcularAtributos(repositorio, catalogo));
            // click 3 + auto 5, crítico 5% ×5 → fator 1,2
            Assert.AreEqual(8 * 1.2, poder.Executar(), 1e-9);

            repositorio.Progresso.ReceberOuro(10);
            new ComprarUpgrade(repositorio, catalogo).Executar(IdsDosUpgrades.Forca, 1);
            Assert.AreEqual(11 * 1.2, poder.Executar(), 1e-9);
        }

        [Test]
        public void Repositorio_em_memoria_devolve_o_mesmo_progresso()
        {
            var r = new RepositorioDeProgressoEmMemoria();
            r.Carregar().ReceberOuro(5);
            Assert.AreEqual(5, r.Carregar().Ouro, 1e-9);
        }
    }
}
