using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using ALotOfDamage.Nuvem;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class ArenaTestes
    {
        class Armazem : IArmazem
        {
            public readonly Dictionary<string, string> D;
            readonly string eu;
            public Armazem(Dictionary<string, string> d, string eu) { D = d; this.eu = eu; }
            string K(string dono, string chave) => (dono == "" ? Armazens.DoJogador(eu) : dono) + "/" + chave;
            public Task<(string texto, string trava)> Ler(string dono, string chave) => Task.FromResult(D.TryGetValue(K(dono, chave), out var t) ? (t, "1") : ((string)null, (string)null));
            public Task<Dictionary<string, string>> LerVarios(string dono, List<string> chaves) => throw new NotSupportedException();
            public Task<bool> Gravar(string dono, string chave, string texto, string trava) { D[K(dono, chave)] = texto; return Task.FromResult(true); }
            public Task Apagar(string dono, string chave) { D.Remove(K(dono, chave)); return Task.CompletedTask; }
        }

        class Posicoes : IPosicoesDaArena
        {
            public readonly Dictionary<string, double> P;
            readonly string eu;
            public Posicoes(Dictionary<string, double> p, string eu) { P = p; this.eu = eu; }
            public Task Pontuar(double pontos) { P[eu] = pontos; return Task.CompletedTask; }
            public Task<List<LinhaDoRanking>> Perto(int n)
            {
                var l = new List<LinhaDoRanking>();
                foreach (var x in P) l.Add(new LinhaDoRanking { JogadorId = x.Key, Nome = "<b>" + x.Key, Pontos = x.Value });
                return Task.FromResult(l);
            }
        }

        class Sorteio : ISorteio { public double V; public double Proximo() => V; }

        readonly ParametrosDaArena p = new ParametrosDaArena();
        Dictionary<string, string> dados;
        Dictionary<string, double> pontos;
        Sorteio sorteio;
        DateTime agora;

        [SetUp]
        public void Criar()
        {
            dados = new Dictionary<string, string>();
            pontos = new Dictionary<string, double>();
            sorteio = new Sorteio();
            agora = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);
            foreach (var j in new[] { "ana", "bia", "caio", "duda", "edu" }) Save(j, 100);
            foreach (var j in new[] { "bia", "caio", "duda", "edu" }) Pedir(j, AcaoDaArena.Ver);
        }

        void Save(string j, double poder) => dados[Armazens.DoJogador(j) + "/" + ModuloDaNuvem.Chave] =
            Recepcao.Escrever(new SaveNaNuvem { Revisao = 1, Poder = poder, Progresso = Recepcao.Escrever(new DadosDoProgresso { HeroiId = "Mago" }) });

        VistaDaArena Pedir(string eu, AcaoDaArena acao, string texto = "") =>
            new ServicoDaArena(new Armazem(dados, eu), new Posicoes(pontos, eu), p, sorteio, eu, agora).Executar(new PedidoDaArena { Acao = acao, Texto = texto }).GetAwaiter().GetResult();

        [Test]
        public void Oferece_outros_jogadores_com_save()
        {
            dados.Remove(Armazens.DoJogador("bia") + "/" + ModuloDaNuvem.Chave);
            var v = Pedir("ana", AcaoDaArena.Ver);
            Assert.That(v.Adversarios.Count, Is.EqualTo(3));
            Assert.That(v.Adversarios.TrueForAll(a => a.JogadorId != "ana" && a.JogadorId != "bia" && a.HeroiId == "Mago" && !a.Nome.Contains("<")));
            Assert.That(Pedir("ana", AcaoDaArena.Ver).Adversarios[0].JogadorId, Is.EqualTo(v.Adversarios[0].JogadorId), "as ofertas não mudam sozinhas");
        }

        [Test]
        public void Sem_save_na_nuvem_nao_entra() { dados.Clear(); Assert.That(Pedir("ana", AcaoDaArena.Ver).Erro, Is.Not.Empty); }

        [Test]
        public void Vitoria_da_pontos_e_cristais_e_derrota_nao_tira()
        {
            var v = Pedir("ana", AcaoDaArena.Lutar, Pedir("ana", AcaoDaArena.Ver).Adversarios[0].JogadorId);
            Assert.That(v.Resultado.Venceu && v.Resultado.Rodadas.Count == 3);
            Assert.That(v.Pontos, Is.EqualTo(1020));
            Assert.That(v.CristaisTotais, Is.EqualTo(p.CristaisPorVitoria));
            sorteio.V = 0.999;
            v = Pedir("ana", AcaoDaArena.Lutar, v.Adversarios[0].JogadorId);
            Assert.That(v.Resultado.Venceu, Is.False);
            Assert.That(v.Pontos, Is.EqualTo(1020));
        }

        [Test]
        public void So_luta_com_quem_foi_oferecido_e_ate_o_limite_do_dia()
        {
            Assert.That(Pedir("ana", AcaoDaArena.Lutar, "bia").Erro, Is.Not.Empty);
            for (int i = 0; i < p.LutasPorDia; i++)
                Assert.That(Pedir("ana", AcaoDaArena.Lutar, Pedir("ana", AcaoDaArena.Ver).Adversarios[0].JogadorId).Erro, Is.Empty);
            string alvo = Pedir("ana", AcaoDaArena.Ver).Adversarios[0].JogadorId;
            Assert.That(Pedir("ana", AcaoDaArena.Lutar, alvo).Erro, Is.Not.Empty);
            agora = agora.AddDays(1);
            Assert.That(Pedir("ana", AcaoDaArena.Lutar, alvo).Erro, Is.Empty);
        }

        [Test]
        public void Temporada_nova_entrega_a_liga_uma_vez_e_zera_os_pontos()
        {
            Pedir("ana", AcaoDaArena.Lutar, Pedir("ana", AcaoDaArena.Ver).Adversarios[0].JogadorId);
            agora = new DateTime(2026, 12, 1, 0, 0, 1, DateTimeKind.Utc);
            var v = Pedir("ana", AcaoDaArena.Ver);
            Assert.That(v.RecompensaDaTemporada, Is.EqualTo(50));
            Assert.That(v.Pontos, Is.EqualTo(p.PontosIniciais));
            Assert.That(Pedir("ana", AcaoDaArena.Ver).RecompensaDaTemporada, Is.Zero);
            Assert.That(Pedir("bia", AcaoDaArena.Ver).RecompensaDaTemporada, Is.Zero, "quem não lutou não ganha");
        }

        [Test]
        public void Chance_favorece_o_mais_forte_sem_ser_certa()
        {
            Assert.That(RegrasDaArena.Chance(p, 100, 100), Is.EqualTo(0.5));
            Assert.That(RegrasDaArena.Chance(p, 1000, 100), Is.InRange(0.7, 0.8));
            Assert.That(RegrasDaArena.Chance(p, 1e300, 1), Is.EqualTo(0.95));
            Assert.That(RegrasDaArena.Chance(p, 0, double.PositiveInfinity), Is.EqualTo(0.5));
        }

        [Test]
        public void Denuncia_tem_limite_por_dia()
        {
            Task<bool> D() => new ServicoDeDenuncias(new Armazem(dados, "ana"), "ana", agora).Registrar(new Denuncia { JogadorId = "bia", Texto = new string('x', 9999) });
            for (int i = 0; i < 10; i++) Assert.That(D().Result, Is.True);
            Assert.That(D().Result, Is.False);
            Assert.That(new ServicoDeDenuncias(new Armazem(dados, "bia"), "bia", agora).Registrar(new Denuncia { JogadorId = "bia" }).Result, Is.False);
        }
    }
}
