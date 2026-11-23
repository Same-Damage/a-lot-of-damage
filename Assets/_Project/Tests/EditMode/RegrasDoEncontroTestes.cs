using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDoEncontroTestes
    {
        [Test]
        public void Atacar_monstro_que_nao_viu_o_jogador_e_ataque_preventivo()
        {
            Assert.AreEqual(Vantagem.Preventivo, RegrasDoEncontro.AoAtacar(inimigoPerseguindo: false));
        }

        [Test]
        public void Atacar_monstro_que_ja_persegue_e_batalha_normal()
        {
            Assert.AreEqual(Vantagem.Normal, RegrasDoEncontro.AoAtacar(inimigoPerseguindo: true));
        }

        [Test]
        public void Ser_alcancado_pelo_monstro_e_emboscada()
        {
            Assert.AreEqual(Vantagem.Emboscada, RegrasDoEncontro.AoSerAlcancado());
        }

        [Test]
        public void Monstro_comeca_a_perseguir_dentro_do_raio_fora_da_protecao()
        {
            Assert.IsTrue(RegrasDoEncontro.DevePerseguir(false, 5f, 6f, 10f, emProtecao: false));
        }

        [Test]
        public void Protecao_apos_batalha_impede_inicio_da_perseguicao()
        {
            Assert.IsFalse(RegrasDoEncontro.DevePerseguir(false, 5f, 6f, 10f, emProtecao: true));
        }

        [Test]
        public void Monstro_desiste_quando_o_jogador_se_afasta()
        {
            Assert.IsFalse(RegrasDoEncontro.DevePerseguir(true, 11f, 6f, 10f, emProtecao: false));
            Assert.IsTrue(RegrasDoEncontro.DevePerseguir(true, 8f, 6f, 10f, emProtecao: false));
        }

        [Test]
        public void Monstro_derrotado_renasce_depois_do_tempo()
        {
            Assert.IsFalse(RegrasDoEncontro.Renasceu(100f, 129f, 30f));
            Assert.IsTrue(RegrasDoEncontro.Renasceu(100f, 130f, 30f));
        }
    }
}
