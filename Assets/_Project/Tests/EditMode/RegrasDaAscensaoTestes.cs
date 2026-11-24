using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaAscensaoTestes
    {
        static ParametrosDaCampanha Parametros() => new ParametrosDaCampanha
        {
            EstagioMinimoDaAscensao = 50, EstagioBaseDasAlmas = 45, DivisorDasAlmas = 5, BonusPorAlma = 0.1,
        };

        [Test]
        public void So_pode_ascender_a_partir_do_estagio_minimo()
        {
            Assert.IsFalse(RegrasDaAscensao.PodeAscender(Parametros(), 49));
            Assert.IsTrue(RegrasDaAscensao.PodeAscender(Parametros(), 50));
        }

        [Test]
        public void Almas_crescem_com_o_quadrado_do_estagio()
        {
            Assert.AreEqual(5, RegrasDaAscensao.AlmasGanhas(Parametros(), 50));
            Assert.AreEqual(45, RegrasDaAscensao.AlmasGanhas(Parametros(), 60));
            Assert.AreEqual(245, RegrasDaAscensao.AlmasGanhas(Parametros(), 80));
        }

        [Test]
        public void Antes_do_minimo_nao_ha_almas()
        {
            Assert.AreEqual(0, RegrasDaAscensao.AlmasGanhas(Parametros(), 49));
        }

        [Test]
        public void Cada_alma_soma_dez_por_cento_de_dano()
        {
            Assert.AreEqual(1, RegrasDaAscensao.Multiplicador(Parametros(), 0), 1e-9);
            Assert.AreEqual(1.5, RegrasDaAscensao.Multiplicador(Parametros(), 5), 1e-9);
            Assert.AreEqual(25.5, RegrasDaAscensao.Multiplicador(Parametros(), 245), 1e-9);
        }
    }
}
