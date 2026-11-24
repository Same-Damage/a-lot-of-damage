using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDeAparenciaTestes
    {
        [Test]
        public void Arma_comum_sem_refino_nao_brilha()
        {
            Assert.AreEqual(0, RegrasDeAparencia.BrilhoDoItem(Raridade.Comum, 0), 1e-6);
        }

        [Test]
        public void Raridade_maior_ja_brilha_sem_refino()
        {
            float anterior = -1;
            foreach (Raridade r in System.Enum.GetValues(typeof(Raridade)))
            {
                float b = RegrasDeAparencia.BrilhoDoItem(r, 0);
                Assert.Greater(b, anterior, r.ToString());
                anterior = b;
            }
            Assert.AreEqual(0.8f, RegrasDeAparencia.BrilhoDoItem(Raridade.Lendario, 0), 1e-6);
        }

        [Test]
        public void Refino_soma_brilho_por_cima_da_raridade()
        {
            Assert.AreEqual(0.5f, RegrasDeAparencia.BrilhoDoItem(Raridade.Comum, 10), 1e-6, "metade do que falta no +10");
            Assert.AreEqual(0.9f, RegrasDeAparencia.BrilhoDoItem(Raridade.Lendario, 10), 1e-6);
            for (int nivel = 1; nivel <= 200; nivel++)
                Assert.Greater(RegrasDeAparencia.BrilhoDoItem(Raridade.Raro, nivel), RegrasDeAparencia.BrilhoDoItem(Raridade.Raro, nivel - 1));
        }

        [Test]
        public void Brilho_nunca_chega_a_um_e_nivel_negativo_conta_como_zero()
        {
            Assert.Less(RegrasDeAparencia.BrilhoDoItem(Raridade.Lendario, 100000), 1);
            Assert.AreEqual(RegrasDeAparencia.BrilhoDoItem(Raridade.Raro, 0), RegrasDeAparencia.BrilhoDoItem(Raridade.Raro, -3), 1e-6);
        }

        [Test]
        public void Cada_raridade_tem_uma_cor_diferente_em_hexadecimal()
        {
            var vistas = new System.Collections.Generic.HashSet<string>();
            foreach (Raridade r in System.Enum.GetValues(typeof(Raridade)))
            {
                string cor = RegrasDosItens.CorDaRaridade(r);
                StringAssert.IsMatch("^#[0-9A-F]{6}$", cor);
                Assert.IsTrue(vistas.Add(cor), r.ToString());
            }
        }
    }
}
