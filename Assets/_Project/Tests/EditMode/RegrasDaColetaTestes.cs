using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaColetaTestes
    {
        [Test]
        public void Quantidade_cresce_um_a_cada_dez_estagios()
        {
            var erva = new DefinicaoDeRecurso { QuantidadeBase = 2 };
            Assert.AreEqual(2, RegrasDaColeta.Quantidade(erva, 1));
            Assert.AreEqual(3, RegrasDaColeta.Quantidade(erva, 10));
            Assert.AreEqual(7, RegrasDaColeta.Quantidade(erva, 55));
        }

        [Test]
        public void Ponto_esgota_depois_dos_golpes()
        {
            var erva = new DefinicaoDeRecurso { Golpes = 5 };
            Assert.IsFalse(RegrasDaColeta.Esgotou(erva, 4));
            Assert.IsTrue(RegrasDaColeta.Esgotou(erva, 5));
        }
    }
}
