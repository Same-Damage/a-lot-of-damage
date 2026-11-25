using System;
using System.Threading.Tasks;

namespace ALotOfDamage.Core
{
    public enum SituacaoDaNuvem { NaoIniciada, Desligada, Conectando, SemConexao, EmDia, Enviando, Conflito, Recusado, JogoAntigo }

    // Mantém o progresso local e o da nuvem iguais. O local é sempre o que o jogo usa; sem internet, nada muda
    public class SincronizarProgresso
    {
        readonly IRepositorioDeProgresso repositorio;
        readonly IContaOnline conta;
        readonly INuvemDeProgresso nuvem;
        readonly ISerializadorDeProgresso serializador;
        readonly IGuardaDaSincronia guarda;
        bool ocupado;

        public SincronizarProgresso(IRepositorioDeProgresso repositorio, IContaOnline conta, INuvemDeProgresso nuvem,
            ISerializadorDeProgresso serializador, IGuardaDaSincronia guarda)
        {
            this.repositorio = repositorio;
            this.conta = conta;
            this.nuvem = nuvem;
            this.serializador = serializador;
            this.guarda = guarda;
        }

        public SituacaoDaNuvem Situacao { get; private set; }
        public string Motivo { get; private set; } = "";
        public SaveNaNuvem EmConflito { get; private set; }
        // Sobe quando o progresso local foi trocado pelo da nuvem: a cena precisa recomeçar
        public int Trocas { get; private set; }
        // Depois de apagar a conta, não cria outra sozinho até o jogo ser reaberto
        public bool Encerrada { get; private set; }

        DadosDoProgresso Local => repositorio.Carregar().Exportar();
        string ImpressaoDe(DadosDoProgresso d) => RegrasDaNuvem.Impressao(d, serializador.Escrever);

        public DadosDoProgresso ProgressoLocal => Local;
        public DadosDoProgresso ProgressoEmConflito => EmConflito == null ? null : serializador.Ler(EmConflito.Progresso);

        // Entra na conta e compara com a nuvem; chamado ao abrir o jogo e para tentar de novo
        public Task Abrir() => Tentar(async () =>
        {
            if (!conta.Disponivel) { Situacao = SituacaoDaNuvem.Desligada; return; }
            Situacao = SituacaoDaNuvem.Conectando;
            if (!conta.Conectada) await conta.Entrar();
            await Comparar();
        });

        // Envio periódico: só se mudou desde a última vez. Sem conexão, tenta entrar de novo
        public Task EnviarSeMudou()
        {
            if (Situacao == SituacaoDaNuvem.SemConexao) return Abrir();
            if (Situacao != SituacaoDaNuvem.EmDia && Situacao != SituacaoDaNuvem.Recusado) return Task.CompletedTask;
            if (ImpressaoDe(Local) == EstadoDaConta().Impressao) return Task.CompletedTask;
            return Tentar(() => Enviar(false, EstadoDaConta().Revisao));
        }

        public Task ResolverConflito(bool usarNuvem)
        {
            if (Situacao != SituacaoDaNuvem.Conflito || EmConflito == null) return Task.CompletedTask;
            var save = EmConflito;
            if (usarNuvem) return Tentar(() => { Baixar(save); return Task.CompletedTask; });
            return Tentar(async () =>
            {
                await Enviar(true, save.Revisao);
                if (Situacao == SituacaoDaNuvem.EmDia) guarda.GuardarBackup(save.Progresso);
                // recusado: o conflito continua aberto e o jogador pode escolher a nuvem
                else if (Situacao == SituacaoDaNuvem.Recusado) { Situacao = SituacaoDaNuvem.Conflito; EmConflito = save; }
            });
        }

        // Entra em outra conta: o progresso deste aparelho passa a ser comparado com o dela
        public Task EntrarComConta(string usuario, string senha) => Executar(async () =>
        {
            await conta.EntrarComConta(usuario, senha);
            EmConflito = null;
            Encerrada = false;
            Situacao = SituacaoDaNuvem.Conectando;
            await Comparar();
        });

        // Apaga a conta e o que está na nuvem; o progresso local fica
        public Task ApagarConta() => Executar(async () =>
        {
            await nuvem.Apagar();
            await conta.Apagar();
            guarda.Gravar(new EstadoDaSincronia());
            EmConflito = null;
            Situacao = SituacaoDaNuvem.SemConexao;
            Motivo = "conta apagada";
            Encerrada = true;
        });

        async Task Comparar()
        {
            var save = await nuvem.Carregar() ?? new SaveNaNuvem();
            if (save.Versao > RegrasDaNuvem.VersaoDoSave) { Situacao = SituacaoDaNuvem.JogoAntigo; return; }
            var local = Local;
            var estado = EstadoDaConta();
            switch (RegrasDaNuvem.Decidir(estado.Revisao, save.Revisao, ImpressaoDe(local) != estado.Impressao, RegrasDaNuvem.Vazio(local)))
            {
                case AcaoDaSincronia.Enviar:
                    await Enviar(false, save.Revisao);
                    break;
                case AcaoDaSincronia.Baixar: Baixar(save); break;
                case AcaoDaSincronia.Conflito: EmConflito = save; Situacao = SituacaoDaNuvem.Conflito; break;
                default: Situacao = SituacaoDaNuvem.EmDia; break;
            }
        }

        async Task Enviar(bool forcar, int revisaoBase)
        {
            Situacao = SituacaoDaNuvem.Enviando;
            var dados = Local;
            string texto = serializador.Escrever(dados);
            string impressao = ImpressaoDe(dados);
            var r = await nuvem.Enviar(new PedidoDeEnvio
            {
                Progresso = texto, RevisaoBase = revisaoBase,
                Versao = RegrasDaNuvem.VersaoDoSave, Dispositivo = guarda.Dispositivo, Forcar = forcar,
            });
            Motivo = r.Motivo;
            switch (r.Resultado)
            {
                case ResultadoDoEnvio.Salvo:
                    Gravar(r.Nuvem.Revisao, impressao);
                    EmConflito = null;
                    Situacao = SituacaoDaNuvem.EmDia;
                    break;
                case ResultadoDoEnvio.Conflito: EmConflito = r.Nuvem; Situacao = SituacaoDaNuvem.Conflito; break;
                case ResultadoDoEnvio.JogoAntigo: Situacao = SituacaoDaNuvem.JogoAntigo; break;
                default: Situacao = SituacaoDaNuvem.Recusado; break;
            }
        }

        void Baixar(SaveNaNuvem save)
        {
            var dados = serializador.Ler(save.Progresso);
            if (RegrasDaNuvem.ProblemaDosDados(dados) != null) throw new FalhaOnline("O progresso da nuvem está ilegível.");
            var local = Local;
            if (!RegrasDaNuvem.Vazio(local)) guarda.GuardarBackup(serializador.Escrever(local));
            var p = Progresso.Importar(dados);
            repositorio.Salvar(p);
            Gravar(save.Revisao, ImpressaoDe(p.Exportar()));
            EmConflito = null;
            Situacao = SituacaoDaNuvem.EmDia;
            Trocas++;
        }

        // Estado de outra conta não vale para esta
        EstadoDaSincronia EstadoDaConta()
        {
            var e = guarda.Ler();
            return e != null && e.JogadorId == conta.JogadorId ? e : new EstadoDaSincronia { JogadorId = conta.JogadorId };
        }

        void Gravar(int revisao, string impressao) =>
            guarda.Gravar(new EstadoDaSincronia { JogadorId = conta.JogadorId, Revisao = revisao, Impressao = impressao });

        // Uma operação por vez; falha de rede fica em Situacao e Motivo
        async Task Tentar(Func<Task> acao)
        {
            if (ocupado || Encerrada) return;
            try { await Executar(acao); }
            catch (FalhaOnline) { }
        }

        async Task Executar(Func<Task> acao)
        {
            if (ocupado) throw new FalhaOnline("Aguarde a sincronização terminar.");
            ocupado = true;
            var antes = Situacao;
            try { await acao(); }
            catch (FalhaOnline e)
            {
                Motivo = e.Message;
                // um conflito aberto não se perde, e uma ação que falhou antes de mudar algo não derruba a sincronia
                bool mantem = (antes == SituacaoDaNuvem.Conflito && EmConflito != null) || (Situacao == antes && antes == SituacaoDaNuvem.EmDia);
                Situacao = mantem ? antes : SituacaoDaNuvem.SemConexao;
                throw;
            }
            finally { ocupado = false; }
        }
    }
}
