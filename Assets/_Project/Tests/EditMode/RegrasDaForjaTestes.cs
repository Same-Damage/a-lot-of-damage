using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaForjaTestes
    {
        static DefinicaoDeItem Espada() => new DefinicaoDeItem
        {
            Id = "espada", Alvo = AlvoDoItem.Click, BonusPorNivel = 0.25,
            CustoBaseEmOuro = 100, CrescimentoDoCusto = 1.5,
            MaterialId = "presa", MaterialPorNivel = 2, ChanceDeSucessoCritico = 0.1,
        };

        [Test]
        public void Item_sem_forja_nao_muda_o_dano()
        {
            Assert.AreEqual(1, RegrasDaForja.Multiplicador(Espada(), 0), 1e-9);
        }

        [Test]
        public void Cada_nivel_multiplica_o_bonus()
        {
            Assert.AreEqual(1.25, RegrasDaForja.Multiplicador(Espada(), 1), 1e-9);
            Assert.AreEqual(1.25 * 1.25 * 1.25, RegrasDaForja.Multiplicador(Espada(), 3), 1e-9);
        }

        [Test]
        public void Custo_em_ouro_cresce_geometricamente()
        {
            Assert.AreEqual(100, RegrasDaForja.CustoEmOuro(Espada(), 0), 1e-9);
            Assert.AreEqual(225, RegrasDaForja.CustoEmOuro(Espada(), 2), 1e-9);
        }

        [Test]
        public void Custo_em_material_cresce_por_nivel()
        {
            Assert.AreEqual(2, RegrasDaForja.CustoEmMaterial(Espada(), 0));
            Assert.AreEqual(6, RegrasDaForja.CustoEmMaterial(Espada(), 2));
        }

        [Test]
        public void Forjar_sobe_um_nivel_e_desconta_ouro_e_material()
        {
            var r = RegrasDaForja.Forjar(Espada(), 0, 150, 5, 0.9);
            Assert.IsTrue(r.Forjou);
            Assert.IsFalse(r.SucessoCritico);
            Assert.AreEqual(1, r.NovoNivel);
            Assert.AreEqual(50, r.OuroRestante, 1e-9);
            Assert.AreEqual(3, r.MaterialRestante);
        }

        [Test]
        public void Sorteio_abaixo_da_chance_da_sucesso_critico_com_dois_niveis()
        {
            var r = RegrasDaForja.Forjar(Espada(), 0, 150, 5, 0.05);
            Assert.IsTrue(r.SucessoCritico);
            Assert.AreEqual(2, r.NovoNivel);
        }

        [Test]
        public void Sem_ouro_nao_forja()
        {
            var r = RegrasDaForja.Forjar(Espada(), 0, 99, 5, 0.9);
            Assert.IsFalse(r.Forjou);
            Assert.AreEqual(0, r.NovoNivel);
            Assert.AreEqual(99, r.OuroRestante, 1e-9);
            Assert.AreEqual(5, r.MaterialRestante);
        }

        [Test]
        public void Sem_material_nao_forja()
        {
            var r = RegrasDaForja.Forjar(Espada(), 0, 1000, 1, 0.9);
            Assert.IsFalse(r.Forjou);
        }
    }
}
