using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ALotOfDamage.Core;

namespace ALotOfDamage.Nuvem
{
    // Onde a guilda fica guardada: Cloud Save no servidor, memória nos testes.
    // Dono vazio é o próprio jogador; "Gravar" com trava falha (false) se o valor mudou desde a leitura
    public interface IArmazem
    {
        Task<(string texto, string trava)> Ler(string dono, string chave);
        Task<Dictionary<string, string>> LerVarios(string dono, List<string> chaves);   // só as que existem
        Task<bool> Gravar(string dono, string chave, string texto, string trava);
        Task Apagar(string dono, string chave);
    }

    public class ServicoDaGuilda
    {
        const int Tentativas = 4;
        const string ChaveDoJogador = "guilda", ChaveDaGuilda = "dados", ChaveDoCodigo = "guilda";

        readonly IArmazem armazem;
        readonly ParametrosDaGuilda p;
        readonly string eu, meuNome;
        readonly DateTime agora;
        readonly int semana;

        class Contribuicao { public List<PontosDaSemana> Semanas = new List<PontosDaSemana>(); }
        class Ponteiro { public string GuildaId = ""; }

        class Erro : Exception { public Erro(string m) : base(m) { } }

        public ServicoDaGuilda(IArmazem armazem, ParametrosDaGuilda p, string eu, string meuNome, DateTime agora)
        {
            this.armazem = armazem;
            this.p = p;
            this.eu = eu;
            this.meuNome = RegrasDaGuilda.NomeLimpo(meuNome);
            this.agora = agora;
            semana = RegrasDaGuilda.Semana(agora);
        }

        public async Task<VistaDaGuilda> Executar(PedidoDeGuilda pedido)
        {
            if (pedido == null) return new VistaDaGuilda { Erro = "Pedido inválido." };
            try
            {
                double pontos = 0;
                switch (pedido.Acao)
                {
                    case AcaoDaGuilda.Ver: break;
                    case AcaoDaGuilda.Criar: await Criar(pedido.Texto); break;
                    case AcaoDaGuilda.Entrar: await Entrar(pedido.Texto); break;
                    case AcaoDaGuilda.Sair: await Sair(); break;
                    case AcaoDaGuilda.Expulsar: await Expulsar(pedido.Texto); break;
                    case AcaoDaGuilda.TrocarCodigo: await TrocarCodigo(); break;
                    case AcaoDaGuilda.ComecarLuta: await ComecarLuta(); break;
                    case AcaoDaGuilda.TerminarLuta: pontos = await TerminarLuta(pedido.Texto, pedido.Contagem); break;
                    case AcaoDaGuilda.Coletar: await Coletar(); break;
                    default: throw new Erro("Pedido inválido.");
                }
                var v = await Ver();
                v.PontosDaLuta = pontos;
                return v;
            }
            catch (Erro e) { return new VistaDaGuilda { Erro = e.Message }; }
        }

        static string DonoDaGuilda(string id) => "guilda-" + id;
        static string DonoDoCodigo(string codigo) => "codigo-" + codigo;
        static string ChaveDoDano(string jogadorId) => "dano-" + jogadorId;

        async Task<(T valor, string trava)> Ler<T>(string dono, string chave) where T : class
        {
            var (texto, trava) = await armazem.Ler(dono, chave);
            if (string.IsNullOrEmpty(texto)) return (null, trava);
            // dado que existe e não se lê não pode ser tratado como vazio e sobrescrito
            return (Recepcao.Ler<T>(texto) ?? throw new InvalidOperationException($"dado ilegível em {chave}"), trava);
        }

        async Task<(JogadorNaGuilda j, string trava)> Jogador()
        {
            var (j, trava) = await Ler<JogadorNaGuilda>("", ChaveDoJogador);
            return (j ?? new JogadorNaGuilda(), trava);
        }

        async Task<(DadosDaGuilda g, string trava)> Guilda(string id) =>
            string.IsNullOrEmpty(id) ? (null, null) : await Ler<DadosDaGuilda>(DonoDaGuilda(id), ChaveDaGuilda);

        // A guilda de que faço parte de verdade: quem foi expulso ainda tem o id guardado
        async Task<(DadosDaGuilda g, string trava)> Minha(JogadorNaGuilda j)
        {
            var (g, trava) = await Guilda(j.GuildaId);
            return g != null && g.Membro(eu) != null ? (g, trava) : (null, null);
        }

        // Lê, muda e grava com trava, repetindo se outra pessoa gravou no meio
        async Task Mudar(Action<JogadorNaGuilda> mudanca)
        {
            for (int i = 0; i < Tentativas; i++)
            {
                var (j, trava) = await Jogador();
                mudanca(j);
                if (await armazem.Gravar("", ChaveDoJogador, Recepcao.Escrever(j), trava)) return;
            }
            throw new Erro("O servidor está ocupado, tente de novo.");
        }

        async Task MudarGuilda(string id, Action<DadosDaGuilda> mudanca)
        {
            for (int i = 0; i < Tentativas; i++)
            {
                var (g, trava) = await Guilda(id);
                if (g == null) throw new Erro("Esta guilda não existe mais.");
                mudanca(g);
                if (await armazem.Gravar(DonoDaGuilda(id), ChaveDaGuilda, Recepcao.Escrever(g), trava)) return;
            }
            throw new Erro("O servidor está ocupado, tente de novo.");
        }

        async Task<Dictionary<string, List<PontosDaSemana>>> Contribuicoes(DadosDaGuilda g)
        {
            var lidas = await armazem.LerVarios(DonoDaGuilda(g.Id), g.Membros.ConvertAll(m => ChaveDoDano(m.JogadorId)));
            var r = new Dictionary<string, List<PontosDaSemana>>();
            foreach (var m in g.Membros)
                r[m.JogadorId] = lidas.TryGetValue(ChaveDoDano(m.JogadorId), out var t) ? Recepcao.Ler<Contribuicao>(t)?.Semanas ?? new List<PontosDaSemana>() : new List<PontosDaSemana>();
            return r;
        }

        async Task ExigirSemGuilda()
        {
            var (j, _) = await Jogador();
            if ((await Minha(j)).g != null) throw new Erro("Você já está em uma guilda.");
        }

        async Task<DadosDaGuilda> ExigirGuilda()
        {
            var (j, _) = await Jogador();
            return (await Minha(j)).g ?? throw new Erro("Você não está em uma guilda.");
        }

        MembroDaGuilda Eu() => new MembroDaGuilda { JogadorId = eu, Nome = meuNome, EntrouEmUtc = agora.Ticks };

        async Task Criar(string nome)
        {
            string problema = RegrasDaGuilda.ProblemaDoNome(nome);
            if (problema != null) throw new Erro(problema);
            await ExigirSemGuilda();
            var g = new DadosDaGuilda { Id = Guid.NewGuid().ToString("N"), Nome = nome.Trim(), LiderId = eu };
            g.Membros.Add(Eu());
            g.Codigo = await ReservarCodigo(g.Id);
            await armazem.Gravar(DonoDaGuilda(g.Id), ChaveDaGuilda, Recepcao.Escrever(g), null);
            await Mudar(j => j.GuildaId = g.Id);
        }

        async Task<string> ReservarCodigo(string guildaId)
        {
            for (int i = 0; i < Tentativas; i++)
            {
                var c = new char[RegrasDaGuilda.TamanhoDoCodigo];
                for (int k = 0; k < c.Length; k++) c[k] = RegrasDaGuilda.LetrasDoCodigo[RandomNumberGenerator.GetInt32(RegrasDaGuilda.LetrasDoCodigo.Length)];
                string codigo = new string(c);
                if ((await Ler<Ponteiro>(DonoDoCodigo(codigo), ChaveDoCodigo)).valor != null) continue;
                await armazem.Gravar(DonoDoCodigo(codigo), ChaveDoCodigo, Recepcao.Escrever(new Ponteiro { GuildaId = guildaId }), null);
                return codigo;
            }
            throw new Erro("O servidor está ocupado, tente de novo.");
        }

        async Task Entrar(string texto)
        {
            string codigo = RegrasDaGuilda.CodigoLimpo(texto);
            if (!RegrasDaGuilda.CodigoValido(codigo)) throw new Erro("Código inválido.");
            await ExigirSemGuilda();
            var (alvo, _) = await Ler<Ponteiro>(DonoDoCodigo(codigo), ChaveDoCodigo);
            if (alvo == null) throw new Erro("Nenhuma guilda usa este código.");
            // quem volta tem os pontos antigos guardados na parte de quem saiu: começa do zero para não contar duas vezes
            var (antes, _) = await Guilda(alvo.GuildaId);
            if (antes != null && antes.Membro(eu) == null) await armazem.Apagar(DonoDaGuilda(alvo.GuildaId), ChaveDoDano(eu));
            await MudarGuilda(alvo.GuildaId, g =>
            {
                if (g.Codigo != codigo) throw new Erro("Nenhuma guilda usa este código.");
                if (g.Membro(eu) != null) return;
                if (g.Membros.Count >= p.MaximoDeMembros) throw new Erro("Esta guilda está cheia.");
                g.Membros.Add(Eu());
            });
            await Mudar(j => j.GuildaId = alvo.GuildaId);
        }

        async Task Sair()
        {
            var g = await ExigirGuilda();
            await Tirar(g, eu);
            await Mudar(j => { j.GuildaId = ""; j.LutaId = ""; });
        }

        async Task Expulsar(string jogadorId)
        {
            var g = await ExigirGuilda();
            if (g.LiderId != eu) throw new Erro("Só o líder pode expulsar.");
            if (jogadorId == eu || g.Membro(jogadorId) == null) throw new Erro("Este jogador não pode ser expulso.");
            await Tirar(g, jogadorId, true);
        }

        async Task Tirar(DadosDaGuilda g, string jogadorId, bool soLider = false)
        {
            var contribuicao = (await Ler<Contribuicao>(DonoDaGuilda(g.Id), ChaveDoDano(jogadorId))).valor?.Semanas ?? new List<PontosDaSemana>();
            bool vazia = false;
            await MudarGuilda(g.Id, x =>
            {
                if (soLider && x.LiderId != eu) throw new Erro("Só o líder pode expulsar.");
                RegrasDaGuilda.Remover(x, jogadorId, contribuicao, semana);
                vazia = x.Membros.Count == 0;
            });
            if (!vazia) return;
            await armazem.Apagar(DonoDoCodigo(g.Codigo), ChaveDoCodigo);
            await armazem.Apagar(DonoDaGuilda(g.Id), ChaveDaGuilda);
        }

        async Task TrocarCodigo()
        {
            var g = await ExigirGuilda();
            if (g.LiderId != eu) throw new Erro("Só o líder pode trocar o código.");
            string novo = await ReservarCodigo(g.Id), antigo = "";
            await MudarGuilda(g.Id, x =>
            {
                if (x.LiderId != eu) throw new Erro("Só o líder pode trocar o código.");
                antigo = x.Codigo;
                x.Codigo = novo;
            });
            await armazem.Apagar(DonoDoCodigo(antigo), ChaveDoCodigo);
        }

        async Task ComecarLuta()
        {
            var g = await ExigirGuilda();
            if (Placar(g, await Contribuicoes(g), semana).chefe.Derrotado) throw new Erro("O chefe desta semana já caiu.");
            string id = Guid.NewGuid().ToString("N");
            await Mudar(j =>
            {
                if (!RegrasDaGuilda.Comecar(p, j, id, agora)) throw new Erro("Suas lutas de hoje acabaram.");
            });
        }

        async Task<double> TerminarLuta(string lutaId, ContagemDaLuta contagem)
        {
            var g = await ExigirGuilda();
            double? pontos = null;
            int semanaDaLuta = semana;
            await Mudar(j =>
            {
                semanaDaLuta = j.LutaInicioUtc > 0 ? RegrasDaGuilda.Semana(new DateTime(j.LutaInicioUtc, DateTimeKind.Utc)) : semana;
                pontos = RegrasDaGuilda.Terminar(p, j, lutaId, contagem, agora);
            });
            if (pontos == null) throw new Erro("Esta luta já acabou.");

            // cada um só grava a própria parte; a soma é feita na leitura
            for (int i = 0; i < Tentativas; i++)
            {
                var (c, trava) = await Ler<Contribuicao>(DonoDaGuilda(g.Id), ChaveDoDano(eu));
                c = c ?? new Contribuicao();
                RegrasDaGuilda.Somar(c.Semanas, semanaDaLuta, pontos.Value, 1, semana);
                if (await armazem.Gravar(DonoDaGuilda(g.Id), ChaveDoDano(eu), Recepcao.Escrever(c), trava)) return pontos.Value;
            }
            throw new Erro("O servidor está ocupado, tente de novo.");
        }

        async Task Coletar()
        {
            var g = await ExigirGuilda();
            var contribuicoes = await Contribuicoes(g);
            await Mudar(j =>
            {
                int novas = 0;
                foreach (int s in new[] { semana - 1, semana })
                {
                    var (chefe, minha) = Placar(g, contribuicoes, s);
                    novas += RegrasDaGuilda.Coletar(p, j, s, chefe.FasesDerrubadas, minha.Lutas > 0, semana);
                }
                if (novas == 0) throw new Erro("Nada para coletar ainda.");
            });
        }

        (SituacaoDoChefe chefe, PontosDaSemana minha) Placar(DadosDaGuilda g, Dictionary<string, List<PontosDaSemana>> contribuicoes, int s)
        {
            double total = RegrasDaGuilda.NaSemana(g.DeQuemSaiu, s).Pontos;
            foreach (var c in contribuicoes.Values) total += RegrasDaGuilda.NaSemana(c, s).Pontos;
            return (RegrasDaGuilda.Chefe(p, total), RegrasDaGuilda.NaSemana(contribuicoes[eu], s));
        }

        async Task<VistaDaGuilda> Ver()
        {
            var (j, _) = await Jogador();
            var v = new VistaDaGuilda { CristaisTotais = j.CristaisTotais, FragmentosTotais = j.FragmentosTotais, SegundosParaVirar = RegrasDaGuilda.SegundosParaVirar(agora) };
            var (g, _) = await Minha(j);
            if (g == null) return v;

            var contribuicoes = await Contribuicoes(g);
            var (chefe, minha) = Placar(g, contribuicoes, semana);
            var (chefeAnterior, minhaAnterior) = Placar(g, contribuicoes, semana - 1);
            foreach (var m in g.Membros)
            {
                var c = RegrasDaGuilda.NaSemana(contribuicoes[m.JogadorId], semana);
                v.Membros.Add(new MembroNaLista { JogadorId = m.JogadorId, Nome = m.Nome, Lider = m.JogadorId == g.LiderId, Pontos = c.Pontos, Lutas = c.Lutas });
            }
            v.Membros.Sort((x, y) => y.Pontos.CompareTo(x.Pontos));
            v.TemGuilda = true;
            v.SouLider = g.LiderId == eu;
            v.Nome = g.Nome;
            v.Codigo = g.Codigo;
            v.Canal = g.Id;
            v.FasesDerrubadas = chefe.FasesDerrubadas;
            v.PontosNaFase = chefe.PontosNaFase;
            v.VidaDaFase = chefe.VidaDaFase;
            v.ChefeDerrotado = chefe.Derrotado;
            v.LutasRestantes = RegrasDaGuilda.LutasRestantes(p, j, agora);
            v.LutaId = j.LutaId;
            v.FasesAColetar = RegrasDaGuilda.FasesAColetar(j, semana, chefe.FasesDerrubadas, minha.Lutas > 0)
                + RegrasDaGuilda.FasesAColetar(j, semana - 1, chefeAnterior.FasesDerrubadas, minhaAnterior.Lutas > 0);
            return v;
        }
    }
}
