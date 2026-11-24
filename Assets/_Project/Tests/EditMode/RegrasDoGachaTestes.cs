using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDoGachaTestes
    {
        static ParametrosDoGacha Parametros() => new ParametrosDoGacha
        {
            Taxas = new double[] { 50, 30, 14, 5, 1 }, PityEpico = 30, PityLendario = 90, BonusPorEstrela = 0.2, MaximoDeEstrelas = 5,
        };

        [Test]
        public void Raridade_segue_as_taxas_publicas()
        {
            var p = Parametros();
            Assert.AreEqual(Raridade.Comum, RegrasDoGacha.SortearRaridade(p, 0.49, 0, 0));
            Assert.AreEqual(Raridade.Incomum, RegrasDoGacha.SortearRaridade(p, 0.5, 0, 0));
            Assert.AreEqual(Raridade.Raro, RegrasDoGacha.SortearRaridade(p, 0.9, 0, 0));
            Assert.AreEqual(Raridade.Epico, RegrasDoGacha.SortearRaridade(p, 0.97, 0, 0));
            Assert.AreEqual(Raridade.Lendario, RegrasDoGacha.SortearRaridade(p, 0.995, 0, 0));
        }

        [Test]
        public void Trigesima_sem_epico_e_garantida_epico()
        {
            Assert.AreEqual(Raridade.Epico, RegrasDoGacha.SortearRaridade(Parametros(), 0.0, 29, 29));
            Assert.AreEqual(Raridade.Comum, RegrasDoGacha.SortearRaridade(Parametros(), 0.0, 28, 28));
        }

        [Test]
        public void Nonagesima_sem_lendario_e_garantida_lendario()
        {
            Assert.AreEqual(Raridade.Lendario, RegrasDoGacha.SortearRaridade(Parametros(), 0.0, 0, 89));
        }

        [Test]
        public void Contadores_do_pity_zeram_quando_sai_o_raro()
        {
            var (semEpico, semLendario) = RegrasDoGacha.AposInvocar(Raridade.Raro, 10, 10);
            Assert.AreEqual((11, 11), (semEpico, semLendario));
            Assert.AreEqual((0, 11), RegrasDoGacha.AposInvocar(Raridade.Epico, 10, 10));
            Assert.AreEqual((0, 0), RegrasDoGacha.AposInvocar(Raridade.Lendario, 10, 10));
        }

        [Test]
        public void Cada_estrela_acima_da_primeira_soma_vinte_por_cento()
        {
            Assert.AreEqual(1, RegrasDoGacha.MultiplicadorDasEstrelas(Parametros(), 1), 1e-9);
            Assert.AreEqual(1.8, RegrasDoGacha.MultiplicadorDasEstrelas(Parametros(), 5), 1e-9);
            Assert.AreEqual(1.8, RegrasDoGacha.MultiplicadorDasEstrelas(Parametros(), 9), 1e-9, "não passa do máximo de estrelas");
        }

        [Test]
        public void Efeito_do_follower_so_vale_para_o_tipo_dele_e_cresce_com_estrelas()
        {
            var f = new DefinicaoDeFollower { Efeito = EfeitoDoFollower.Ouro, ValorDoEfeito = 0.25, FracaoDoAutoAtaque = 0.2 };
            Assert.AreEqual(0.25, RegrasDoGacha.Efeito(Parametros(), f, 1, EfeitoDoFollower.Ouro), 1e-9);
            Assert.AreEqual(0.25 * 1.4, RegrasDoGacha.Efeito(Parametros(), f, 3, EfeitoDoFollower.Ouro), 1e-9);
            Assert.AreEqual(0, RegrasDoGacha.Efeito(Parametros(), f, 3, EfeitoDoFollower.Critico), 1e-9);
            Assert.AreEqual(0, RegrasDoGacha.Efeito(Parametros(), null, 3, EfeitoDoFollower.Ouro), 1e-9);
        }

        [Test]
        public void Fracao_de_dano_cresce_com_estrelas_e_soma_o_dano_extra()
        {
            var comum = new DefinicaoDeFollower { FracaoDoAutoAtaque = 0.2 };
            Assert.AreEqual(0.2 * 1.4, RegrasDoGacha.FracaoDoAutoAtaque(Parametros(), comum, 3), 1e-9);
            var guerreiro = new DefinicaoDeFollower { FracaoDoAutoAtaque = 0.2, Efeito = EfeitoDoFollower.DanoExtra, ValorDoEfeito = 0.3 };
            Assert.AreEqual(0.5, RegrasDoGacha.FracaoDoAutoAtaque(Parametros(), guerreiro, 1), 1e-9);
        }
    }
}
