using System;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaGuildaTestes
    {
        readonly ParametrosDaGuilda p = new ParametrosDaGuilda();

        [Test]
        public void Semana_vira_na_segunda()
        {
            var domingo = new DateTime(2026, 11, 22, 23, 59, 59, DateTimeKind.Utc);
            Assert.AreEqual(RegrasDaGuilda.Semana(domingo) + 1, RegrasDaGuilda.Semana(domingo.AddSeconds(1)));
            Assert.AreEqual(1, RegrasDaGuilda.SegundosParaVirar(domingo), 1e-6);
        }

        [Test]
        public void Pontos_cortam_o_que_os_cliques_nao_permitem()
        {
            Assert.AreEqual(900, RegrasDaGuilda.Pontos(p, new ContagemDaLuta { Cliques = 100000 }, 60));
            // 10 cliques: no máximo 10 pontos fracos, combo 10 e 1 ultimate
            Assert.AreEqual(10 + 40 + 15 + 2, RegrasDaGuilda.Pontos(p, new ContagemDaLuta { Cliques = 10, PontosFracos = 99, Ultimates = 99, MaiorCombo = 99 }, 60));
            Assert.AreEqual(0, RegrasDaGuilda.Pontos(p, new ContagemDaLuta { Cliques = -5, PontosFracos = -5, MaiorCombo = -9 }, 60));
        }

        [Test]
        public void Uma_luta_normal_nunca_e_cortada()
        {
            // o combate de verdade: 15 cliques por segundo, com ponto fraco e ultimate carregando como no CombateClicker
            var c = new ContagemDaLuta();
            float carga = 0;
            for (int i = 0; i < 900; i++)
            {
                bool fraco = i % 4 == 0;
                c.Cliques++;
                if (fraco) c.PontosFracos++;
                carga = Math.Min(100, carga + 2 + (fraco ? 10 : 0));
                if (carga >= 100) { carga = 0; c.Ultimates++; }
            }
            c.MaiorCombo = 900;
            Assert.AreEqual(Math.Floor(900 + c.PontosFracos * 4 + c.Ultimates * 15 + 180.0), RegrasDaGuilda.Pontos(p, c, 60));
        }

        [Test]
        public void Chefe_passa_de_fase_e_cai()
        {
            Assert.AreEqual(0, RegrasDaGuilda.Chefe(p, 2999).FasesDerrubadas);
            var s = RegrasDaGuilda.Chefe(p, 3100);
            Assert.AreEqual(1, s.FasesDerrubadas);
            Assert.AreEqual(100, s.PontosNaFase);
            Assert.IsTrue(RegrasDaGuilda.Chefe(p, 1e9).Derrotado);
            Assert.AreEqual(p.Fases, RegrasDaGuilda.Chefe(p, 1e9).FasesDerrubadas);
        }

        [Test]
        public void Luta_conta_ate_o_tempo_acabar()
        {
            var l = new LutaDaGuilda(new DesafioDaGuilda { Fase = 2, VidaDaFase = 100, PontosNaFase = 98 }, p);
            l.Clique(1, false);
            Assert.AreEqual(1, l.VidaRestante);
            StringAssert.Contains("60 s", l.Titulo);
            l.Avancar(60);
            Assert.IsTrue(l.Acabou);
            l.Clique(2, true);
            Assert.AreEqual(1, l.Contagem.Cliques);
        }

        [TestCase("ABCDEF", true)]
        [TestCase("abcdef", false)]
        [TestCase("ABCDE0", false)]
        [TestCase("ABC", false)]
        [TestCase(null, false)]
        public void Codigo(string c, bool vale) => Assert.AreEqual(vale, RegrasDaGuilda.CodigoValido(c));

        class Servico : IServicoDeGuilda
        {
            public VistaDaGuilda Vista = new VistaDaGuilda();
            public int Falhas, Pedidos;
            public Task<VistaDaGuilda> Pedir(PedidoDeGuilda pedido)
            {
                Pedidos++;
                return Falhas-- > 0 ? Task.FromException<VistaDaGuilda>(new FalhaOnline("rede")) : Task.FromResult(Vista);
            }
        }

        [Test]
        public void Recompensa_repetida_nao_duplica_e_sobrevive_ao_save()
        {
            var r = new RepositorioDeProgressoEmMemoria();
            var s = new Servico { Vista = new VistaDaGuilda { CristaisTotais = 60, FragmentosTotais = 4 } };
            var g = new GuildaDoJogador(r, s);
            Assert.AreEqual(60, g.Pedir(AcaoDaGuilda.Coletar).GetAwaiter().GetResult().Cristais);
            Assert.AreEqual(0, g.Pedir(AcaoDaGuilda.Ver).GetAwaiter().GetResult().Cristais);
            Assert.AreEqual(60, r.Carregar().Cristais);
            Assert.AreEqual(4, r.Carregar().Fragmentos);

            var p2 = Progresso.Importar(r.Carregar().Exportar());
            s.Vista.CristaisTotais = 90;
            Assert.AreEqual(30, p2.ReceberDaGuilda(90, 4).Cristais);
            Assert.AreEqual(90, p2.Cristais);
            Assert.AreEqual(0, p2.ReceberDaGuilda(10, 0).Cristais, "outra conta, com total menor, não tira nem dá");
        }

        [Test]
        public void Erro_do_servidor_vira_falha_e_nao_troca_a_vista()
        {
            var g = new GuildaDoJogador(new RepositorioDeProgressoEmMemoria(), new Servico { Vista = new VistaDaGuilda { Erro = "cheia" } });
            Assert.Throws<FalhaOnline>(() => g.Pedir(AcaoDaGuilda.Entrar, "x").GetAwaiter().GetResult());
            Assert.IsNull(g.Vista);
        }
    }
}
