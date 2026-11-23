using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDeUpgradeTestes
    {
        static DefinicaoDeUpgrade Dano() => new DefinicaoDeUpgrade
        {
            Id = "forca",
            CustoBase = 10,
            CrescimentoDoCusto = 1.07,
            ValorBase = 3,
            Tipo = TipoDeUpgrade.ComMarcos,
        };

        static DefinicaoDeUpgrade Chance() => new DefinicaoDeUpgrade
        {
            Id = "precisao",
            CustoBase = 50,
            CrescimentoDoCusto = 1.15,
            ValorBase = 0.05,
            ValorPorNivel = 0.01,
            Teto = 0.5,
            Tipo = TipoDeUpgrade.Linear,
        };

        // Custo

        [Test]
        public void Custo_do_primeiro_upgrade_e_o_custo_base()
        {
            Assert.AreEqual(10, RegrasDeUpgrade.Custo(Dano(), nivelAtual: 1), 1e-9);
        }

        [Test]
        public void Custo_cresce_7_por_cento_por_nivel()
        {
            Assert.AreEqual(10 * 1.07, RegrasDeUpgrade.Custo(Dano(), 2), 1e-9);
            Assert.AreEqual(10 * System.Math.Pow(1.07, 9), RegrasDeUpgrade.Custo(Dano(), 10), 1e-9);
        }

        [Test]
        public void Custo_de_varios_niveis_e_a_soma_dos_custos()
        {
            double esperado = RegrasDeUpgrade.Custo(Dano(), 5) + RegrasDeUpgrade.Custo(Dano(), 6) + RegrasDeUpgrade.Custo(Dano(), 7);
            Assert.AreEqual(esperado, RegrasDeUpgrade.CustoDeVarios(Dano(), 5, 3), 1e-6);
        }

        [Test]
        public void Quantos_niveis_cabem_no_ouro()
        {
            double tres = RegrasDeUpgrade.CustoDeVarios(Dano(), 1, 3);
            Assert.AreEqual(3, RegrasDeUpgrade.QuantosCabem(Dano(), 1, tres));
            Assert.AreEqual(2, RegrasDeUpgrade.QuantosCabem(Dano(), 1, tres - 0.01));
            Assert.AreEqual(0, RegrasDeUpgrade.QuantosCabem(Dano(), 1, 9.99));
        }

        // Marcos

        [TestCase(1, 1)]
        [TestCase(9, 1)]
        [TestCase(10, 2)]
        [TestCase(20, 4)]
        [TestCase(25, 16)]          // 2^2 * 4
        [TestCase(50, 32 * 16)]     // 2^5 * 4^2
        [TestCase(100, 1024 * 256 * 10.0)]
        public void Multiplicador_de_marcos(int nivel, double esperado)
        {
            Assert.AreEqual(esperado, RegrasDeUpgrade.MultiplicadorDeMarcos(nivel), 1e-6);
        }

        [TestCase(1, 10)]
        [TestCase(10, 20)]
        [TestCase(20, 25)]
        [TestCase(25, 30)]
        [TestCase(99, 100)]
        public void Proximo_marco(int nivel, int esperado)
        {
            Assert.AreEqual(esperado, RegrasDeUpgrade.ProximoMarco(nivel));
        }

        [TestCase(1, 0)]
        [TestCase(10, 10)]
        [TestCase(24, 20)]
        [TestCase(26, 25)]
        [TestCase(31, 30)]
        public void Marco_anterior(int nivel, int esperado)
        {
            Assert.AreEqual(esperado, RegrasDeUpgrade.MarcoAnterior(nivel));
        }

        // Valor

        [Test]
        public void Valor_com_marcos_e_base_vezes_nivel_vezes_marcos()
        {
            Assert.AreEqual(3, RegrasDeUpgrade.Valor(Dano(), 1), 1e-9);
            Assert.AreEqual(3 * 9, RegrasDeUpgrade.Valor(Dano(), 9), 1e-9);
            Assert.AreEqual(3 * 10 * 2, RegrasDeUpgrade.Valor(Dano(), 10), 1e-9);
        }

        [Test]
        public void Valor_linear_respeita_o_teto()
        {
            Assert.AreEqual(0.05, RegrasDeUpgrade.Valor(Chance(), 1), 1e-9);
            Assert.AreEqual(0.14, RegrasDeUpgrade.Valor(Chance(), 10), 1e-9);
            Assert.AreEqual(0.5, RegrasDeUpgrade.Valor(Chance(), 500), 1e-9);
        }

        [Test]
        public void Ganho_relativo_no_marco_mostra_o_salto()
        {
            // 9 -> 10: 27 -> 60, ganho de 122%
            Assert.AreEqual(60.0 / 27 - 1, RegrasDeUpgrade.GanhoRelativo(Dano(), 9), 1e-9);
        }

        [Test]
        public void Linear_no_teto_nao_pode_mais_subir()
        {
            Assert.IsTrue(RegrasDeUpgrade.NoMaximo(Chance(), 46));
            Assert.IsFalse(RegrasDeUpgrade.NoMaximo(Chance(), 45));
            Assert.IsFalse(RegrasDeUpgrade.NoMaximo(Dano(), 10_000));
        }

        // Compra

        [Test]
        public void Comprar_desconta_o_ouro_e_sobe_o_nivel()
        {
            var r = RegrasDeUpgrade.Comprar(Dano(), nivelAtual: 1, ouro: 100, quantidade: 1);
            Assert.IsTrue(r.Comprou);
            Assert.AreEqual(2, r.NovoNivel);
            Assert.AreEqual(90, r.OuroRestante, 1e-9);
        }

        [Test]
        public void Sem_ouro_suficiente_nao_compra_nada()
        {
            var r = RegrasDeUpgrade.Comprar(Dano(), 1, 5, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(1, r.NovoNivel);
            Assert.AreEqual(5, r.OuroRestante, 1e-9);
        }

        [Test]
        public void Comprar_maximo_compra_tudo_que_cabe()
        {
            double ouro = RegrasDeUpgrade.CustoDeVarios(Dano(), 1, 4) + 1;
            var r = RegrasDeUpgrade.Comprar(Dano(), 1, ouro, RegrasDeUpgrade.Maximo);
            Assert.AreEqual(5, r.NovoNivel);
            Assert.AreEqual(1, r.OuroRestante, 1e-6);
        }

        [Test]
        public void Compra_que_cruza_um_marco_avisa()
        {
            double ouro = RegrasDeUpgrade.CustoDeVarios(Dano(), 9, 1);
            Assert.IsTrue(RegrasDeUpgrade.Comprar(Dano(), 9, ouro, 1).AtingiuMarco);
            Assert.IsFalse(RegrasDeUpgrade.Comprar(Dano(), 1, 100, 1).AtingiuMarco);
        }

        [Test]
        public void Linear_nao_passa_do_teto_na_compra_maxima()
        {
            var r = RegrasDeUpgrade.Comprar(Chance(), 40, 1e12, RegrasDeUpgrade.Maximo);
            Assert.AreEqual(46, r.NovoNivel);
        }
    }
}
