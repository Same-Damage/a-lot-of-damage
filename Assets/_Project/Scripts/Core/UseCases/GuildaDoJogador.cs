using System.Threading.Tasks;

namespace ALotOfDamage.Core
{
    public struct RecompensaDaGuilda
    {
        public long Cristais, Fragmentos;
    }

    // Conversa com o servidor da guilda e entrega no progresso o que ele deu. Erros viram FalhaOnline
    public class GuildaDoJogador
    {
        readonly IRepositorioDeProgresso repositorio;
        readonly IServicoDeGuilda servico;

        public GuildaDoJogador(IRepositorioDeProgresso repositorio, IServicoDeGuilda servico)
        {
            this.repositorio = repositorio;
            this.servico = servico;
        }

        // Última situação conhecida; null antes da primeira resposta
        public VistaDaGuilda Vista { get; private set; }

        public async Task<RecompensaDaGuilda> Pedir(AcaoDaGuilda acao, string texto = "", ContagemDaLuta contagem = null)
        {
            var v = await servico.Pedir(new PedidoDeGuilda { Acao = acao, Texto = texto, Contagem = contagem ?? new ContagemDaLuta() });
            if (!string.IsNullOrEmpty(v.Erro)) throw new FalhaOnline(v.Erro);
            Vista = v;
            return Receber(v);
        }

        RecompensaDaGuilda Receber(VistaDaGuilda v)
        {
            var p = repositorio.Carregar();
            var r = p.ReceberDaGuilda(v.CristaisTotais, v.FragmentosTotais);
            if (r.Cristais > 0 || r.Fragmentos > 0) repositorio.Salvar(p);
            return r;
        }

        // Abre a luta no servidor (gasta a tentativa) e devolve o que a arena precisa
        public async Task<DesafioDaGuilda> ComecarLuta()
        {
            await Pedir(AcaoDaGuilda.ComecarLuta);
            return new DesafioDaGuilda { LutaId = Vista.LutaId, Fase = Vista.FasesDerrubadas + 1, PontosNaFase = Vista.PontosNaFase, VidaDaFase = Vista.VidaDaFase };
        }

        // Manda o resultado; a tentativa já foi gasta, então insiste um pouco se a rede falhar
        public async Task<double> TerminarLuta(string lutaId, ContagemDaLuta contagem, int tentativas = 3)
        {
            for (int i = 1; ; i++)
            {
                try
                {
                    await Pedir(AcaoDaGuilda.TerminarLuta, lutaId, contagem);
                    return Vista.PontosDaLuta;
                }
                catch (FalhaOnline) when (i < tentativas) { await Task.Delay(1500); }
            }
        }
    }
}
