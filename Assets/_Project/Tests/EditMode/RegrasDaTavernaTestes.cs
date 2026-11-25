using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaTavernaTestes
    {
        static DefinicaoDePredio Balcao() => new DefinicaoDePredio
        {
            Id = "balcao", Efeito = EfeitoDoPredio.OuroPorSegundo, BonusPorNivel = 0.02, CustoBase = 100, CrescimentoDoCusto = 1.5,
        };

        [Test]
        public void Custo_cresce_geometricamente()
        {
            Assert.AreEqual(100, RegrasDaTaverna.Custo(Balcao(), 0), 1e-9);
            Assert.AreEqual(225, RegrasDaTaverna.Custo(Balcao(), 2), 1e-9);
        }

        [Test]
        public void Ouro_por_segundo_e_uma_fracao_do_ouro_da_vitoria_por_nivel()
        {
            // 2% do ouro de uma vitória por segundo, por nível
            Assert.AreEqual(0, RegrasDaTaverna.OuroPorSegundo(Balcao(), 0, 500), 1e-9);
            Assert.AreEqual(500 * 0.02 * 3, RegrasDaTaverna.OuroPorSegundo(Balcao(), 3, 500), 1e-9);
        }

        [Test]
        public void Ouro_guardado_para_de_crescer_no_limite()
        {
            Assert.AreEqual(3600 * 10, RegrasDaTaverna.Acumulado(10, 3600, 12), 1e-9);
            Assert.AreEqual(12 * 3600 * 10, RegrasDaTaverna.Acumulado(10, 100 * 3600, 12), 1e-9);
            Assert.AreEqual(0, RegrasDaTaverna.Acumulado(10, -50, 12), 1e-9);
        }

        [Test]
        public void Bonus_do_predio_soma_por_nivel()
        {
            var cozinha = new DefinicaoDePredio { Efeito = EfeitoDoPredio.Dano, BonusPorNivel = 0.02 };
            Assert.AreEqual(0.1, RegrasDaTaverna.Bonus(cozinha, 5), 1e-9);
        }
    }
}
