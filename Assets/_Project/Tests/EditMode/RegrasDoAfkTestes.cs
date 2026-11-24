using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDoAfkTestes
    {
        static ParametrosDaCampanha Parametros() => new ParametrosDaCampanha
        {
            FracaoDoAfk = 0.25, LimiteDoAfkEmHoras = 12, SegundosEntreLutasNoAfk = 5,
        };

        [Test]
        public void Tempo_fora_e_limitado_a_12_horas()
        {
            Assert.AreEqual(3600, RegrasDoAfk.SegundosQueContam(Parametros(), 3600), 1e-9);
            Assert.AreEqual(12 * 3600, RegrasDoAfk.SegundosQueContam(Parametros(), 30 * 3600), 1e-9);
        }

        [Test]
        public void Tempo_negativo_nao_conta()
        {
            Assert.AreEqual(0, RegrasDoAfk.SegundosQueContam(Parametros(), -500), 1e-9);
        }

        [Test]
        public void Vitorias_sao_as_lutas_do_tempo_vezes_a_fracao_do_afk()
        {
            // dano 10/s contra 50 de vida: 5 s de luta + 5 s entre lutas = 10 s por vitória; 1 h = 360 lutas; 25% = 90
            Assert.AreEqual(90, RegrasDoAfk.Vitorias(Parametros(), 3600, 10, 50), 1e-9);
        }

        [Test]
        public void Sem_dano_automatico_nao_ha_vitorias()
        {
            Assert.AreEqual(0, RegrasDoAfk.Vitorias(Parametros(), 3600, 0, 50), 1e-9);
        }

        [Test]
        public void Itens_do_afk_seguem_a_chance_de_drop_das_vitorias()
        {
            Assert.AreEqual(3, RegrasDoAfk.Itens(10.9, 0.3));
            Assert.AreEqual(0, RegrasDoAfk.Itens(2, 0.3));
        }
    }
}
