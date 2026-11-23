using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class ProgressoTestes
    {
        static readonly DefinicaoDeUpgrade Forca = new DefinicaoDeUpgrade
        {
            Id = "teste_forca", CustoBase = 10, CrescimentoDoCusto = 1.07, ValorBase = 3,
        };

        Progresso progresso;

        [SetUp]
        public void Limpar() => progresso = new Progresso();

        [Test]
        public void Upgrade_comeca_no_nivel_1()
        {
            Assert.AreEqual(1, progresso.Nivel(Forca.Id));
            Assert.AreEqual(3, progresso.Valor(Forca), 1e-9);
        }

        [Test]
        public void Compra_guarda_o_nivel_e_desconta_o_ouro()
        {
            progresso.ReceberOuro(100);
            progresso.Comprar(Forca, 1);
            Assert.AreEqual(2, progresso.Nivel(Forca.Id));
            Assert.AreEqual(90, progresso.Ouro, 1e-9);
        }

        [Test]
        public void Compra_sem_ouro_nao_muda_nada()
        {
            progresso.ReceberOuro(5);
            var r = progresso.Comprar(Forca, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(1, progresso.Nivel(Forca.Id));
            Assert.AreEqual(5, progresso.Ouro, 1e-9);
        }

        static readonly DefinicaoDeItem Espada = new DefinicaoDeItem
        {
            Id = "teste_espada", Alvo = AlvoDoItem.Click, BonusPorNivel = 1,
            CustoBaseEmOuro = 10, CrescimentoDoCusto = 2, MaterialId = "presa", MaterialPorNivel = 1,
        };

        static readonly ParametrosDoBestiario Bestiario = new ParametrosDoBestiario
        {
            PrimeiroMarco = 10, FatorEntreMarcos = 10, BonusContraTipoPorMarco = 0.5, BonusGeralPorMarco = 0.1,
        };

        [Test]
        public void Vitoria_sem_monstro_do_catalogo_conta_so_a_vitoria()
        {
            Assert.IsFalse(progresso.RegistrarVitoria(null, Bestiario));
            Assert.AreEqual(1, progresso.Vitorias);
        }

        [Test]
        public void Vitoria_conta_no_bestiario_e_da_material()
        {
            var monstro = new DefinicaoDeMonstro { Id = "lobo", MaterialId = "presa", MaterialPorVitoria = 2 };
            progresso.RegistrarVitoria(monstro, Bestiario);
            Assert.AreEqual(1, progresso.Derrotas("lobo"));
            Assert.AreEqual(2, progresso.Material("presa"));
        }

        [Test]
        public void Forjar_guarda_o_nivel_e_gasta_ouro_e_material()
        {
            progresso.ReceberOuro(15);
            progresso.AdicionarMaterial("presa", 3);
            var r = progresso.Forjar(Espada, 0.99);
            Assert.IsTrue(r.Forjou);
            Assert.AreEqual(1, progresso.NivelDoItem(Espada.Id));
            Assert.AreEqual(5, progresso.Ouro, 1e-9);
            Assert.AreEqual(2, progresso.Material("presa"));
        }

        [Test]
        public void Fontes_de_poder_se_multiplicam()
        {
            progresso.ReceberOuro(10);
            progresso.AdicionarMaterial("presa", 1);
            progresso.Forjar(Espada, 0.99); // ×2
            var lobo = new DefinicaoDeMonstro { Id = "lobo" };
            for (int i = 0; i < 10; i++) progresso.RegistrarVitoria(lobo, Bestiario); // ×1,5 contra lobo, ×1,1 geral

            var itens = new[] { Espada };
            Assert.AreEqual(2 * 1.5 * 1.1, progresso.MultiplicadorDeDano(AlvoDoItem.Click, "lobo", itens, Bestiario), 1e-9);
            Assert.AreEqual(2 * 1.1, progresso.MultiplicadorDeDano(AlvoDoItem.Click, "slime", itens, Bestiario), 1e-9);
            Assert.AreEqual(1.1, progresso.MultiplicadorDeDano(AlvoDoItem.Automatico, "slime", itens, Bestiario), 1e-9);
        }

        [Test]
        public void Balanceamento_padrao_tem_os_tres_upgrades_do_prototipo()
        {
            var b = BalanceamentoDeUpgrades.Padrao();
            Assert.IsNotNull(b.Find(u => u.Id == IdsDosUpgrades.Forca));
            Assert.IsNotNull(b.Find(u => u.Id == IdsDosUpgrades.Automatico));
            Assert.IsNotNull(b.Find(u => u.Id == IdsDosUpgrades.Precisao));
        }
    }
}
