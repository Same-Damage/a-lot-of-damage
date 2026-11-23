using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDoClickTestes
    {
        static ParametrosDoClick Padrao() => new ParametrosDoClick
        {
            DanoBase = 10,
            BonusDeComboPorHit = 0.02,
            MultiplicadorCritico = 5,
            MultiplicadorPontoFraco = 10,
        };

        [Test]
        public void Sem_combo_critico_ou_ponto_fraco_e_o_dano_base()
        {
            Assert.AreEqual(10, RegrasDoClick.Dano(Padrao(), 0, false, false), 1e-9);
        }

        [Test]
        public void Combo_soma_2_por_cento_por_hit()
        {
            Assert.AreEqual(12, RegrasDoClick.Dano(Padrao(), 10, false, false), 1e-9);
        }

        [Test]
        public void Critico_e_ponto_fraco_se_multiplicam()
        {
            Assert.AreEqual(10 * 5 * 10, RegrasDoClick.Dano(Padrao(), 0, true, true), 1e-9);
        }
    }

    public class LimitadorDeCliquesTestes
    {
        [Test]
        public void Aceita_ate_o_limite_dentro_de_um_segundo()
        {
            var l = new LimitadorDeCliques(3);
            Assert.IsTrue(l.Registrar(0f));
            Assert.IsTrue(l.Registrar(0.1f));
            Assert.IsTrue(l.Registrar(0.2f));
            Assert.IsFalse(l.Registrar(0.3f));
        }

        [Test]
        public void Libera_de_novo_depois_que_o_segundo_passa()
        {
            var l = new LimitadorDeCliques(1);
            Assert.IsTrue(l.Registrar(0f));
            Assert.IsFalse(l.Registrar(0.5f));
            Assert.IsTrue(l.Registrar(1.01f));
        }

        [Test]
        public void Clique_recusado_nao_conta_para_o_limite()
        {
            var l = new LimitadorDeCliques(1);
            l.Registrar(0f);
            l.Registrar(0.9f); // recusado
            Assert.IsTrue(l.Registrar(1.01f));
        }
    }
}
