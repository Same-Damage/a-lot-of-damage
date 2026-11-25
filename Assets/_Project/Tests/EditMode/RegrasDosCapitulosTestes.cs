using System.Collections.Generic;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDosCapitulosTestes
    {
        static readonly List<LiberacaoDeSistema> Liberacoes = new List<LiberacaoDeSistema>
        {
            new LiberacaoDeSistema { Sistema = SistemaDoJogo.Forja, Estagio = 3 },
            new LiberacaoDeSistema { Sistema = SistemaDoJogo.Taverna, Estagio = 18 },
        };

        [Test]
        public void Sistema_abre_no_estagio_dele()
        {
            Assert.IsFalse(RegrasDosCapitulos.Liberado(Liberacoes, SistemaDoJogo.Forja, 2));
            Assert.IsTrue(RegrasDosCapitulos.Liberado(Liberacoes, SistemaDoJogo.Forja, 3));
        }

        [Test]
        public void Sistema_sem_regra_ja_vem_aberto()
        {
            Assert.IsTrue(RegrasDosCapitulos.Liberado(Liberacoes, SistemaDoJogo.Upgrades, 1));
        }

        [Test]
        public void Capitulo_do_estagio()
        {
            var capitulos = new List<DefinicaoDeCapitulo>
            {
                new DefinicaoDeCapitulo { Numero = 1, EstagioFinal = 30 },
                new DefinicaoDeCapitulo { Numero = 2, EstagioFinal = 60 },
            };
            Assert.AreEqual(1, RegrasDosCapitulos.Capitulo(capitulos, 30).Numero);
            Assert.AreEqual(2, RegrasDosCapitulos.Capitulo(capitulos, 31).Numero);
            Assert.IsNull(RegrasDosCapitulos.Capitulo(capitulos, 61), "depois do último, estágios infinitos sem capítulo");
        }
    }
}
