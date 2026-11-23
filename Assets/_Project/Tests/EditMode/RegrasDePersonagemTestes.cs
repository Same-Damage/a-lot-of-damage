using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDePersonagemTestes
    {
        static DefinicaoDeClasse Guerreiro() => new DefinicaoDeClasse
        {
            Id = "guerreiro", BonusDeFuriaPorAcumulo = 0.05, CombosPorAcumulo = 10, MaximoDeAcumulos = 10,
        };

        static DefinicaoDeFollower Falcao() => new DefinicaoDeFollower
        {
            Id = "pip", FracaoDoAutoAtaque = 0.2, BonusNaJanelaDoPontoFraco = 0.3,
        };

        [TestCase(0, 0)]
        [TestCase(9, 0)]
        [TestCase(10, 1)]
        [TestCase(35, 3)]
        [TestCase(500, 10)]
        public void Furia_acumula_a_cada_10_de_combo_ate_o_maximo(int combo, int esperado)
        {
            Assert.AreEqual(esperado, RegrasDePersonagem.AcumulosDeFuria(Guerreiro(), combo));
        }

        [Test]
        public void Cada_acumulo_de_furia_da_5_por_cento()
        {
            Assert.AreEqual(1, RegrasDePersonagem.MultiplicadorDeFuria(Guerreiro(), 5), 1e-9);
            Assert.AreEqual(1.15, RegrasDePersonagem.MultiplicadorDeFuria(Guerreiro(), 30), 1e-9);
            Assert.AreEqual(1.5, RegrasDePersonagem.MultiplicadorDeFuria(Guerreiro(), 999), 1e-9);
        }

        [Test]
        public void Classe_sem_furia_nao_muda_o_dano()
        {
            var semFuria = new DefinicaoDeClasse { CombosPorAcumulo = 0 };
            Assert.AreEqual(1, RegrasDePersonagem.MultiplicadorDeFuria(semFuria, 100), 1e-9);
        }

        [Test]
        public void Follower_ataca_com_fracao_do_auto_ataque()
        {
            Assert.AreEqual(20, RegrasDePersonagem.DanoDoFollower(Falcao(), 100), 1e-9);
            Assert.AreEqual(0, RegrasDePersonagem.DanoDoFollower(null, 100), 1e-9);
        }

        [Test]
        public void Follower_alonga_a_janela_do_ponto_fraco()
        {
            Assert.AreEqual(1.3, RegrasDePersonagem.DuracaoDoPontoFraco(Falcao(), 1), 1e-6);
            Assert.AreEqual(1, RegrasDePersonagem.DuracaoDoPontoFraco(null, 1), 1e-6);
        }
    }
}
