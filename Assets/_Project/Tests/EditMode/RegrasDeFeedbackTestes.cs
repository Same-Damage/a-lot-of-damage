using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDeFeedbackTestes
    {
        [Test]
        public void Golpes_comuns_nao_pausam_o_jogo()
        {
            Assert.AreEqual(0, RegrasDeFeedback.HitStop(TipoDeGolpe.Click));
            Assert.AreEqual(0, RegrasDeFeedback.HitStop(TipoDeGolpe.Automatico));
            Assert.AreEqual(0, RegrasDeFeedback.HitStop(TipoDeGolpe.Follower));
        }

        [Test]
        public void Quanto_mais_forte_o_golpe_mais_feedback()
        {
            TipoDeGolpe[] ordem = { TipoDeGolpe.Automatico, TipoDeGolpe.Click, TipoDeGolpe.PontoFraco, TipoDeGolpe.Critico, TipoDeGolpe.Ultimate };
            for (int i = 1; i < ordem.Length; i++)
            {
                Assert.GreaterOrEqual(RegrasDeFeedback.HitStop(ordem[i]), RegrasDeFeedback.HitStop(ordem[i - 1]));
                Assert.Greater(RegrasDeFeedback.Particulas(ordem[i]), RegrasDeFeedback.Particulas(ordem[i - 1]));
                Assert.Greater(RegrasDeFeedback.Tremor(ordem[i]), RegrasDeFeedback.Tremor(ordem[i - 1]));
            }
        }

        [Test]
        public void Hit_stop_e_curto_para_nao_travar_o_jogo()
        {
            Assert.LessOrEqual(RegrasDeFeedback.HitStop(TipoDeGolpe.Ultimate), 0.15f);
        }

        [Test]
        public void Golpe_mais_forte_prevalece()
        {
            Assert.AreEqual(TipoDeGolpe.Critico, RegrasDeFeedback.MaisForte(TipoDeGolpe.Critico, TipoDeGolpe.Click));
            Assert.AreEqual(TipoDeGolpe.Ultimate, RegrasDeFeedback.MaisForte(TipoDeGolpe.Click, TipoDeGolpe.Ultimate));
        }
    }
}
