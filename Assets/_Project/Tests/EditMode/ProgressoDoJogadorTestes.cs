using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class ProgressoDoJogadorTestes
    {
        static readonly DefinicaoDeUpgrade Forca = new DefinicaoDeUpgrade
        {
            Id = "teste_forca", CustoBase = 10, CrescimentoDoCusto = 1.07, ValorBase = 3,
        };

        [SetUp]
        public void Limpar() => ProgressoDoJogador.Reiniciar();

        [Test]
        public void Upgrade_comeca_no_nivel_1()
        {
            Assert.AreEqual(1, ProgressoDoJogador.Nivel(Forca.Id));
            Assert.AreEqual(3, ProgressoDoJogador.Valor(Forca), 1e-9);
        }

        [Test]
        public void Compra_guarda_o_nivel_e_desconta_o_ouro()
        {
            ProgressoDoJogador.Ouro = 100;
            ProgressoDoJogador.Comprar(Forca, 1);
            Assert.AreEqual(2, ProgressoDoJogador.Nivel(Forca.Id));
            Assert.AreEqual(90, ProgressoDoJogador.Ouro, 1e-9);
        }

        [Test]
        public void Compra_sem_ouro_nao_muda_nada()
        {
            ProgressoDoJogador.Ouro = 5;
            var r = ProgressoDoJogador.Comprar(Forca, 1);
            Assert.IsFalse(r.Comprou);
            Assert.AreEqual(1, ProgressoDoJogador.Nivel(Forca.Id));
            Assert.AreEqual(5, ProgressoDoJogador.Ouro, 1e-9);
        }

        [Test]
        public void Balanceamento_padrao_tem_os_tres_upgrades_do_prototipo()
        {
            var b = BalanceamentoDeUpgrades.Padrao();
            Assert.IsNotNull(b.Find(u => u.Id == BalanceamentoDeUpgrades.IdForca));
            Assert.IsNotNull(b.Find(u => u.Id == BalanceamentoDeUpgrades.IdAutomatico));
            Assert.IsNotNull(b.Find(u => u.Id == BalanceamentoDeUpgrades.IdPrecisao));
        }
    }
}
