using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class ProgressoDoJogadorTestes
    {
        static readonly DefinicaoDeUpgrade Forca = new DefinicaoDeUpgrade
        {
            Id = "teste_forca", CustoBase = 10, CrescimentoDoCusto = 1.07, ValorBase = 3,
        };

        [SetUp]
        public void Limpar() => ProgressoDoJogador.Reiniciar();

        [Test]
        public void Upgrade_comeca_no_nivel_1()
        {
            Assert.AreEqual(1, ProgressoDoJogador.Nivel(Forca.Id));
            Assert.AreEqual(3, ProgressoDoJogador.Valor(Forca), 1e-9);
        }

        [Test]
        public void Compra_guarda_o_nivel_e_desconta_o_ouro()
        {
            ProgressoDoJogador.Ouro = 100;
            ProgressoDoJogador.Comprar(Forca, 1);
            Assert.AreEqual(2, ProgressoDoJogador.Nivel(Forca.Id));
            Assert.AreEqual(90, ProgressoDoJogador.Ouro, 1e-9);
        }

        [Test]
        public void Compra_sem_ouro_nao_muda_nada()
        {
            ProgressoDoJogador.Ouro = 5;
            var r = ProgressoDoJogador.Comprar(Forca, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(1, ProgressoDoJogador.Nivel(Forca.Id));
            Assert.AreEqual(5, ProgressoDoJogador.Ouro, 1e-9);
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
        public void Vitoria_conta_no_bestiario_e_da_material()
        {
            var monstro = new DefinicaoDeMonstro { Id = "lobo", MaterialId = "presa", MaterialPorVitoria = 2 };
            ProgressoDoJogador.RegistrarVitoria(monstro, Bestiario);
            Assert.AreEqual(1, ProgressoDoJogador.Derrotas("lobo"));
            Assert.AreEqual(2, ProgressoDoJogador.Material("presa"));
        }

        [Test]
        public void Forjar_guarda_o_nivel_e_gasta_ouro_e_material()
        {
            ProgressoDoJogador.Ouro = 15;
            ProgressoDoJogador.AdicionarMaterial("presa", 3);
            var r = ProgressoDoJogador.Forjar(Espada, 0.99);
            Assert.IsTrue(r.Forjou);
            Assert.AreEqual(1, ProgressoDoJogador.NivelDoItem(Espada.Id));
            Assert.AreEqual(5, ProgressoDoJogador.Ouro, 1e-9);
            Assert.AreEqual(2, ProgressoDoJogador.Material("presa"));
        }

        [Test]
        public void Fontes_de_poder_se_multiplicam()
        {
            ProgressoDoJogador.Ouro = 10;
            ProgressoDoJogador.AdicionarMaterial("presa", 1);
            ProgressoDoJogador.Forjar(Espada, 0.99); // ×2
            var lobo = new DefinicaoDeMonstro { Id = "lobo" };
            for (int i = 0; i < 10; i++) ProgressoDoJogador.RegistrarVitoria(lobo, Bestiario); // ×1,5 contra lobo, ×1,1 geral

            var itens = new[] { Espada };
            Assert.AreEqual(2 * 1.5 * 1.1, ProgressoDoJogador.MultiplicadorDeDano(AlvoDoItem.Click, "lobo", itens, Bestiario), 1e-9);
            Assert.AreEqual(2 * 1.1, ProgressoDoJogador.MultiplicadorDeDano(AlvoDoItem.Click, "slime", itens, Bestiario), 1e-9);
            Assert.AreEqual(1.1, ProgressoDoJogador.MultiplicadorDeDano(AlvoDoItem.Automatico, "slime", itens, Bestiario), 1e-9);
        }

        [Test]
        public void Balanceamento_padrao_tem_os_tres_upgrades_do_prototipo()
        {
            var b = BalanceamentoDeUpgrades.Padrao();
            Assert.IsNotNull(b.Find(u => u.Id == BalanceamentoDeUpgrades.IdForca));
            Assert.IsNotNull(b.Find(u => u.Id == BalanceamentoDeUpgrades.IdAutomatico));
            Assert.IsNotNull(b.Find(u => u.Id == BalanceamentoDeUpgrades.IdPrecisao));
        }
    }
}
