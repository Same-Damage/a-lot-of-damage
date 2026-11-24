using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDeAparenciaTestes
    {
        [Test]
        public void Arma_sem_forja_nao_brilha()
        {
            Assert.AreEqual(0, RegrasDeAparencia.BrilhoDaArma(0), 1e-6);
        }

        [Test]
        public void Brilho_cresce_com_o_nivel_da_forja()
        {
            float anterior = 0;
            for (int nivel = 1; nivel <= 200; nivel++)
            {
                float b = RegrasDeAparencia.BrilhoDaArma(nivel);
                Assert.Greater(b, anterior, $"nível {nivel}");
                anterior = b;
            }
        }

        [Test]
        public void Brilho_chega_a_metade_no_mais_10_e_nunca_passa_de_um()
        {
            Assert.AreEqual(0.5f, RegrasDeAparencia.BrilhoDaArma(10), 1e-6);
            Assert.Less(RegrasDeAparencia.BrilhoDaArma(100000), 1);
        }

        [Test]
        public void Nivel_negativo_conta_como_zero()
        {
            Assert.AreEqual(0, RegrasDeAparencia.BrilhoDaArma(-3), 1e-6);
        }
    }
}
