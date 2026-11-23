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

        // Regressão: o ciclo usava o relógio global e o passo começava numa fase qualquer, com tranco
        [Test]
        public void Andar_comeca_com_as_pernas_juntas()
        {
            var inicio = RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, 0, 0);
            Assert.AreEqual(0, inicio.PernaFrente, 0.5f);
            Assert.AreEqual(0, inicio.PernaTras, 0.5f);
        }

        // Num andar natural o corpo fica mais baixo quando o pé toca o chão (pernas abertas)
        // e mais alto quando as pernas se cruzam; o contrário faz o boneco "pular"
        [Test]
        public void Corpo_desce_no_toque_do_pe_e_sobe_no_cruzamento()
        {
            float ciclo = RegrasDeAnimacao.CicloDoPasso;
            var cruzando = RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, 0, 0);
            var tocando = RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, ciclo * 0.25f, 0);
            Assert.Greater(cruzando.Elevacao, tocando.Elevacao);
        }

        [Test]
        public void Andando_o_tronco_inclina_para_a_frente()
        {
            float soma = 0;
            for (int i = 0; i < 20; i++) soma += RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, RegrasDeAnimacao.CicloDoPasso * i / 20f, 0).Tronco;
            // o desenho olha para a direita: inclinar para a frente é girar no sentido horário (negativo)
            Assert.Less(soma / 20, -2);
        }

        [Test]
        public void Braco_balanca_ao_contrario_da_perna_do_mesmo_lado()
        {
            var p = RegrasDeAnimacao.Pose(EstadoDoBoneco.Andando, RegrasDeAnimacao.CicloDoPasso * 0.25f, 0);
            Assert.Less(p.PernaFrente * p.BracoFrente, 0);
        }

        [Test]
        public void Mistura_vai_de_uma_pose_a_outra_sem_salto()
        {
            var a = new Pose { PernaFrente = 20, Tronco = -4, Elevacao = 0.1f };
            var b = new Pose { PernaFrente = 0, Tronco = 0, Elevacao = 0 };
            Assert.AreEqual(20, RegrasDeAnimacao.Misturar(a, b, 0).PernaFrente, 1e-4);
            Assert.AreEqual(10, RegrasDeAnimacao.Misturar(a, b, 0.5f).PernaFrente, 1e-4);
            Assert.AreEqual(0, RegrasDeAnimacao.Misturar(a, b, 1).PernaFrente, 1e-4);
            Assert.AreEqual(0, RegrasDeAnimacao.Misturar(a, b, 3).Elevacao, 1e-4);
        }

        // ---------- monstros ----------

        [Test]
        public void Gosma_amassa_sem_perder_volume()
        {
            for (float t = 0; t < 2; t += 0.05f)
            foreach (bool andando in new[] { false, true })
            {
                var p = RegrasDeAnimacao.Monstro(FormaDoMonstro.Gosma, t, andando);
                Assert.AreEqual(1, p.Largura * p.Largura * p.Altura, 0.03f);
            }
        }

        [Test]
        public void Gosma_pula_ao_andar_e_fica_no_chao_parada()
        {
            float maxAndando = 0, maxParada = 0;
            for (float t = 0; t < 2; t += 0.02f)
            {
                maxAndando = System.Math.Max(maxAndando, RegrasDeAnimacao.Monstro(FormaDoMonstro.Gosma, t, true).Elevacao);
                maxParada = System.Math.Max(maxParada, RegrasDeAnimacao.Monstro(FormaDoMonstro.Gosma, t, false).Elevacao);
            }
            Assert.Greater(maxAndando, 0.25f);
            Assert.AreEqual(0, maxParada, 1e-4);
        }

        [Test]
        public void Gosma_achata_quando_toca_o_chao_e_estica_no_ar()
        {
            var noChao = RegrasDeAnimacao.Monstro(FormaDoMonstro.Gosma, 0, true);
            var noAlto = RegrasDeAnimacao.Monstro(FormaDoMonstro.Gosma, RegrasDeAnimacao.CicloDoPulo * 0.5f, true);
            Assert.Less(noChao.Altura, 1);
            Assert.Greater(noAlto.Altura, 1);
            Assert.Greater(noAlto.Elevacao, noChao.Elevacao);
        }

        [Test]
        public void Quadrupede_trota_com_as_diagonais_opostas()
        {
            var p = RegrasDeAnimacao.Monstro(FormaDoMonstro.Quadrupede, RegrasDeAnimacao.CicloDoTrote * 0.25f, true);
            Assert.Greater(System.Math.Abs(p.PernasA), 15);
            Assert.AreEqual(-p.PernasA, p.PernasB, 1e-4);
        }

        [Test]
        public void Golem_balanca_os_bracos_ao_contrario_das_pernas()
        {
            var p = RegrasDeAnimacao.Monstro(FormaDoMonstro.Golem, RegrasDeAnimacao.CicloDoGolem * 0.25f, true);
            Assert.Less(p.PernasA * p.Bracos, 0);
            Assert.Greater(System.Math.Abs(p.Inclinacao), 1);
        }

        [Test]
        public void Monstro_parado_nao_mexe_as_pernas()
        {
            foreach (var forma in new[] { FormaDoMonstro.Quadrupede, FormaDoMonstro.Golem })
            for (float t = 0; t < 2; t += 0.1f)
                Assert.Less(System.Math.Abs(RegrasDeAnimacao.Monstro(forma, t, false).PernasA), 1);
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
