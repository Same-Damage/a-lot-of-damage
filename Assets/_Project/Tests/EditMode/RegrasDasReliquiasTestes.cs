using System.Collections.Generic;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDasReliquiasTestes
    {
        static ParametrosDasReliquias Parametros() => new ParametrosDasReliquias
        {
            FragmentosPorChefe = 3, EstagiosPorFragmentoExtra = 10, NivelMaximo = 10, CustoBaseDeMelhoria = 10,
        };

        [Test]
        public void Chefe_da_mais_fragmentos_em_estagios_altos()
        {
            Assert.AreEqual(3, RegrasDasReliquias.FragmentosDoChefe(Parametros(), 5));
            Assert.AreEqual(4, RegrasDasReliquias.FragmentosDoChefe(Parametros(), 10));
            Assert.AreEqual(8, RegrasDasReliquias.FragmentosDoChefe(Parametros(), 55));
        }

        [Test]
        public void Montar_custa_o_preco_da_reliquia_e_melhorar_cresce_com_o_nivel()
        {
            var r = new DefinicaoDeReliquia { CustoParaMontar = 20 };
            Assert.AreEqual(20, RegrasDasReliquias.Custo(Parametros(), r, 0));
            Assert.AreEqual(10, RegrasDasReliquias.Custo(Parametros(), r, 1));
            Assert.AreEqual(90, RegrasDasReliquias.Custo(Parametros(), r, 9));
            Assert.AreEqual(-1, RegrasDasReliquias.Custo(Parametros(), r, 10), "no máximo não melhora");
        }

        [Test]
        public void Reliquias_do_mesmo_alvo_se_multiplicam()
        {
            var a = new DefinicaoDeReliquia { Id = "a", Alvo = AlvoDaReliquia.Click, BonusPorNivel = 0.5 };
            var b = new DefinicaoDeReliquia { Id = "b", Alvo = AlvoDaReliquia.Click, BonusPorNivel = 0.25 };
            var c = new DefinicaoDeReliquia { Id = "c", Alvo = AlvoDaReliquia.Ouro, BonusPorNivel = 1 };
            var niveis = new Dictionary<string, int> { ["a"] = 2, ["b"] = 4, ["c"] = 3 };
            int Nivel(string id) => niveis.TryGetValue(id, out var n) ? n : 0;
            var lista = new[] { a, b, c };

            Assert.AreEqual(2 * 2, RegrasDasReliquias.Multiplicador(lista, Nivel, AlvoDaReliquia.Click), 1e-9);
            Assert.AreEqual(4, RegrasDasReliquias.Multiplicador(lista, Nivel, AlvoDaReliquia.Ouro), 1e-9);
            Assert.AreEqual(1, RegrasDasReliquias.Multiplicador(lista, Nivel, AlvoDaReliquia.Afk), 1e-9);
        }

        [Test]
        public void Reliquia_nao_montada_nao_da_bonus()
        {
            var a = new DefinicaoDeReliquia { Id = "a", Alvo = AlvoDaReliquia.Click, BonusPorNivel = 0.5 };
            Assert.AreEqual(1, RegrasDasReliquias.Multiplicador(new[] { a }, _ => 0, AlvoDaReliquia.Click), 1e-9);
        }
    }
}
