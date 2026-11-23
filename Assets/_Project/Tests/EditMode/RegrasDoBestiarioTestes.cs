using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDoBestiarioTestes
    {
        static ParametrosDoBestiario Padrao() => new ParametrosDoBestiario
        {
            PrimeiroMarco = 10, FatorEntreMarcos = 10, BonusContraTipoPorMarco = 0.25, BonusGeralPorMarco = 0.02,
        };

        [TestCase(0, 0)]
        [TestCase(9, 0)]
        [TestCase(10, 1)]
        [TestCase(99, 1)]
        [TestCase(100, 2)]
        [TestCase(1000, 3)]
        [TestCase(12345, 4)]
        public void Marcos_em_10_100_1000(int derrotas, int esperado)
        {
            Assert.AreEqual(esperado, RegrasDoBestiario.MarcosAtingidos(Padrao(), derrotas));
        }

        [TestCase(0, 10)]
        [TestCase(10, 100)]
        [TestCase(150, 1000)]
        public void Proximo_marco(int derrotas, int esperado)
        {
            Assert.AreEqual(esperado, RegrasDoBestiario.ProximoMarco(Padrao(), derrotas));
        }

        [Test]
        public void Bonus_contra_o_tipo_soma_por_marco()
        {
            Assert.AreEqual(1, RegrasDoBestiario.MultiplicadorContraTipo(Padrao(), 5), 1e-9);
            Assert.AreEqual(1.5, RegrasDoBestiario.MultiplicadorContraTipo(Padrao(), 100), 1e-9);
        }

        [Test]
        public void Bonus_geral_conta_os_marcos_de_todos_os_tipos()
        {
            Assert.AreEqual(1.06, RegrasDoBestiario.MultiplicadorGeral(Padrao(), new[] { 10, 100, 3 }), 1e-9);
        }

        [Test]
        public void Vitoria_que_alcanca_o_marco_avisa()
        {
            Assert.IsTrue(RegrasDoBestiario.AtingiuMarco(Padrao(), 9, 10));
            Assert.IsFalse(RegrasDoBestiario.AtingiuMarco(Padrao(), 10, 11));
        }
    }
}
