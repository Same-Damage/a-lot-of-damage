using System;
using System.Collections.Generic;
using System.Linq;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes.EditMode
{
    public class RegrasDoCenarioTestes
    {
        static readonly ParametrosDaIlha Ilha = new ParametrosDaIlha();

        static VisualDaRegiao Visual(int arvores = 40, int pinheiros = 20, int pedras = 20, int cristais = 6, int ruinas = 4, int arbustos = 30, int grama = 120) =>
            new VisualDaRegiao { Arvores = arvores, Pinheiros = pinheiros, Pedras = pedras, Cristais = cristais, Ruinas = ruinas, Arbustos = arbustos, Grama = grama };

        static float Distancia(ItemDoCenario a, ItemDoCenario b) => (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Z - b.Z) * (a.Z - b.Z));
        static float Raio(ItemDoCenario i) => (float)Math.Sqrt(i.X * i.X + i.Z * i.Z);

        [Test]
        public void Mesma_semente_gera_o_mesmo_cenario()
        {
            var a = RegrasDoCenario.Espalhar(Ilha, Visual(), 7, null);
            var b = RegrasDoCenario.Espalhar(Ilha, Visual(), 7, null);
            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void Sementes_diferentes_geram_cenarios_diferentes()
        {
            var a = RegrasDoCenario.Espalhar(Ilha, Visual(), 1, null);
            var b = RegrasDoCenario.Espalhar(Ilha, Visual(), 2, null);
            CollectionAssert.AreNotEqual(a, b);
        }

        [Test]
        public void Tudo_fica_dentro_da_ilha()
        {
            var itens = RegrasDoCenario.Espalhar(Ilha, Visual(), 3, null);
            Assert.IsTrue(itens.All(i => Raio(i) <= Ilha.RaioDaBorda - 1));
        }

        [Test]
        public void Nada_nasce_na_area_livre_do_inicio()
        {
            var itens = RegrasDoCenario.Espalhar(Ilha, Visual(), 3, null);
            Assert.IsTrue(itens.All(i => Raio(i) >= Ilha.RaioLivreNoInicio));
        }

        [Test]
        public void Areas_livres_ficam_vazias()
        {
            var livres = new List<AreaLivre> { new AreaLivre(20, 0, 4), new AreaLivre(-15, 10, 3) };
            var itens = RegrasDoCenario.Espalhar(Ilha, Visual(grama: 400), 5, livres);
            foreach (var a in livres)
                Assert.IsTrue(itens.All(i => Math.Sqrt((i.X - a.X) * (i.X - a.X) + (i.Z - a.Z) * (i.Z - a.Z)) >= a.Raio));
        }

        [Test]
        public void Obstaculos_respeitam_a_distancia_minima_entre_si()
        {
            var obstaculos = RegrasDoCenario.Espalhar(Ilha, Visual(), 9, null).Where(i => RegrasDoCenario.Bloqueia(i.Tipo)).ToList();
            for (int i = 0; i < obstaculos.Count; i++)
            for (int j = i + 1; j < obstaculos.Count; j++)
                Assert.GreaterOrEqual(Distancia(obstaculos[i], obstaculos[j]), Ilha.DistanciaMinima - 0.001f);
        }

        [Test]
        public void Quantidade_segue_o_visual_da_regiao()
        {
            var itens = RegrasDoCenario.Espalhar(Ilha, Visual(arvores: 10, pinheiros: 0, pedras: 5, cristais: 0, ruinas: 0, arbustos: 0, grama: 0), 4, null);
            Assert.AreEqual(10, itens.Count(i => i.Tipo == TipoDeItemDoCenario.Arvore));
            Assert.AreEqual(5, itens.Count(i => i.Tipo == TipoDeItemDoCenario.Pedra));
            Assert.AreEqual(0, itens.Count(i => i.Tipo == TipoDeItemDoCenario.Cristal));
        }

        [Test]
        public void Parte_das_arvores_emoldura_a_borda_da_ilha()
        {
            var arvores = RegrasDoCenario.Espalhar(Ilha, Visual(arvores: 60), 11, null).Where(i => i.Tipo == TipoDeItemDoCenario.Arvore).ToList();
            Assert.Greater(arvores.Count(i => Raio(i) > Ilha.RaioAndavel), 10);
        }

        [Test]
        public void Escala_e_rotacao_ficam_em_faixas_razoaveis()
        {
            foreach (var i in RegrasDoCenario.Espalhar(Ilha, Visual(), 6, null))
            {
                Assert.That(i.Escala, Is.InRange(0.6f, 1.6f));
                Assert.That(i.Rotacao, Is.InRange(0f, 360f));
            }
        }

        [Test]
        public void Grama_nao_bloqueia_o_caminho()
        {
            Assert.IsFalse(RegrasDoCenario.Bloqueia(TipoDeItemDoCenario.Grama));
            Assert.IsFalse(RegrasDoCenario.Bloqueia(TipoDeItemDoCenario.Arbusto));
            Assert.IsTrue(RegrasDoCenario.Bloqueia(TipoDeItemDoCenario.Arvore));
        }

        [Test]
        public void Area_andavel_e_plana()
        {
            Assert.AreEqual(0f, RegrasDoCenario.Altura(Ilha, 0, 0, 1));
            Assert.AreEqual(0f, RegrasDoCenario.Altura(Ilha, Ilha.RaioAndavel - 0.5f, 0, 1));
        }

        [Test]
        public void Borda_sobe_em_morros_que_fecham_a_ilha()
        {
            float soma = 0;
            for (int a = 0; a < 36; a++)
            {
                double ang = a * Math.PI / 18;
                float r = (Ilha.RaioAndavel + Ilha.RaioDaBorda) * 0.5f;
                soma += RegrasDoCenario.Altura(Ilha, (float)(Math.Cos(ang) * r), (float)(Math.Sin(ang) * r), 1);
            }
            Assert.Greater(soma / 36, 1f);
        }

        [Test]
        public void Item_fica_apoiado_na_altura_do_chao()
        {
            foreach (var i in RegrasDoCenario.Espalhar(Ilha, Visual(), 8, null))
                Assert.AreEqual(RegrasDoCenario.Altura(Ilha, i.X, i.Z, 8), i.Y, 0.0001f);
        }

        [Test]
        public void Depois_dos_morros_a_praia_desce_ate_o_mar()
        {
            for (int a = 0; a < 12; a++)
            {
                double ang = a * Math.PI / 6;
                float Em(float r) => RegrasDoCenario.Altura(Ilha, (float)(Math.Cos(ang) * r), (float)(Math.Sin(ang) * r), 3);
                Assert.Greater(Em(Ilha.RaioDaBorda), Ilha.NivelDoMar);
                Assert.Less(Em(Ilha.RaioDaPraia), Ilha.NivelDoMar);
                Assert.Greater(Em(Ilha.RaioDaBorda), Em((Ilha.RaioDaBorda + Ilha.RaioDaPraia) * 0.5f));
            }
        }

        [Test]
        public void A_area_andavel_fica_acima_do_mar()
        {
            Assert.Greater(RegrasDoCenario.Altura(Ilha, 0, 0, 1), Ilha.NivelDoMar);
            Assert.Less(Ilha.RaioDaBorda, Ilha.RaioDaPraia);
        }
    }
}
