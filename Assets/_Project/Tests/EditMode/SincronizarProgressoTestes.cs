using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;

namespace ALotOfDamage.Testes
{
    public class SincronizarProgressoTestes
    {
        class Repositorio : IRepositorioDeProgresso
        {
            public Progresso P = new Progresso();
            public Progresso Carregar() => P;
            public void Salvar(Progresso p) => P = p;
        }

        class Conta : IContaOnline
        {
            public bool Disponivel { get; set; } = true;
            public bool Conectada { get; set; }
            public bool SemRede, ErroNaSenha;
            public string JogadorId { get; set; } = "a";
            public string Apelido => "";
            public bool Vinculada => false;
            public Task Entrar() { if (SemRede) throw new FalhaOnline("sem rede"); Conectada = true; return Task.CompletedTask; }
            public Task Vincular(string u, string s) => Task.CompletedTask;
            public Task EntrarComConta(string u, string s) { if (ErroNaSenha) throw new FalhaOnline("senha"); JogadorId = "b"; return Task.CompletedTask; }
            public Task TrocarApelido(string a) => Task.CompletedTask;
            public Task Apagar() { Conectada = false; return Task.CompletedTask; }
        }

        // O servidor de verdade usa as mesmas regras
        class Nuvem : INuvemDeProgresso
        {
            public SaveNaNuvem Save;
            public bool SemRede, Recusar;
            public int Envios;
            public Task<SaveNaNuvem> Carregar() { if (SemRede) throw new FalhaOnline("sem rede"); return Task.FromResult(Save); }
            public Task<RespostaDoEnvio> Enviar(PedidoDeEnvio pedido)
            {
                if (SemRede) throw new FalhaOnline("sem rede");
                Envios++;
                if (Recusar) return Task.FromResult(new RespostaDoEnvio { Resultado = ResultadoDoEnvio.Recusado, Motivo = "x" });
                var r = RegrasDaNuvem.Receber(new ParametrosDaNuvem(), new ParametrosDaCampanha(), Save, null, pedido, S.Ler(pedido.Progresso), 0, DateTime.UtcNow);
                if (r.Resultado == ResultadoDoEnvio.Salvo) Save = r.Nuvem;
                return Task.FromResult(r);
            }
            public Task Apagar() { Save = null; return Task.CompletedTask; }
        }

        class Guarda : IGuardaDaSincronia
        {
            public EstadoDaSincronia E;
            public readonly List<string> Backups = new List<string>();
            public string Dispositivo => "teste";
            public EstadoDaSincronia Ler() => E;
            public void Gravar(EstadoDaSincronia e) => E = e;
            public void GuardarBackup(string p) => Backups.Add(p);
        }

        static readonly SerializadorDeProgresso S = new SerializadorDeProgresso();
        Repositorio repositorio;
        Conta conta;
        Nuvem nuvem;
        Guarda guarda;
        SincronizarProgresso s;

        [SetUp]
        public void Criar()
        {
            repositorio = new Repositorio();
            conta = new Conta();
            nuvem = new Nuvem();
            guarda = new Guarda();
            Reabrir();
        }

        void Reabrir() => s = new SincronizarProgresso(repositorio, conta, nuvem, S, guarda);
        void Abrir() => s.Abrir().GetAwaiter().GetResult();
        void Enviar() => s.EnviarSeMudou().GetAwaiter().GetResult();

        // Outro aparelho: manda um progresso por cima do que está na nuvem
        void OutroAparelho(double ouro)
        {
            var p = new Progresso();
            p.ReceberOuro(ouro);
            int revisao = nuvem.Save?.Revisao ?? 0;
            nuvem.Enviar(new PedidoDeEnvio { Progresso = S.Escrever(p.Exportar()), RevisaoBase = revisao, Versao = 1, Dispositivo = "celular", Forcar = true }).GetAwaiter().GetResult();
        }

        [Test]
        public void Sem_projeto_configurado_fica_desligada()
        {
            conta.Disponivel = false;
            Abrir();
            Assert.AreEqual(SituacaoDaNuvem.Desligada, s.Situacao);
        }

        [Test]
        public void Primeira_vez_envia_o_progresso_local()
        {
            repositorio.P.ReceberOuro(10);
            Abrir();
            Assert.AreEqual(SituacaoDaNuvem.EmDia, s.Situacao);
            Assert.AreEqual(1, guarda.E.Revisao);
            Assert.AreEqual(10, S.Ler(nuvem.Save.Progresso).Ouro);
        }

        [Test]
        public void So_envia_quando_mudou()
        {
            Abrir();
            Enviar();
            Assert.AreEqual(1, nuvem.Envios);
            repositorio.P.MarcarPresenca(DateTime.UtcNow);
            Enviar();
            Assert.AreEqual(1, nuvem.Envios);
            repositorio.P.ReceberOuro(1);
            Enviar();
            Assert.AreEqual(2, nuvem.Envios);
        }

        [Test]
        public void Sem_internet_joga_offline_e_envia_depois()
        {
            conta.SemRede = true;
            Abrir();
            Assert.AreEqual(SituacaoDaNuvem.SemConexao, s.Situacao);
            repositorio.P.ReceberOuro(5);
            conta.SemRede = false;
            Enviar();
            Assert.AreEqual(SituacaoDaNuvem.EmDia, s.Situacao);
            Assert.AreEqual(5, S.Ler(nuvem.Save.Progresso).Ouro);
        }

        [Test]
        public void Instalacao_nova_baixa_o_da_nuvem()
        {
            OutroAparelho(70);
            Abrir();
            Assert.AreEqual(70, repositorio.P.Ouro);
            Assert.AreEqual(1, s.Trocas);
            Assert.IsEmpty(guarda.Backups);
            Enviar();
            Assert.AreEqual(1, nuvem.Envios, "baixar não pode gerar um envio");
        }

        [Test]
        public void So_o_outro_aparelho_mudou_baixa_e_guarda_o_local()
        {
            repositorio.P.ReceberOuro(10);
            Abrir();
            OutroAparelho(70);
            Reabrir();
            Abrir();
            Assert.AreEqual(70, repositorio.P.Ouro);
            Assert.AreEqual(1, guarda.Backups.Count);
        }

        void AbrirEmConflito()
        {
            repositorio.P.ReceberOuro(10);
            Abrir();
            OutroAparelho(70);
            repositorio.P.ReceberOuro(1);
            Reabrir();
            Abrir();
            Assert.AreEqual(SituacaoDaNuvem.Conflito, s.Situacao);
            Assert.AreEqual(11, repositorio.P.Ouro, "nada muda antes da escolha");
        }

        [Test]
        public void Conflito_pela_nuvem()
        {
            AbrirEmConflito();
            Assert.AreEqual(70, s.ProgressoEmConflito.Ouro);
            s.ResolverConflito(true).GetAwaiter().GetResult();
            Assert.AreEqual(70, repositorio.P.Ouro);
            Assert.AreEqual(11, S.Ler(guarda.Backups[0]).Ouro);
        }

        [Test]
        public void Conflito_por_este_aparelho()
        {
            AbrirEmConflito();
            s.ResolverConflito(false).GetAwaiter().GetResult();
            Assert.AreEqual(SituacaoDaNuvem.EmDia, s.Situacao);
            Assert.AreEqual(11, S.Ler(nuvem.Save.Progresso).Ouro);
            Assert.AreEqual(70, S.Ler(guarda.Backups[0]).Ouro);
        }

        [Test]
        public void Conflito_continua_se_a_internet_cai_ou_o_servidor_recusa()
        {
            AbrirEmConflito();
            nuvem.SemRede = true;
            s.ResolverConflito(false).GetAwaiter().GetResult();
            Assert.AreEqual(SituacaoDaNuvem.Conflito, s.Situacao);
            nuvem.SemRede = false;
            nuvem.Recusar = true;
            s.ResolverConflito(false).GetAwaiter().GetResult();
            Assert.AreEqual(SituacaoDaNuvem.Conflito, s.Situacao);
            Assert.IsEmpty(guarda.Backups);
        }

        // Fechar o jogo com o conflito aberto, depois de uma tentativa que falhou, não pode escolher um lado sozinho
        [Test]
        public void Conflito_nao_se_resolve_sozinho_ao_reabrir()
        {
            AbrirEmConflito();
            nuvem.SemRede = true;
            s.ResolverConflito(false).GetAwaiter().GetResult();
            nuvem.SemRede = false;
            Reabrir();
            Abrir();
            Assert.AreEqual(SituacaoDaNuvem.Conflito, s.Situacao);
        }

        [Test]
        public void Conflito_enquanto_joga_aparece_no_envio()
        {
            Abrir();
            OutroAparelho(70);
            repositorio.P.ReceberOuro(1);
            Enviar();
            Assert.AreEqual(SituacaoDaNuvem.Conflito, s.Situacao);
        }

        [Test]
        public void Save_de_jogo_mais_novo_nao_e_baixado()
        {
            OutroAparelho(70);
            nuvem.Save.Versao = RegrasDaNuvem.VersaoDoSave + 1;
            Abrir();
            Assert.AreEqual(SituacaoDaNuvem.JogoAntigo, s.Situacao);
            Assert.AreEqual(0, repositorio.P.Ouro);
        }

        [Test]
        public void Save_ilegivel_na_nuvem_nao_apaga_o_local()
        {
            repositorio.P.ReceberOuro(10);
            Abrir();
            OutroAparelho(70);
            nuvem.Save.Progresso = "{ quebrado";
            Reabrir();
            Abrir();
            Assert.AreEqual(10, repositorio.P.Ouro);
        }

        [Test]
        public void Entrar_em_outra_conta_com_progresso_pergunta_qual_vale()
        {
            repositorio.P.ReceberOuro(10);
            Abrir();
            OutroAparelho(70);
            s.EntrarComConta("u", "s").GetAwaiter().GetResult();
            Assert.AreEqual(SituacaoDaNuvem.Conflito, s.Situacao);
        }

        [Test]
        public void Senha_errada_avisa_e_mantem_a_conta()
        {
            Abrir();
            conta.ErroNaSenha = true;
            Assert.Throws<FalhaOnline>(() => s.EntrarComConta("u", "s").GetAwaiter().GetResult());
            Assert.AreEqual(SituacaoDaNuvem.EmDia, s.Situacao);
        }

        [Test]
        public void Apagar_a_conta_mantem_o_local_e_nao_cria_outra_sozinha()
        {
            repositorio.P.ReceberOuro(10);
            Abrir();
            s.ApagarConta().GetAwaiter().GetResult();
            Assert.IsNull(nuvem.Save);
            Assert.AreEqual(10, repositorio.P.Ouro);
            Enviar();
            Assert.IsFalse(conta.Conectada);
        }
    }
}
