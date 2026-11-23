using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDePoderTestes
    {
        [Test]
        public void Poder_soma_click_e_auto_com_o_critico_medio()
        {
            var a = new AtributosDeCombate { DanoDoClick = 10, DanoAutomatico = 30, ChanceCritica = 0.25 };
            // crítico ×5 com 25% de chance: fator médio 1 + 0,25 × 4 = 2
            Assert.AreEqual(80, RegrasDePoder.Poder(a, 5), 1e-9);
        }

        [Test]
        public void Sem_critico_o_poder_e_a_soma()
        {
            var a = new AtributosDeCombate { DanoDoClick = 3, DanoAutomatico = 5 };
            Assert.AreEqual(8, RegrasDePoder.Poder(a, 5), 1e-9);
        }

        [TestCase(100, 138, "+38%")]
        [TestCase(100, 100.4, "+0,4%")]
        [TestCase(100, 400, "×4")]
        [TestCase(100, 250, "×2,5")]
        [TestCase(1, 5e6, "×5 M")]
        public void Ganho_relativo_em_porcentagem_ou_multiplicador(double antes, double depois, string esperado)
        {
            Assert.AreEqual(esperado, RegrasDePoder.TextoDoGanho(antes, depois));
        }

        [Test]
        public void Sem_ganho_nao_tem_texto()
        {
            Assert.IsNull(RegrasDePoder.TextoDoGanho(100, 100));
            Assert.IsNull(RegrasDePoder.TextoDoGanho(100, 90));
            Assert.IsNull(RegrasDePoder.TextoDoGanho(0, 50));
        }

        [Test]
        public void Contador_anda_em_escala_logaritmica_ate_o_alvo()
        {
            // Com rapidez ln 2, em 1 s percorre metade da distância em log: de 1 a 1.000.000 passa por 1.000
            double meio = RegrasDePoder.ContadorAnimado(1, 1e6, 1, System.Math.Log(2));
            Assert.AreEqual(1000, meio, 1e-6);
            // Perto do alvo, encosta nele
            Assert.AreEqual(1e6, RegrasDePoder.ContadorAnimado(1, 1e6, 30, 1), 1e-9);
        }

        [Test]
        public void Contador_tambem_desce_e_lida_com_zero()
        {
            Assert.AreEqual(100, RegrasDePoder.ContadorAnimado(1000, 100, 30, 1), 1e-9);
            Assert.AreEqual(25, RegrasDePoder.ContadorAnimado(0, 50, 1, System.Math.Log(2)), 1e-9);
        }
    }
}
