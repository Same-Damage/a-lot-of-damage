using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class FormatadorDeNumerosTestes
    {
        [TearDown]
        public void Limpar() => FormatadorDeNumeros.NotacaoCientifica = false;

        [TestCase(0, "0")]
        [TestCase(845.9, "845")]
        [TestCase(845320, "845.320")]
        [TestCase(999999, "999.999")]
        public void Ate_999999_mostra_o_numero_completo(double valor, string esperado)
        {
            Assert.AreEqual(esperado, FormatadorDeNumeros.Formatar(valor));
        }

        [TestCase(1_000_000, "1 M")]
        [TestCase(12_400_000, "12,4 M")]
        [TestCase(3.51e12, "3,51 T")]
        [TestCase(870e9, "870 B")]
        public void Usa_sufixos_curtos_ate_trilhao(double valor, string esperado)
        {
            Assert.AreEqual(esperado, FormatadorDeNumeros.Formatar(valor));
        }

        [TestCase(4.7e15, "4,7 aa")]
        [TestCase(1e18, "1 ab")]
        public void Depois_de_trilhao_usa_letras(double valor, string esperado)
        {
            Assert.AreEqual(esperado, FormatadorDeNumeros.Formatar(valor));
        }

        [Test]
        public void Grupo_26_depois_de_T_e_az()
        {
            Assert.AreEqual("1 az", FormatadorDeNumeros.Formatar(System.Math.Pow(1000, 5 + 25)));
        }

        [Test]
        public void Depois_de_zz_passa_para_tres_letras()
        {
            // aa..zz são 26*26 grupos depois de T (grupo 5); esse valor não cabe em double
            Assert.AreEqual("zz", FormatadorDeNumeros.Sufixo(5 + 26 * 26 - 1));
            Assert.AreEqual("aaa", FormatadorDeNumeros.Sufixo(5 + 26 * 26));
        }

        [Test]
        public void Arredondamento_nao_mostra_1000_do_sufixo_anterior()
        {
            Assert.AreEqual("1 B", FormatadorDeNumeros.Formatar(999_999_999));
        }

        [Test]
        public void Negativo_mantem_o_sinal()
        {
            Assert.AreEqual("-12,4 M", FormatadorDeNumeros.Formatar(-12_400_000));
        }

        [Test]
        public void Notacao_cientifica_quando_ativada()
        {
            FormatadorDeNumeros.NotacaoCientifica = true;
            Assert.AreEqual("4,7e15", FormatadorDeNumeros.Formatar(4.7e15));
        }
    }
}
