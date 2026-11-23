using System.Collections.Generic;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaCampanhaTestes
    {
        static ParametrosDaCampanha Padrao() => new ParametrosDaCampanha
        {
            VidaBase = 100, CrescimentoPorEstagio = 1.25, OuroPorVida = 0.5,
            VitoriasParaAvancar = 10, ChefeACada = 5, MultiplicadorDeVidaDoChefe = 10, EstagiosPorRegiao = 10,
        };

        static readonly List<DefinicaoDeRegiao> Regioes = new List<DefinicaoDeRegiao>
        {
            new DefinicaoDeRegiao { Nome = "Campos" },
            new DefinicaoDeRegiao { Nome = "Bosque" },
        };

        [Test]
        public void Vida_cresce_geometricamente_por_estagio()
        {
            Assert.AreEqual(100, RegrasDaCampanha.VidaDoInimigo(Padrao(), 1, 1, false), 1e-9);
            Assert.AreEqual(100 * 1.25 * 1.25, RegrasDaCampanha.VidaDoInimigo(Padrao(), 3, 1, false), 1e-9);
        }

        [Test]
        public void Tipo_e_chefe_multiplicam_a_vida()
        {
            Assert.AreEqual(100 * 4 * 10, RegrasDaCampanha.VidaDoInimigo(Padrao(), 1, 4, true), 1e-9);
        }

        [Test]
        public void Ouro_e_proporcional_a_vida()
        {
            Assert.AreEqual(500, RegrasDaCampanha.Ouro(Padrao(), 1000), 1e-9);
        }

        [TestCase(4, false)]
        [TestCase(5, true)]
        [TestCase(10, true)]
        [TestCase(11, false)]
        public void Chefe_a_cada_5_estagios(int estagio, bool chefe)
        {
            Assert.AreEqual(chefe, RegrasDaCampanha.EhEstagioDeChefe(Padrao(), estagio));
        }

        [TestCase(1, "Campos")]
        [TestCase(10, "Campos")]
        [TestCase(11, "Bosque")]
        [TestCase(21, "Campos")]
        public void Regioes_mudam_a_cada_10_estagios_e_se_repetem(int estagio, string nome)
        {
            Assert.AreEqual(nome, RegrasDaCampanha.Regiao(Padrao(), Regioes, estagio).Nome);
        }

        [Test]
        public void Pontos_do_mapa_se_revezam_entre_os_monstros_da_regiao()
        {
            var regiao = new DefinicaoDeRegiao { Monstros = new List<string> { "slime", "lobo" } };
            Assert.AreEqual("slime", RegrasDaCampanha.MonstroDoPonto(regiao, 0, false));
            Assert.AreEqual("lobo", RegrasDaCampanha.MonstroDoPonto(regiao, 1, false));
            Assert.AreEqual("slime", RegrasDaCampanha.MonstroDoPonto(regiao, 2, false));
            Assert.AreEqual("lobo", RegrasDaCampanha.MonstroDoPonto(regiao, 0, true));
            Assert.IsNull(RegrasDaCampanha.MonstroDoPonto(new DefinicaoDeRegiao(), 0, false));
        }

        [Test]
        public void Dez_vitorias_avancam_o_estagio()
        {
            var r = RegrasDaCampanha.AposVitoria(Padrao(), 1, 9, chefe: false);
            Assert.IsTrue(r.Avancou);
            Assert.AreEqual(2, r.Estagio);
            Assert.AreEqual(0, r.VitoriasNoEstagio);

            r = RegrasDaCampanha.AposVitoria(Padrao(), 1, 3, chefe: false);
            Assert.IsFalse(r.Avancou);
            Assert.AreEqual(4, r.VitoriasNoEstagio);
        }

        [Test]
        public void Estagio_de_chefe_so_avanca_derrotando_o_chefe()
        {
            var r = RegrasDaCampanha.AposVitoria(Padrao(), 5, 9, chefe: false);
            Assert.IsFalse(r.Avancou);
            Assert.AreEqual(10, r.VitoriasNoEstagio);

            r = RegrasDaCampanha.AposVitoria(Padrao(), 5, 10, chefe: false);
            Assert.AreEqual(10, r.VitoriasNoEstagio);

            r = RegrasDaCampanha.AposVitoria(Padrao(), 5, 2, chefe: true);
            Assert.IsTrue(r.Avancou);
            Assert.AreEqual(6, r.Estagio);
        }
    }
}
