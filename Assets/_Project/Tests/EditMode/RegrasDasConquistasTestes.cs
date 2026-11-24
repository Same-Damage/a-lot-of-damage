using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDasConquistasTestes
    {
        static DefinicaoDeConquista Vitorias() => new DefinicaoDeConquista
        {
            Id = "vitorias", Meta = MetaDaConquista.Vitorias, Degraus = new double[] { 10, 100, 1000 },
        };

        static ParametrosDasConquistas Parametros() => new ParametrosDasConquistas { BonusPorConquista = 0.01, CristaisBase = 10 };

        [Test]
        public void Degraus_atingidos_contam_quantos_valores_foram_passados()
        {
            Assert.AreEqual(0, RegrasDasConquistas.DegrausAtingidos(Vitorias(), 9));
            Assert.AreEqual(1, RegrasDasConquistas.DegrausAtingidos(Vitorias(), 10));
            Assert.AreEqual(2, RegrasDasConquistas.DegrausAtingidos(Vitorias(), 999));
            Assert.AreEqual(3, RegrasDasConquistas.DegrausAtingidos(Vitorias(), 1e9));
        }

        [Test]
        public void Proxima_meta_e_o_primeiro_degrau_nao_atingido()
        {
            Assert.AreEqual(100, RegrasDasConquistas.ProximaMeta(Vitorias(), 1), 1e-9);
            Assert.IsTrue(double.IsNaN(RegrasDasConquistas.ProximaMeta(Vitorias(), 3)), "todos os degraus feitos");
        }

        [Test]
        public void Cristais_crescem_com_o_degrau()
        {
            Assert.AreEqual(10, RegrasDasConquistas.Cristais(Parametros(), 0));
            Assert.AreEqual(40, RegrasDasConquistas.Cristais(Parametros(), 1));
            Assert.AreEqual(90, RegrasDasConquistas.Cristais(Parametros(), 2));
        }

        [Test]
        public void Cada_conquista_coletada_soma_um_por_cento_de_dano()
        {
            Assert.AreEqual(1, RegrasDasConquistas.Multiplicador(Parametros(), 0), 1e-9);
            Assert.AreEqual(1.25, RegrasDasConquistas.Multiplicador(Parametros(), 25), 1e-9);
        }
    }
}
