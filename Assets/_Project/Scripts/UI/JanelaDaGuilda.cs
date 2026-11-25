using System;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Guilda: sem guilda, criar ou entrar por código; com guilda, o chefe da semana, as lutas, a recompensa e os membros
    public class JanelaDaGuilda : JanelaDoJogo
    {
        const int Colunas = 2;
        const float Validade = 30f;

        protected override string Titulo => "Guilda";
        protected override Key Tecla => Key.N;
        protected override int Ordem => 11;
        protected override IconeDoHud Icone => IconeDoHud.Guilda;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1180, 880);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        GuildaDoJogador guilda;
        IContaOnline conta;
        IEntradaNaArena arena;

        GameObject semGuilda, comGuilda;
        InputField nome, codigo;
        Text aviso, titulo, convite, chefe, vida, tempo, rotuloDeLutar, rotuloDeColetar, rotuloDeSair;
        Barra barra;
        Button lutar, coletar, trocarCodigo;
        Image[] fundos;
        Text[] nomes, pontos;
        Button[] expulsar;
        bool fazendo;
        float buscadoEm = float.NegativeInfinity, fimDaConfirmacao;
        string confirmando;
        VistaDaGuilda mostrada;

        [Inject]
        public void Construir(GuildaDoJogador guilda, IContaOnline conta, IEntradaNaArena arena)
        {
            this.guilda = guilda;
            this.conta = conta;
            this.arena = arena;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            var sem = KitDeInterface.Novo("Sem guilda", painel);
            KitDeInterface.Esticar(sem);
            semGuilda = sem.gameObject;
            Rotulo(sem, "Criar uma guilda", 52, 130, 30);
            nome = Campo(sem, "nome da guilda", 24, 52, 184);
            Posicionar(KitDeInterface.Botao("Criar", sem, "Criar", () => Fazer(() => guilda.Pedir(AcaoDaGuilda.Criar, nome.text), "Guilda criada!")), 700, 184, 240, 60);
            Rotulo(sem, "Entrar com o código de um amigo", 52, 310, 30);
            codigo = Campo(sem, "código", RegrasDaGuilda.TamanhoDoCodigo, 52, 364);
            Posicionar(KitDeInterface.Botao("Entrar", sem, "Entrar", () => Fazer(() => guilda.Pedir(AcaoDaGuilda.Entrar, codigo.text), "Você entrou na guilda!")), 700, 364, 240, 60);
            var regras = KitDeInterface.Texto("Regras", sem, 22, KitDeInterface.TextoClaro, TextAnchor.UpperLeft);
            regras.horizontalOverflow = HorizontalWrapMode.Wrap;
            KitDeInterface.Ancorar((RectTransform)regras.transform, new Vector2(0, 1), new Vector2(52, -500), new Vector2(1060, 160));
            var p = catalogo.Guilda;
            regras.text = $"Toda semana, {p.NomeDoChefe} desafia a guilda. Cada membro tem {p.LutasPorDia} lutas por dia, de {p.SegundosDaLuta:0} segundos.\n"
                + "Contam os cliques, o combo, os pontos fracos e as ultimates, não o Poder. Cada fase derrubada dá recompensa a quem lutou.";

            var com = KitDeInterface.Novo("Com guilda", painel);
            KitDeInterface.Esticar(com);
            comGuilda = com.gameObject;
            titulo = Rotulo(com, "", 52, 76, 32);
            titulo.supportRichText = false;
            convite = Rotulo(com, "", 640, 80, 24, KitDeInterface.TextoClaro);
            Posicionar(KitDeInterface.Botao("Copiar", com, "Copiar", () => { GUIUtility.systemCopyBuffer = guilda.Vista.Codigo; aviso.text = "Código copiado."; }, 20), 900, 72, 110, 46);
            trocarCodigo = KitDeInterface.Botao("Trocar", com, "Trocar", () => Fazer(() => guilda.Pedir(AcaoDaGuilda.TrocarCodigo), "Código trocado: o antigo não vale mais."), 20);
            Posicionar(trocarCodigo, 1018, 72, 110, 46);

            chefe = Rotulo(com, "", 52, 136, 26);
            tempo = Rotulo(com, "", 700, 138, 22, KitDeInterface.TextoClaro);
            barra = KitDeInterface.NovaBarra("Vida do chefe", com, KitDeInterface.Vermelho);
            KitDeInterface.Ancorar(barra.Rect, new Vector2(0, 1), new Vector2(52, -178), new Vector2(1076, 34));
            vida = KitDeInterface.Texto("Vida", barra.Rect, 20, Color.white, TextAnchor.MiddleCenter, true);
            KitDeInterface.Esticar((RectTransform)vida.transform);

            lutar = KitDeInterface.Botao("Lutar", com, "Lutar", Lutar, 26);
            Posicionar(lutar, 52, 228, 520, 66);
            rotuloDeLutar = lutar.GetComponentInChildren<Text>();
            coletar = KitDeInterface.Botao("Coletar", com, "Coletar", Coletar, 26);
            Posicionar(coletar, 608, 228, 520, 66);
            rotuloDeColetar = coletar.GetComponentInChildren<Text>();

            int n = catalogo.Guilda.MaximoDeMembros, porColuna = Mathf.CeilToInt(n / (float)Colunas);
            fundos = new Image[n]; nomes = new Text[n]; pontos = new Text[n]; expulsar = new Button[n];
            for (int i = 0; i < n; i++) MontarMembro(com, i, 52 + i / porColuna * 548, 316 + i % porColuna * 44);

            var sair = KitDeInterface.Botao("Sair", com, "Sair da guilda", Sair, 20);
            KitDeInterface.Ancorar((RectTransform)sair.transform, new Vector2(1, 0), new Vector2(-52, 40), new Vector2(300, 50));
            rotuloDeSair = sair.GetComponentInChildren<Text>();

            aviso = KitDeInterface.Texto("Aviso", painel, 22, KitDeInterface.Dourado, TextAnchor.MiddleLeft);
            KitDeInterface.Ancorar((RectTransform)aviso.transform, new Vector2(0, 0), new Vector2(52, 44), new Vector2(760, 40));
            semGuilda.SetActive(false);
            comGuilda.SetActive(false);
        }

        static Text Rotulo(RectTransform pai, string texto, float x, float y, int tamanho, Color? cor = null)
        {
            var t = Texto(pai, x, y, tamanho, cor ?? KitDeInterface.Dourado, 560, true);
            t.text = texto;
            return t;
        }

        static InputField Campo(RectTransform pai, string dica, int limite, float x, float y)
        {
            var c = KitDeInterface.Campo(dica, pai, dica, false, limite);
            KitDeInterface.Ancorar((RectTransform)c.transform, new Vector2(0, 1), new Vector2(x, -y), new Vector2(620, 60));
            return c;
        }

        static void Posicionar(Button b, float x, float y, float largura, float altura) =>
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(0, 1), new Vector2(x, -y), new Vector2(largura, altura));

        void MontarMembro(RectTransform pai, int i, float x, float y)
        {
            fundos[i] = KitDeInterface.Painel($"Membro {i}", pai, Color.clear, false);
            fundos[i].raycastTarget = false;
            var linha = (RectTransform)fundos[i].transform;
            KitDeInterface.Ancorar(linha, new Vector2(0, 1), new Vector2(x, -y), new Vector2(528, 40));
            nomes[i] = Texto(linha, 12, 4, 21, KitDeInterface.TextoClaro, 290);
            nomes[i].supportRichText = false;   // o nome é escrito por outra pessoa
            pontos[i] = KitDeInterface.Texto("Pontos", linha, 21, KitDeInterface.Dourado, TextAnchor.MiddleRight, true);
            KitDeInterface.Ancorar((RectTransform)pontos[i].transform, new Vector2(1, 0.5f), new Vector2(-56, 0), new Vector2(200, 34));
            expulsar[i] = KitDeInterface.Botao("Expulsar", linha, "x", () => Expulsar(i), 18);
            KitDeInterface.Ancorar((RectTransform)expulsar[i].transform, new Vector2(1, 0.5f), new Vector2(-6, 0), new Vector2(40, 34));
        }

        // Pede o segundo clique para o que não tem volta
        bool Confirmar(string oque, string texto)
        {
            if (confirmando == oque && Time.unscaledTime < fimDaConfirmacao) { confirmando = null; return true; }
            confirmando = oque;
            fimDaConfirmacao = Time.unscaledTime + 5f;
            aviso.text = texto;
            return false;
        }

        void Expulsar(int i)
        {
            var v = guilda.Vista;
            if (v == null || i >= v.Membros.Count) return;
            var m = v.Membros[i];
            if (Confirmar(m.JogadorId, "Clique no x de novo para expulsar.")) Fazer(() => guilda.Pedir(AcaoDaGuilda.Expulsar, m.JogadorId), "Membro expulso.");
        }

        void Sair()
        {
            if (Confirmar("sair", "Colete a recompensa antes. Clique de novo para sair.")) Fazer(() => guilda.Pedir(AcaoDaGuilda.Sair), "Você saiu da guilda.");
        }

        void Coletar() => Fazer(async () =>
        {
            var r = await guilda.Pedir(AcaoDaGuilda.Coletar);
            if (faixa) faixa.Mostrar($"+{r.Cristais} cristais  +{r.Fragmentos} fragmentos", 2f);
        }, "Recompensa coletada.");

        void Lutar() => Fazer(async () =>
        {
            var d = await guilda.ComecarLuta();
            arena.LutarPelaGuilda(d);
        }, "");

        async void Fazer(Func<Task> acao, string sucesso)
        {
            if (fazendo) return;
            fazendo = true;
            aviso.text = "Aguarde...";
            string resultado;
            try { await acao(); resultado = sucesso; }
            catch (FalhaOnline e) { resultado = e.Message; }
            catch (Exception e) { Debug.LogException(e); resultado = "Algo deu errado."; }
            if (!this) return;   // a luta troca de cena
            fazendo = false;
            buscadoEm = Time.unscaledTime;
            aviso.text = resultado;
        }

        protected override void AoAbrir()
        {
            mostrada = null;
            if (!conta.Conectada) aviso.text = "Sem conexão: a guilda aparece quando você estiver online.";
        }

        protected override void AtualizarConteudo()
        {
            if (conta.Conectada && !fazendo && Time.unscaledTime - buscadoEm > Validade) Fazer(() => guilda.Pedir(AcaoDaGuilda.Ver), "");
            var v = guilda.Vista;
            if (v == null || v == mostrada) return;
            mostrada = v;
            semGuilda.SetActive(!v.TemGuilda);
            comGuilda.SetActive(v.TemGuilda);
            if (v.TemGuilda) Mostrar(v);
        }

        void Mostrar(VistaDaGuilda v)
        {
            var p = catalogo.Guilda;
            titulo.text = v.Nome;
            convite.text = $"Código: {v.Codigo}";
            trocarCodigo.gameObject.SetActive(v.SouLider);
            chefe.text = v.ChefeDerrotado ? $"{p.NomeDoChefe} derrotado!" : $"{p.NomeDoChefe}  ·  fase {v.FasesDerrubadas + 1} de {p.Fases}";
            var t = TimeSpan.FromSeconds(Math.Max(0, v.SegundosParaVirar));
            tempo.text = $"Novo chefe em {(t.Days > 0 ? $"{t.Days} d {t.Hours} h" : $"{t.Hours} h {t.Minutes:00} min")}";
            double resta = Math.Max(0, v.VidaDaFase - v.PontosNaFase);
            barra.Definir((float)(resta / Math.Max(1, v.VidaDaFase)));
            vida.text = v.ChefeDerrotado ? "" : $"{FormatadorDeNumeros.Formatar(resta)} / {FormatadorDeNumeros.Formatar(v.VidaDaFase)}";
            lutar.interactable = v.LutasRestantes > 0 && !v.ChefeDerrotado;
            rotuloDeLutar.text = v.ChefeDerrotado ? "Até a próxima semana" : v.LutasRestantes > 0 ? $"Lutar ({v.LutasRestantes} hoje)" : "Sem lutas hoje";
            rotuloDeLutar.color = lutar.interactable ? KitDeInterface.Dourado : KitDeInterface.DouradoEscuro;
            coletar.interactable = v.FasesAColetar > 0;
            rotuloDeColetar.text = v.FasesAColetar > 0 ? $"Coletar {v.FasesAColetar * p.CristaisPorFase} cristais e {v.FasesAColetar * p.FragmentosPorFase} fragmentos" : "Nada para coletar";
            rotuloDeColetar.fontSize = 22;
            rotuloDeColetar.color = coletar.interactable ? KitDeInterface.Verde : KitDeInterface.DouradoEscuro;
            rotuloDeSair.text = "Sair da guilda";

            for (int i = 0; i < fundos.Length; i++)
            {
                bool ha = i < v.Membros.Count;
                fundos[i].gameObject.SetActive(ha);
                if (!ha) continue;
                var m = v.Membros[i];
                bool eu = m.JogadorId == conta.JogadorId;
                fundos[i].color = eu ? new Color(1f, 0.8f, 0.3f, 0.18f) : new Color(0, 0, 0, i % 2 == 0 ? 0.28f : 0.14f);
                nomes[i].text = (string.IsNullOrEmpty(m.Nome) ? "Sem nome" : m.Nome) + (m.Lider ? " (líder)" : "");
                pontos[i].text = m.Lutas > 0 ? FormatadorDeNumeros.Formatar(m.Pontos) : "não lutou";
                expulsar[i].gameObject.SetActive(v.SouLider && !eu);
            }
        }
    }
}
