using System;
using ALotOfDamage.Core;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class RegrasDaNuvemTestes
    {
        static readonly DateTime Agora = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        readonly ParametrosDaNuvem p = new ParametrosDaNuvem { SegundosPorVitoria = 1, FolgaDeVitorias = 10, FolgaDoOuro = 1, HorasDeOuroAcumulado = 0 };
        readonly ParametrosDaCampanha c = new ParametrosDaCampanha();

        static DadosDoProgresso Dados(int vitorias = 100, int estagio = 5, double ouro = 50) =>
            new DadosDoProgresso { Vitorias = vitorias, Estagio = estagio, EstagioMaximo = estagio, Ouro = ouro, OuroTotalGanho = ouro };

        [TestCase(0, 0, true, false, AcaoDaSincronia.Enviar)]      // nada na nuvem
        [TestCase(3, 3, true, false, AcaoDaSincronia.Enviar)]
        [TestCase(3, 3, false, false, AcaoDaSincronia.Nada)]
        [TestCase(3, 4, false, false, AcaoDaSincronia.Baixar)]
        [TestCase(3, 4, true, false, AcaoDaSincronia.Conflito)]
        [TestCase(0, 4, true, true, AcaoDaSincronia.Baixar)]       // instalação nova
        [TestCase(5, 4, false, false, AcaoDaSincronia.Conflito)]   // a nuvem voltou no tempo
        public void Decide_quem_vale(int local, int nuvem, bool mudou, bool vazio, AcaoDaSincronia esperado) =>
            Assert.AreEqual(esperado, RegrasDaNuvem.Decidir(local, nuvem, mudou, vazio));

        [Test]
        public void Presenca_nao_conta_como_mudanca()
        {
            string Texto(DadosDoProgresso d) => $"{d.Ouro}|{d.UltimaPresencaUtc}";
            var d1 = Dados(); d1.UltimaPresencaUtc = 10;
            var d2 = Dados(); d2.UltimaPresencaUtc = 99;
            Assert.AreEqual(RegrasDaNuvem.Impressao(d1, Texto), RegrasDaNuvem.Impressao(d2, Texto));
            Assert.AreEqual(10, d1.UltimaPresencaUtc);
            d2.Ouro++;
            Assert.AreNotEqual(RegrasDaNuvem.Impressao(d1, Texto), RegrasDaNuvem.Impressao(d2, Texto));
        }

        [Test]
        public void Progresso_novo_e_valido_e_vazio()
        {
            var d = new Progresso().Exportar();
            Assert.IsNull(RegrasDaNuvem.ProblemaDosDados(d));
            Assert.IsTrue(RegrasDaNuvem.Vazio(d));
            Assert.IsFalse(RegrasDaNuvem.Vazio(Dados()));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1)]
        public void Ouro_impossivel_e_recusado(double ouro) => Assert.IsNotNull(RegrasDaNuvem.ProblemaDosDados(Dados(ouro: ouro)));

        [Test]
        public void Ouro_maior_que_o_total_ganho_e_recusado()
        {
            var d = Dados(); d.OuroTotalGanho = 1; d.Ouro = 500;
            Assert.IsNotNull(RegrasDaNuvem.ProblemaDosDados(d));
        }

        [Test]
        public void Avanco_dentro_do_tempo_passa_e_fora_dele_nao()
        {
            Assert.IsNull(RegrasDaNuvem.ProblemaDoAvanco(p, c, 100, 5, 50, Dados(160, 8, 60), 60));
            Assert.IsNotNull(RegrasDaNuvem.ProblemaDoAvanco(p, c, 100, 5, 50, Dados(1000, 8, 60), 60));
            Assert.IsNotNull(RegrasDaNuvem.ProblemaDoAvanco(p, c, 100, 5, 50, Dados(102, 50, 60), 60));
            Assert.IsNotNull(RegrasDaNuvem.ProblemaDoAvanco(p, c, 100, 5, 50, Dados(160, 8, 1e30), 60));
        }

        RespostaDoEnvio Receber(SaveNaNuvem atual, DadosDoProgresso anterior, PedidoDeEnvio pedido, DadosDoProgresso novo) =>
            RegrasDaNuvem.Receber(p, c, atual, anterior, pedido, novo, 7, Agora);

        [Test]
        public void Primeiro_envio_cria_a_revisao_1_e_guarda_o_ponto_de_partida()
        {
            var r = Receber(null, null, new PedidoDeEnvio { Versao = 1, Progresso = "x", Dispositivo = "pc" }, Dados());
            Assert.AreEqual(ResultadoDoEnvio.Salvo, r.Resultado);
            Assert.AreEqual(1, r.Nuvem.Revisao);
            Assert.AreEqual(100, r.Nuvem.VitoriasIniciais);
            Assert.AreEqual(Agora.Ticks, r.Nuvem.CriadoEmUtc);
            Assert.AreEqual(7, r.Nuvem.Poder);
        }

        static SaveNaNuvem Guardado(int segundosAtras) => new SaveNaNuvem
        {
            Revisao = 3, Versao = 1, SalvoEmUtc = Agora.AddSeconds(-segundosAtras).Ticks, CriadoEmUtc = Agora.AddHours(-10).Ticks,
            VitoriasIniciais = 0, EstagioInicial = 1, OuroInicial = 0,
        };

        [Test]
        public void Envio_por_cima_sobe_a_revisao_e_mantem_o_ponto_de_partida()
        {
            var r = Receber(Guardado(60), Dados(), new PedidoDeEnvio { Versao = 1, RevisaoBase = 3 }, Dados(150, 6));
            Assert.AreEqual(ResultadoDoEnvio.Salvo, r.Resultado);
            Assert.AreEqual(4, r.Nuvem.Revisao);
            Assert.AreEqual(0, r.Nuvem.VitoriasIniciais);
        }

        [Test]
        public void Revisao_desconhecida_e_conflito() =>
            Assert.AreEqual(ResultadoDoEnvio.Conflito, Receber(Guardado(60), Dados(), new PedidoDeEnvio { Versao = 1, RevisaoBase = 2 }, Dados()).Resultado);

        [Test]
        public void Jogo_antigo_nao_sobrescreve_save_novo()
        {
            var g = Guardado(60); g.Versao = 2;
            Assert.AreEqual(ResultadoDoEnvio.JogoAntigo, Receber(g, Dados(), new PedidoDeEnvio { Versao = 1, RevisaoBase = 3 }, Dados()).Resultado);
        }

        [Test]
        public void Conflito_resolvido_compara_so_com_o_ponto_de_partida()
        {
            var pedido = new PedidoDeEnvio { Versao = 1, RevisaoBase = 3 };
            Assert.AreEqual(ResultadoDoEnvio.Recusado, Receber(Guardado(1), Dados(), pedido, Dados(5000, 6)).Resultado);
            pedido.Forcar = true;
            Assert.AreEqual(ResultadoDoEnvio.Salvo, Receber(Guardado(1), Dados(), pedido, Dados(5000, 6)).Resultado);
            Assert.AreEqual(ResultadoDoEnvio.Recusado, Receber(Guardado(1), Dados(), pedido, Dados(500000, 6)).Resultado);
        }

        [TestCase("heroi_1", true)]
        [TestCase("ab", false)]
        [TestCase("com espaço", false)]
        public void Usuario(string u, bool vale) => Assert.AreEqual(vale, RegrasDaConta.ProblemaDoUsuario(u) == null);

        [TestCase("Exemplo#123", true)]
        [TestCase("exemplo#123", false)]
        [TestCase("Ex#1", false)]
        [TestCase("Exemplo1234", false)]
        public void Senha(string s, bool vale) => Assert.AreEqual(vale, RegrasDaConta.ProblemaDaSenha(s) == null);

        [TestCase("Heroi", true)]
        [TestCase("a b c", false)]
        [TestCase("", false)]
        public void Apelido(string a, bool vale) => Assert.AreEqual(vale, RegrasDaConta.ProblemaDoApelido(a) == null);
    }
}
