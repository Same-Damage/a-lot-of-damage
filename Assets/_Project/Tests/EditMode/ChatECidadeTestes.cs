using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class ChatECidadeTestes
    {
        class Falso : ITransporteDeChat, IBloqueados, IServicoDeDenuncias, IRelogio
        {
            public event Action<string, string, string, string> Recebida;
            public readonly List<string> Feito = new List<string>();
            public readonly HashSet<string> Bloqueados = new HashSet<string>();
            public DateTime AgoraUtc { get; set; } = new DateTime(2026, 1, 1);
            public bool SemRede;
            Task Fazer(string s) { if (SemRede) return Task.FromException(new FalhaOnline("rede")); Feito.Add(s); return Task.CompletedTask; }
            public Task EntrarNoCanal(string c, string n) => Fazer("entrou " + c);
            public Task SairDoCanal(string c) => Fazer("saiu " + c);
            public Task Enviar(string c, string t) => Fazer($"{c}: {t}");
            public bool Esta(string id) => Bloqueados.Contains(id);
            public void Bloquear(string id) => Bloqueados.Add(id);
            public Task Denunciar(Denuncia d) => Fazer("denunciou " + d.JogadorId);
            public void Chega(string canal, string id, string texto, string nome = "Bia") => Recebida(canal, id, nome, texto);
        }

        Falso f;
        ConversaDoJogador c;

        [SetUp]
        public void Criar()
        {
            f = new Falso();
            c = new ConversaDoJogador(f, f, f, new OnlineDesligado(), f);
        }

        static void Fim(Task t) => t.GetAwaiter().GetResult();

        [Test]
        public void Mensagem_aparece_so_do_canal_em_que_estou()
        {
            Fim(c.Entrar(CanalDeChat.Cidade, "s1"));
            f.Chega("cidade-s1", "b", "oi");
            f.Chega("cidade-s2", "b", "outra sala");
            Assert.AreEqual(1, c.Historico.Count);
            Assert.AreEqual("oi", c.Historico[0].Texto);
        }

        [Test]
        public void Trocar_de_sala_limpa_a_conversa_dela()
        {
            Fim(c.Entrar(CanalDeChat.Cidade, "s1"));
            Fim(c.Entrar(CanalDeChat.Guilda, "g"));
            f.Chega("cidade-s1", "b", "oi");
            f.Chega("guilda-g", "b", "oi guilda");
            Fim(c.Entrar(CanalDeChat.Cidade, ""));
            Assert.AreEqual(1, c.Historico.Count);
            Assert.IsFalse(c.Conectado(CanalDeChat.Cidade));
            CollectionAssert.Contains(f.Feito, "saiu cidade-s1");
        }

        [Test]
        public void Falha_ao_entrar_nao_deixa_o_canal_marcado()
        {
            f.SemRede = true;
            Assert.Throws<FalhaOnline>(() => Fim(c.Entrar(CanalDeChat.Cidade, "s1")));
            Assert.IsFalse(c.Conectado(CanalDeChat.Cidade));
        }

        [Test]
        public void Limite_de_mensagens()
        {
            Fim(c.Entrar(CanalDeChat.Cidade, "s"));
            for (int i = 0; i < RegrasDoChat.MensagensPorJanela; i++) Fim(c.Enviar(CanalDeChat.Cidade, "oi"));
            Assert.Throws<FalhaOnline>(() => Fim(c.Enviar(CanalDeChat.Cidade, "oi")));
            f.AgoraUtc = f.AgoraUtc.AddSeconds(RegrasDoChat.SegundosDaJanela + 1);
            Assert.DoesNotThrow(() => Fim(c.Enviar(CanalDeChat.Cidade, "oi")));
        }

        [Test]
        public void Bloquear_some_com_as_mensagens_e_denunciar_bloqueia()
        {
            Fim(c.Entrar(CanalDeChat.Cidade, "s"));
            f.Chega("cidade-s", "b", "oi");
            f.Chega("cidade-s", "d", "ei");
            c.Bloquear("b");
            f.Chega("cidade-s", "b", "de novo");
            Assert.AreEqual(1, c.Historico.Count);
            Fim(c.Denunciar(c.Historico[0]));
            CollectionAssert.Contains(f.Feito, "denunciou d");
            Assert.AreEqual(0, c.Historico.Count);
        }

        [Test]
        public void Denuncia_que_falha_nao_bloqueia()
        {
            Fim(c.Entrar(CanalDeChat.Cidade, "s"));
            f.Chega("cidade-s", "b", "oi");
            f.SemRede = true;
            Assert.Throws<FalhaOnline>(() => Fim(c.Denunciar(c.Historico[0])));
            Assert.AreEqual(1, c.Historico.Count);
        }

        [Test]
        public void Texto_e_nome_dos_outros_sao_limpos()
        {
            Fim(c.Entrar(CanalDeChat.Cidade, "s"));
            f.Chega("cidade-s", "b", "linha\nnova " + new string('x', 500), "<color=red>Bia");
            Assert.IsFalse(c.Historico[0].Texto.Contains("\n"));
            Assert.LessOrEqual(c.Historico[0].Texto.Length, RegrasDoChat.TamanhoMaximo);
            Assert.AreEqual("color=redBia", c.Historico[0].Nome);
        }

        [TestCase("que m3rd4!", "que ******")]
        [TestCase("computador e deputado", "computador e deputado")]
        [TestCase("oi", "oi")]
        public void Filtro(string texto, string esperado) => Assert.AreEqual(esperado, RegrasDoChat.Filtrar(texto));

        [Test]
        public void Ponto_da_cidade_e_conferido()
        {
            float x = float.NaN, z = 0;
            Assert.IsFalse(RegrasDaCidade.Ponto(ref x, ref z));
            x = 1e9f; z = 0;
            Assert.IsTrue(RegrasDaCidade.Ponto(ref x, ref z));
            Assert.AreEqual(RegrasDaCidade.Raio, x, 0.01f);
            Assert.IsFalse(RegrasDaCidade.DeveEnviar(0.01f, 10));
            Assert.IsFalse(RegrasDaCidade.DeveEnviar(5, 0.01f));
            Assert.IsTrue(RegrasDaCidade.DeveEnviar(1, 0.3f));
        }

        class Arena : IServicoDaArena
        {
            public VistaDaArena V = new VistaDaArena { CristaisTotais = 15 };
            public Task<VistaDaArena> Pedir(PedidoDaArena p) => Task.FromResult(V);
        }

        [Test]
        public void Cristais_da_arena_entram_uma_vez_e_ficam_no_save()
        {
            var r = new RepositorioDeProgressoEmMemoria();
            var a = new ArenaDoJogador(r, new Arena());
            Assert.AreEqual(15, a.Pedir(AcaoDaArena.Ver).GetAwaiter().GetResult());
            Assert.AreEqual(0, a.Pedir(AcaoDaArena.Ver).GetAwaiter().GetResult());
            Assert.AreEqual(0, Progresso.Importar(r.Carregar().Exportar()).ReceberDaArena(15));
            Assert.AreEqual(15, r.Carregar().Cristais);
        }
    }
}
