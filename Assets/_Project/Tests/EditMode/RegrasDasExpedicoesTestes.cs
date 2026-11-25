using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDasExpedicoesTestes
    {
        static ParametrosDoGacha Gacha() => new ParametrosDoGacha { BonusPorEstrela = 0.2, MaximoDeEstrelas = 5 };

        [Test]
        public void Follower_mais_raro_e_com_mais_estrelas_traz_mais()
        {
            var comum = new DefinicaoDeFollower { Raridade = Raridade.Comum };
            var lendario = new DefinicaoDeFollower { Raridade = Raridade.Lendario };
            Assert.AreEqual(1, RegrasDasExpedicoes.Fator(Gacha(), comum, 1), 1e-9);
            Assert.AreEqual(3, RegrasDasExpedicoes.Fator(Gacha(), lendario, 1), 1e-9);
            Assert.AreEqual(3 * 1.8, RegrasDasExpedicoes.Fator(Gacha(), lendario, 5), 1e-9);
        }

        [Test]
        public void Missao_termina_depois_das_horas_dela()
        {
            var m = new DefinicaoDeMissao { Horas = 4 };
            Assert.AreEqual(4 * 3600, RegrasDasExpedicoes.SegundosRestantes(m, 0), 1e-9);
            Assert.AreEqual(3600, RegrasDasExpedicoes.SegundosRestantes(m, 3 * 3600), 1e-9);
            Assert.AreEqual(0, RegrasDasExpedicoes.SegundosRestantes(m, 9 * 3600), 1e-9);
        }
    }
}
