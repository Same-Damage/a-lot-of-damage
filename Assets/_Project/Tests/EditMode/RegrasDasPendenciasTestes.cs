using System.Collections.Generic;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes.EditMode
{
    public class RegrasDasPendenciasTestes
    {
        static List<Pendencia> Lista(params TipoDePendencia[] tipos)
        {
            var lista = new List<Pendencia>();
            foreach (var t in tipos) lista.Add(new Pendencia { Tipo = t, Texto = t.ToString() });
            return lista;
        }

        [Test]
        public void Conta_so_as_pendencias_do_tipo()
        {
            var lista = Lista(TipoDePendencia.Upgrade, TipoDePendencia.Forja, TipoDePendencia.Upgrade);
            Assert.AreEqual(2, RegrasDasPendencias.Quantas(lista, TipoDePendencia.Upgrade));
            Assert.AreEqual(0, RegrasDasPendencias.Quantas(lista, TipoDePendencia.Chefe));
        }

        [TestCase(0, "")]
        [TestCase(3, "3")]
        [TestCase(9, "9")]
        [TestCase(10, "9+")]
        public void Selo_mostra_ate_nove(int quantas, string esperado) =>
            Assert.AreEqual(esperado, RegrasDasPendencias.Selo(quantas));

        [Test]
        public void Resumo_mostra_a_primeira_e_quantas_mais()
        {
            Assert.AreEqual("", RegrasDasPendencias.Resumo(Lista()));
            Assert.AreEqual("Chefe", RegrasDasPendencias.Resumo(Lista(TipoDePendencia.Chefe)));
            Assert.AreEqual("Chefe  +2", RegrasDasPendencias.Resumo(Lista(TipoDePendencia.Chefe, TipoDePendencia.Forja, TipoDePendencia.Upgrade)));
        }
    }
}
