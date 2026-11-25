using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ALotOfDamage.Core;

namespace ALotOfDamage.Nuvem
{
    // Posições da arena: Leaderboards no servidor, memória nos testes
    public interface IPosicoesDaArena
    {
        Task Pontuar(double pontos);
        Task<List<LinhaDoRanking>> Perto(int alcance);
    }

    public class SorteioSeguro : ISorteio
    {
        public double Proximo() => RandomNumberGenerator.GetInt32(1 << 30) / (double)(1 << 30);
    }

    // PvP assíncrono: a luta é decidida aqui, com o Poder dos saves guardados
    public class ServicoDaArena
    {
        const int Tentativas = 4, Alcance = 8;
        const string Chave = "arena";

        readonly IArmazem armazem;
        readonly IPosicoesDaArena posicoes;
        readonly ParametrosDaArena p;
        readonly ISorteio sorteio;
        readonly string eu;
        readonly DateTime agora;

        class Erro : Exception { public Erro(string m) : base(m) { } }

        public ServicoDaArena(IArmazem armazem, IPosicoesDaArena posicoes, ParametrosDaArena p, ISorteio sorteio, string eu, DateTime agora)
        {
            this.armazem = armazem;
            this.posicoes = posicoes;
            this.p = p;
            this.sorteio = sorteio;
            this.eu = eu;
            this.agora = agora;
        }

        public async Task<VistaDaArena> Executar(PedidoDaArena pedido)
        {
            if (pedido == null) return new VistaDaArena { Erro = "Pedido inválido." };
            try
            {
                var meu = await Save("");
                if (meu == null || !meu.Existe) throw new Erro("Sincronize o progresso antes de entrar na arena.");

                long premio = 0;
                ResultadoDaArena resultado = null;
                var j = await Mudar(x =>
                {
                    premio = RegrasDaArena.Virar(p, x, agora);
                    if (pedido.Acao == AcaoDaArena.OutrosAdversarios) x.Ofertas.Clear();
                    if (pedido.Acao != AcaoDaArena.Lutar) return;
                    if (RegrasDaArena.LutasRestantes(p, x, agora) <= 0) throw new Erro("Suas lutas de hoje acabaram.");
                    resultado = RegrasDaArena.Enfrentar(p, x, pedido.Texto, meu.Poder, sorteio, agora) ?? throw new Erro("Este adversário não está mais disponível.");
                });
                await posicoes.Pontuar(j.Pontos);

                if (j.Ofertas.Count == 0)
                {
                    var ofertas = await Procurar(j);
                    j = await Mudar(x => x.Ofertas = ofertas);
                }
                var liga = RegrasDaArena.Liga(p, j.Pontos);
                return new VistaDaArena
                {
                    Pontos = j.Pontos, LutasRestantes = RegrasDaArena.LutasRestantes(p, j, agora), Liga = liga.Nome, CristaisDaLiga = liga.Cristais,
                    CristaisTotais = j.CristaisTotais, MeuPoder = meu.Poder, SegundosParaVirar = RegrasDaArena.SegundosParaVirar(agora),
                    Adversarios = j.Ofertas, Resultado = resultado ?? new ResultadoDaArena(), RecompensaDaTemporada = premio,
                };
            }
            catch (Erro e) { return new VistaDaArena { Erro = e.Message }; }
        }

        async Task<SaveNaNuvem> Save(string jogadorId)
        {
            var (texto, _) = await armazem.Ler(jogadorId == "" ? "" : Armazens.DoJogador(jogadorId), ModuloDaNuvem.Chave);
            return Recepcao.Ler<SaveNaNuvem>(texto);
        }

        async Task<JogadorNaArena> Mudar(Action<JogadorNaArena> mudanca)
        {
            for (int i = 0; i < Tentativas; i++)
            {
                var (texto, trava) = await armazem.Ler("", Chave);
                var j = string.IsNullOrEmpty(texto) ? new JogadorNaArena() : Recepcao.Ler<JogadorNaArena>(texto) ?? throw new InvalidOperationException("arena ilegível");
                mudanca(j);
                if (await armazem.Gravar("", Chave, Recepcao.Escrever(j), trava)) return j;
            }
            throw new Erro("O servidor está ocupado, tente de novo.");
        }

        // Sorteia entre os que estão perto na classificação e têm save
        async Task<List<AdversarioDaArena>> Procurar(JogadorNaArena j)
        {
            var perto = await posicoes.Perto(Alcance);
            perto.RemoveAll(l => l.JogadorId == eu);
            for (int i = perto.Count - 1; i > 0; i--)
            {
                int k = (int)(sorteio.Proximo() * (i + 1));
                (perto[i], perto[k]) = (perto[k], perto[i]);
            }
            var r = new List<AdversarioDaArena>();
            foreach (var l in perto)
            {
                if (r.Count >= p.Adversarios) break;
                var save = await Save(l.JogadorId);
                if (save == null || !save.Existe) continue;
                var dados = Recepcao.Ler<DadosDoProgresso>(save.Progresso);
                if (dados == null) continue;
                r.Add(new AdversarioDaArena { JogadorId = l.JogadorId, Nome = RegrasDaGuilda.NomeLimpo(l.Nome), Pontos = (int)l.Pontos, Poder = save.Poder, HeroiId = dados.HeroiId ?? "", Estagio = dados.EstagioMaximo });
            }
            r.Sort((a, b) => a.Poder.CompareTo(b.Poder));
            return r;
        }
    }
}
