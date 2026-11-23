using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDeAnimacaoTestes
    {
        [Test]
        public void Parado_so_respira()
        {
            for (float t = 0; t < 3; t += 0.1f)
            {
                var p = RegrasDeAnimacao.Pose(EstadoDoBoneco.Parado, t, 0);
                Assert.Less(System.Math.Abs(p.BracoFrente), 5);
                Assert.Less(System.Math.Abs(p.PernaFrente), 1);
                Assert.Less(System.Math.Abs(p.Elevacao), 0.03f);
            }
        }

        [Test]
        public void Andando_as_pernas_vao_em_sentidos_opostos_e_alternam()
        {
            float ciclo = RegrasDeAnimacao.CicloDoPasso;
            var a = RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, ciclo * 0.25f, 0);
            var b = RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, ciclo * 0.75f, 0);

            Assert.Less(a.PernaFrente * a.PernaTras, 0, "uma perna para cada lado");
            Assert.Less(a.PernaFrente * b.PernaFrente, 0, "meio passo depois inverte");
            Assert.Greater(System.Math.Abs(a.PernaFrente), 10);
        }

        [Test]
        public void Golpe_levanta_a_espada_e_depois_desce()
        {
            var preparo = RegrasDeAnimacao.Pose(EstadoDoBoneco.Golpeando, 0, RegrasDeAnimacao.FimDoPreparo);
            var corte = RegrasDeAnimacao.Pose(EstadoDoBoneco.Golpeando, 0, RegrasDeAnimacao.FimDoCorte);
            // O desenho olha para a direita: erguer é girar no sentido horário (negativo) e cortar, para a frente (positivo)
            Assert.Less(preparo.BracoFrente, -100, "braço erguido");
            Assert.Greater(corte.BracoFrente, 40, "braço passou para a frente");
        }

        [Test]
        public void Golpe_termina_na_pose_de_repouso()
        {
            var fim = RegrasDeAnimacao.Pose(EstadoDoBoneco.Golpeando, 0, 1);
            var repouso = RegrasDeAnimacao.Pose(EstadoDoBoneco.Parado, 0, 0);
            Assert.AreEqual(repouso.BracoFrente, fim.BracoFrente, 1);
            Assert.AreEqual(repouso.Espada, fim.Espada, 1);
        }

        [Test]
        public void Dano_inclina_para_tras_e_volta()
        {
            var inicio = RegrasDeAnimacao.Pose(EstadoDoBoneco.LevandoDano, 0, 0);
            var fim = RegrasDeAnimacao.Pose(EstadoDoBoneco.LevandoDano, 0, 1);
            Assert.Greater(inicio.Tronco, 5);
            Assert.AreEqual(0, fim.Tronco, 1);
        }

        // Regressão: o desenho do Guerreiro olha para a direita, e ele andava virado para o lado contrário
        [Test]
        public void So_espelha_quando_olha_para_o_lado_contrario_do_desenho()
        {
            Assert.AreEqual(1, RegrasDeAnimacao.EscalaHorizontal(desenhoOlhaParaDireita: true, olhandoParaDireita: true));
            Assert.AreEqual(-1, RegrasDeAnimacao.EscalaHorizontal(desenhoOlhaParaDireita: true, olhandoParaDireita: false));
            Assert.AreEqual(-1, RegrasDeAnimacao.EscalaHorizontal(desenhoOlhaParaDireita: false, olhandoParaDireita: true));
        }

        [Test]
        public void Voo_bate_as_duas_asas_espelhadas()
        {
            float ciclo = RegrasDeAnimacao.CicloDaBatida;
            var cima = RegrasDeAnimacao.Voo(ciclo * 0.25f);
            var baixo = RegrasDeAnimacao.Voo(ciclo * 0.75f);
            Assert.AreEqual(-cima.AsaTras, cima.AsaFrente, 1e-3, "as asas giram em sentidos opostos");
            Assert.Greater(System.Math.Abs(cima.AsaFrente), 20);
            Assert.Less(cima.AsaFrente * baixo.AsaFrente, 0, "meia batida depois inverte");
        }

        [Test]
        public void Corpo_sobe_quando_as_asas_descem()
        {
            float ciclo = RegrasDeAnimacao.CicloDaBatida;
            var a = RegrasDeAnimacao.Voo(ciclo * 0.25f);
            var b = RegrasDeAnimacao.Voo(ciclo * 0.75f);
            // asa da frente com ângulo positivo = ponta para baixo
            var descendo = a.AsaFrente > 0 ? a : b;
            var subindo = a.AsaFrente > 0 ? b : a;
            Assert.Greater(descendo.Elevacao, subindo.Elevacao);
        }

        [Test]
        public void Progresso_fora_da_faixa_e_limitado()
        {
            var depois = RegrasDeAnimacao.Pose(EstadoDoBoneco.Golpeando, 0, 5);
            var fim = RegrasDeAnimacao.Pose(EstadoDoBoneco.Golpeando, 0, 1);
            Assert.AreEqual(fim.BracoFrente, depois.BracoFrente, 1e-3);
        }
    }
}
