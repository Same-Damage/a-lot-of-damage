using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDosItensTestes
    {
        static ParametrosDosItens Parametros() => new ParametrosDosItens
        {
            ChanceDeDrop = 0.3,
            PesosDaRaridade = new double[] { 60, 25, 10, 4, 1 },
            RaridadeMinimaDoChefe = Raridade.Incomum,
            BonusDaRaridade = new[] { 0.1, 0.25, 0.5, 1, 2 },
            CrescimentoPorEstagio = 1.05,
            CrescimentoDoPreco = 1.15,
            CustoDeFabricarEmOuro = 100,
            PrecoDeVenda = new double[] { 10, 30, 90, 300, 1000 },
            MaterialAoDesmontar = new[] { 1, 2, 4, 8, 16 },
        };

        static DefinicaoDeItem Espada() => new DefinicaoDeItem { Id = "espada", Slot = SlotDeItem.Arma, Alvo = AlvoDoItem.Click, BonusPorNivel = 0.5 };

        [Test]
        public void Monstro_comum_dropa_so_abaixo_da_chance()
        {
            Assert.IsTrue(RegrasDosItens.CaiItem(Parametros(), 0.29, false));
            Assert.IsFalse(RegrasDosItens.CaiItem(Parametros(), 0.3, false));
        }

        [Test]
        public void Chefe_sempre_dropa()
        {
            Assert.IsTrue(RegrasDosItens.CaiItem(Parametros(), 0.99, true));
        }

        [Test]
        public void Raridade_segue_os_pesos_acumulados()
        {
            var p = Parametros(); // total 100: comum até 0,60, incomum até 0,85, raro até 0,95, épico até 0,99
            Assert.AreEqual(Raridade.Comum, RegrasDosItens.SortearRaridade(p, 0.0, false));
            Assert.AreEqual(Raridade.Comum, RegrasDosItens.SortearRaridade(p, 0.59, false));
            Assert.AreEqual(Raridade.Incomum, RegrasDosItens.SortearRaridade(p, 0.6, false));
            Assert.AreEqual(Raridade.Raro, RegrasDosItens.SortearRaridade(p, 0.9, false));
            Assert.AreEqual(Raridade.Epico, RegrasDosItens.SortearRaridade(p, 0.97, false));
            Assert.AreEqual(Raridade.Lendario, RegrasDosItens.SortearRaridade(p, 0.995, false));
        }

        [Test]
        public void Chefe_nunca_dropa_abaixo_da_raridade_minima()
        {
            Assert.AreEqual(Raridade.Incomum, RegrasDosItens.SortearRaridade(Parametros(), 0.0, true));
            Assert.AreEqual(Raridade.Lendario, RegrasDosItens.SortearRaridade(Parametros(), 0.995, true));
        }

        [Test]
        public void Sorteio_do_tipo_cobre_todos_os_tipos_sem_estourar()
        {
            Assert.AreEqual(0, RegrasDosItens.SortearTipo(8, 0.0));
            Assert.AreEqual(4, RegrasDosItens.SortearTipo(8, 0.5));
            Assert.AreEqual(7, RegrasDosItens.SortearTipo(8, 0.999999));
        }

        [Test]
        public void Bonus_cresce_com_raridade_estagio_e_refino()
        {
            var p = Parametros();
            var comum = new ItemDoJogador { Raridade = Raridade.Comum, Estagio = 1 };
            Assert.AreEqual(1.1, RegrasDosItens.Multiplicador(p, Espada(), comum), 1e-9);

            var lendario = new ItemDoJogador { Raridade = Raridade.Lendario, Estagio = 1 };
            Assert.AreEqual(3, RegrasDosItens.Multiplicador(p, Espada(), lendario), 1e-9);

            var deEstagioAlto = new ItemDoJogador { Raridade = Raridade.Comum, Estagio = 3 };
            Assert.AreEqual(1 + 0.1 * 1.05 * 1.05, RegrasDosItens.Multiplicador(p, Espada(), deEstagioAlto), 1e-9);

            var refinado = new ItemDoJogador { Raridade = Raridade.Comum, Estagio = 1, Nivel = 2 };
            Assert.AreEqual(1.1 * 1.5 * 1.5, RegrasDosItens.Multiplicador(p, Espada(), refinado), 1e-9);
        }

        [Test]
        public void Venda_vale_mais_com_raridade_estagio_e_refino()
        {
            var p = Parametros();
            Assert.AreEqual(10, RegrasDosItens.PrecoDeVenda(p, new ItemDoJogador { Raridade = Raridade.Comum, Estagio = 1 }), 1e-9);
            Assert.AreEqual(1000, RegrasDosItens.PrecoDeVenda(p, new ItemDoJogador { Raridade = Raridade.Lendario, Estagio = 1 }), 1e-9);
            Assert.AreEqual(10 * 1.15, RegrasDosItens.PrecoDeVenda(p, new ItemDoJogador { Raridade = Raridade.Comum, Estagio = 2 }), 1e-9);
            Assert.AreEqual(10 * 2, RegrasDosItens.PrecoDeVenda(p, new ItemDoJogador { Raridade = Raridade.Comum, Estagio = 1, Nivel = 2 }), 1e-9);
        }

        [Test]
        public void Desmontar_devolve_material_pela_raridade_e_refino()
        {
            var p = Parametros();
            Assert.AreEqual(1, RegrasDosItens.MaterialAoDesmontar(p, new ItemDoJogador { Raridade = Raridade.Comum }));
            Assert.AreEqual(8 + 3, RegrasDosItens.MaterialAoDesmontar(p, new ItemDoJogador { Raridade = Raridade.Epico, Nivel = 3 }));
        }

        [Test]
        public void Fabricar_custa_mais_em_estagios_altos()
        {
            Assert.AreEqual(100, RegrasDosItens.CustoDeFabricar(Parametros(), 1), 1e-9);
            Assert.AreEqual(100 * 1.15 * 1.15, RegrasDosItens.CustoDeFabricar(Parametros(), 3), 1e-9);
        }
    }
}
