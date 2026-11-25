using System;
using System.Diagnostics;
using System.IO;
using ALotOfDamage.Core;
using ALotOfDamage.Nuvem;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    // O Core rodando fora da Unity: mesmo JSON, mesmo Poder, mesma decisão
    public class RecepcaoTestes
    {
        static readonly DateTime Agora = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        class Prova { public string Progresso = null; public double Poder = 0; }

        static Prova Ler() => Recepcao.Ler<Prova>(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "prova.json")));

        static PedidoDeEnvio Pedido(int revisao = 0) => new PedidoDeEnvio { Progresso = Ler().Progresso, Versao = 1, RevisaoBase = revisao, Dispositivo = "pc" };

        [Test]
        public void O_servidor_calcula_o_mesmo_poder_que_o_jogo()
        {
            var p = Ler();
            Assert.That(p.Poder, Is.GreaterThan(1000));
            Assert.That(Recepcao.Receber(null, Pedido(), Agora).Nuvem.Poder, Is.EqualTo(p.Poder).Within(1e-9).Percent);
        }

        [Test]
        public void Salva_o_json_que_veio_da_unity()
        {
            var r = Recepcao.Receber(null, Pedido(), Agora);
            Assert.That(r.Resultado, Is.EqualTo(ResultadoDoEnvio.Salvo));
            Assert.That(r.Nuvem.Poder, Is.EqualTo(Ler().Poder).Within(1e-9).Percent);
            var ida = Recepcao.Ler<SaveNaNuvem>(Recepcao.Escrever(r.Nuvem));
            Assert.That(ida.Progresso, Is.EqualTo(Ler().Progresso));
            Assert.That(Recepcao.Receber(ida, Pedido(1), Agora.AddMinutes(5)).Nuvem.Revisao, Is.EqualTo(2));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{ quebrado")]
        [TestCase("null")]
        public void Progresso_que_nao_se_le_e_recusado(string texto) =>
            Assert.That(Recepcao.Receber(null, new PedidoDeEnvio { Progresso = texto, Versao = 1 }, Agora).Resultado, Is.EqualTo(ResultadoDoEnvio.Recusado));

        [Test]
        public void Pedido_grande_demais_e_recusado() =>
            Assert.That(Recepcao.Receber(null, new PedidoDeEnvio { Progresso = new string(' ', Recepcao.TamanhoMaximo + 1), Versao = 1 }, Agora).Resultado, Is.EqualTo(ResultadoDoEnvio.Recusado));

        [Test]
        public void Listas_nulas_no_json_nao_derrubam_o_servidor() =>
            Assert.That(Recepcao.Receber(null, new PedidoDeEnvio { Versao = 1, Progresso = "{\"Niveis\":null,\"Inventario\":[null],\"Equipados\":null,\"Followers\":null,\"Reliquias\":[null]}" }, Agora).Resultado, Is.EqualTo(ResultadoDoEnvio.Recusado));

        [Test]
        public void Cada_chamada_e_rapida()
        {
            var atual = Recepcao.Receber(null, Pedido(), Agora).Nuvem;
            var t = Stopwatch.StartNew();
            for (int i = 0; i < 200; i++) Recepcao.Receber(atual, Pedido(1), Agora.AddMinutes(5));
            TestContext.WriteLine($"{t.Elapsed.TotalMilliseconds / 200:0.00} ms por chamada, sem rede");
            Assert.That(t.Elapsed.TotalMilliseconds / 200, Is.LessThan(50));
        }
    }
}
