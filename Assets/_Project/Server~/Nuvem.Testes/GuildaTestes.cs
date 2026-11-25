using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using ALotOfDamage.Nuvem;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class GuildaTestes
    {
        // Guarda tudo em memória, com trava como o Cloud Save
        class Memoria
        {
            public readonly Dictionary<string, (string texto, int versao)> Dados = new Dictionary<string, (string, int)>();
            public Action AntesDeGravar;
            int versoes;

            public IArmazem De(string jogador) => new Visao(this, jogador);

            class Visao : IArmazem
            {
                readonly Memoria m;
                readonly string jogador;
                public Visao(Memoria m, string jogador) { this.m = m; this.jogador = jogador; }
                string K(string dono, string chave) => (dono == "" ? "jogador-" + jogador : dono) + "/" + chave;

                public Task<(string texto, string trava)> Ler(string dono, string chave) =>
                    Task.FromResult(m.Dados.TryGetValue(K(dono, chave), out var v) ? (v.texto, v.versao.ToString()) : ((string)null, (string)null));

                public Task<Dictionary<string, string>> LerVarios(string dono, List<string> chaves)
                {
                    var r = new Dictionary<string, string>();
                    foreach (var c in chaves) if (m.Dados.TryGetValue(K(dono, c), out var v)) r[c] = v.texto;
                    return Task.FromResult(r);
                }

                public Task<bool> Gravar(string dono, string chave, string texto, string trava)
                {
                    var a = m.AntesDeGravar; m.AntesDeGravar = null; a?.Invoke();
                    if (trava != null && (!m.Dados.TryGetValue(K(dono, chave), out var v) || v.versao.ToString() != trava)) return Task.FromResult(false);
                    m.Dados[K(dono, chave)] = (texto, ++m.versoes);
                    return Task.FromResult(true);
                }

                public Task Apagar(string dono, string chave) { m.Dados.Remove(K(dono, chave)); return Task.CompletedTask; }
            }
        }

        static readonly DateTime Segunda = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);
        readonly ParametrosDaGuilda p = new ParametrosDaGuilda { VidaDaPrimeiraFase = 500, CrescimentoDaFase = 2, Fases = 3, MaximoDeMembros = 3 };
        Memoria memoria;
        DateTime agora;

        [SetUp]
        public void Criar() { memoria = new Memoria(); agora = Segunda; }

        VistaDaGuilda Pedir(string eu, AcaoDaGuilda acao, string texto = "", ContagemDaLuta c = null) =>
            new ServicoDaGuilda(memoria.De(eu), p, eu, "Nome " + eu, agora).Executar(new PedidoDeGuilda { Acao = acao, Texto = texto, Contagem = c }).GetAwaiter().GetResult();

        VistaDaGuilda Certo(VistaDaGuilda v) { Assert.That(v.Erro, Is.Empty); return v; }
        static void Falhou(VistaDaGuilda v) => Assert.That(v.Erro, Is.Not.Empty);

        string CriarGuilda(params string[] membros)
        {
            string codigo = Certo(Pedir("ana", AcaoDaGuilda.Criar, "Os Testes")).Codigo;
            foreach (var m in membros) Certo(Pedir(m, AcaoDaGuilda.Entrar, codigo.ToLowerInvariant()));
            return codigo;
        }

        VistaDaGuilda Lutar(string eu, int cliques = 600)
        {
            string id = Certo(Pedir(eu, AcaoDaGuilda.ComecarLuta)).LutaId;
            agora = agora.AddSeconds(60);
            return Certo(Pedir(eu, AcaoDaGuilda.TerminarLuta, id, new ContagemDaLuta { Cliques = cliques }));
        }

        [Test]
        public void Criar_e_entrar_pelo_codigo()
        {
            CriarGuilda("bia");
            var v = Certo(Pedir("bia", AcaoDaGuilda.Ver));
            Assert.That(v.Membros.Count, Is.EqualTo(2));
            Assert.That(v.SouLider, Is.False);
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).SouLider, Is.True);
        }

        [Test]
        public void Nome_codigo_lotacao_e_segunda_guilda_sao_barrados()
        {
            Falhou(Pedir("ana", AcaoDaGuilda.Criar, "x"));
            Falhou(Pedir("ana", AcaoDaGuilda.Criar, "<b>oi</b>"));
            string codigo = CriarGuilda("bia", "caio");
            Falhou(Pedir("duda", AcaoDaGuilda.Entrar, codigo));
            Falhou(Pedir("duda", AcaoDaGuilda.Entrar, "ABCDEF"));
            Falhou(Pedir("duda", AcaoDaGuilda.Entrar, "../x"));
            Falhou(Pedir("bia", AcaoDaGuilda.Criar, "Outra"));
        }

        [Test]
        public void Dois_entrando_juntos_nao_se_apagam()
        {
            string codigo = CriarGuilda();
            memoria.AntesDeGravar = () => Certo(Pedir("bia", AcaoDaGuilda.Entrar, codigo));
            Certo(Pedir("caio", AcaoDaGuilda.Entrar, codigo));
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).Membros.Count, Is.EqualTo(3));
        }

        [Test]
        public void O_dano_de_todos_soma_e_derruba_fases()
        {
            CriarGuilda("bia");
            Assert.That(Lutar("ana").PontosDaLuta, Is.EqualTo(600));
            var v = Lutar("bia");
            Assert.That(v.FasesDerrubadas, Is.EqualTo(1));
            Assert.That(v.PontosNaFase, Is.EqualTo(700));
            Assert.That(v.Membros.TrueForAll(m => m.Pontos == 600 && m.Lutas == 1));
        }

        [Test]
        public void Lutas_por_dia_tem_limite_e_voltam_no_dia_seguinte()
        {
            p.VidaDaPrimeiraFase = 1e9;
            CriarGuilda();
            for (int i = 0; i < p.LutasPorDia; i++) Lutar("ana");
            Falhou(Pedir("ana", AcaoDaGuilda.ComecarLuta));
            agora = agora.AddDays(1);
            Certo(Pedir("ana", AcaoDaGuilda.ComecarLuta));
            p.VidaDaPrimeiraFase = 500;
        }

        [Test]
        public void Trocar_de_guilda_nao_devolve_as_lutas()
        {
            CriarGuilda("bia");
            Lutar("bia", 10);
            Certo(Pedir("bia", AcaoDaGuilda.Sair));
            Assert.That(Certo(Pedir("bia", AcaoDaGuilda.Criar, "Nova")).LutasRestantes, Is.EqualTo(p.LutasPorDia - 1));
        }

        [Test]
        public void Resultado_repetido_atrasado_ou_de_outra_luta_nao_conta()
        {
            CriarGuilda();
            string id = Certo(Pedir("ana", AcaoDaGuilda.ComecarLuta)).LutaId;
            Falhou(Pedir("ana", AcaoDaGuilda.TerminarLuta, "outra", new ContagemDaLuta { Cliques = 5 }));
            Falhou(Pedir("ana", AcaoDaGuilda.TerminarLuta, ""));
            agora = agora.AddSeconds(30);
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.TerminarLuta, id, new ContagemDaLuta { Cliques = 5 })).PontosDaLuta, Is.EqualTo(5));
            Falhou(Pedir("ana", AcaoDaGuilda.TerminarLuta, id, new ContagemDaLuta { Cliques = 5 }));
            id = Certo(Pedir("ana", AcaoDaGuilda.ComecarLuta)).LutaId;
            agora = agora.AddHours(1);
            Falhou(Pedir("ana", AcaoDaGuilda.TerminarLuta, id, new ContagemDaLuta { Cliques = 5 }));
        }

        [Test]
        public void Cliques_impossiveis_sao_cortados()
        {
            CriarGuilda();
            string id = Certo(Pedir("ana", AcaoDaGuilda.ComecarLuta)).LutaId;
            agora = agora.AddSeconds(2);
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.TerminarLuta, id, new ContagemDaLuta { Cliques = int.MaxValue })).PontosDaLuta, Is.EqualTo(30));
        }

        [Test]
        public void Recompensa_so_para_quem_lutou_e_uma_vez_por_fase()
        {
            CriarGuilda("bia");
            Lutar("ana");
            Falhou(Pedir("bia", AcaoDaGuilda.Coletar));
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).FasesAColetar, Is.EqualTo(1));
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Coletar)).CristaisTotais, Is.EqualTo(p.CristaisPorFase));
            Falhou(Pedir("ana", AcaoDaGuilda.Coletar));
            Lutar("bia"); Lutar("bia");
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Coletar)).CristaisTotais, Is.EqualTo(p.CristaisPorFase * 2));
            Assert.That(Certo(Pedir("bia", AcaoDaGuilda.Coletar)).CristaisTotais, Is.EqualTo(p.CristaisPorFase * 2));
        }

        [Test]
        public void Semana_nova_tem_chefe_novo_e_a_anterior_ainda_se_coleta()
        {
            CriarGuilda();
            Lutar("ana");
            agora = Segunda.AddDays(7);
            var v = Certo(Pedir("ana", AcaoDaGuilda.Ver));
            Assert.That(v.FasesDerrubadas, Is.EqualTo(0));
            Assert.That(v.FasesAColetar, Is.EqualTo(1));
            Certo(Pedir("ana", AcaoDaGuilda.Coletar));
            agora = Segunda.AddDays(14);
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).FasesAColetar, Is.EqualTo(0));
        }

        [Test]
        public void Chefe_derrotado_nao_aceita_luta()
        {
            CriarGuilda("bia", "caio");
            foreach (var m in new[] { "ana", "bia", "caio" }) for (int i = 0; i < 2; i++) if (!Certo(Pedir(m, AcaoDaGuilda.Ver)).ChefeDerrotado) Lutar(m, 900);
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).ChefeDerrotado, Is.True);
            Falhou(Pedir("ana", AcaoDaGuilda.ComecarLuta));
        }

        [Test]
        public void Quem_sai_deixa_o_dano_e_voltar_nao_duplica()
        {
            string codigo = CriarGuilda("bia");
            Lutar("bia", 400);
            Certo(Pedir("bia", AcaoDaGuilda.Sair));
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).PontosNaFase, Is.EqualTo(400));
            Certo(Pedir("bia", AcaoDaGuilda.Entrar, codigo));
            Assert.That(Certo(Pedir("ana", AcaoDaGuilda.Ver)).PontosNaFase, Is.EqualTo(400));
        }

        [Test]
        public void Lider_que_sai_passa_o_cargo_e_guilda_vazia_some()
        {
            string codigo = CriarGuilda("bia");
            Certo(Pedir("ana", AcaoDaGuilda.Sair));
            Assert.That(Certo(Pedir("bia", AcaoDaGuilda.Ver)).SouLider, Is.True);
            Certo(Pedir("bia", AcaoDaGuilda.Sair));
            Falhou(Pedir("caio", AcaoDaGuilda.Entrar, codigo));
        }

        [Test]
        public void So_o_lider_expulsa_e_troca_o_codigo()
        {
            string codigo = CriarGuilda("bia");
            Falhou(Pedir("bia", AcaoDaGuilda.Expulsar, "ana"));
            Falhou(Pedir("bia", AcaoDaGuilda.TrocarCodigo));
            string novo = Certo(Pedir("ana", AcaoDaGuilda.TrocarCodigo)).Codigo;
            Falhou(Pedir("caio", AcaoDaGuilda.Entrar, codigo));
            Certo(Pedir("caio", AcaoDaGuilda.Entrar, novo));
            Falhou(Pedir("ana", AcaoDaGuilda.Expulsar, "ana"));
            Certo(Pedir("ana", AcaoDaGuilda.Expulsar, "bia"));
            Assert.That(Certo(Pedir("bia", AcaoDaGuilda.Ver)).TemGuilda, Is.False);
            Falhou(Pedir("bia", AcaoDaGuilda.ComecarLuta));
        }
    }
}
