using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Base das janelas do mapa (upgrades, forja, bestiário) no kit de interface.
    // Cada janela tem um ícone no MenuDoMapa e uma tecla; só uma fica aberta por vez (Esc fecha).
    public abstract class JanelaDoJogo : MonoBehaviour
    {
        const float Largura = 1000;
        const float IntervaloDeAtualizacao = 0.15f;

        static JanelaDoJogo aberta;

        protected IRepositorioDeProgresso repositorio;
        protected ICatalogo catalogo;
        protected Progresso Progresso => repositorio.Carregar();
        protected Faixa faixa;

        RectTransform painel;
        Text cabecalho;
        BotaoDeIcone botao;
        float proximaAtualizacao;

        protected abstract string Titulo { get; }
        protected abstract Key Tecla { get; }
        protected abstract int Ordem { get; }   // posição no MenuDoMapa (as primeiras ficam sempre à vista)
        protected abstract IconeDoHud Icone { get; }
        protected virtual TipoDePendencia? Pendencia => null;   // o que acende o selo do ícone
        protected abstract int Linhas { get; }
        protected abstract float AlturaDaLinha { get; }
        protected abstract void MontarLinha(int i, RectTransform linha);
        protected abstract void AtualizarLinha(int i);

        // Janelas com layout próprio (como o inventário) trocam o tamanho e o conteúdo em vez de usar linhas
        protected virtual Vector2 Tamanho => new Vector2(Largura, 90 + Linhas * AlturaDaLinha);

        protected virtual void MontarConteudo(RectTransform painel)
        {
            for (int i = 0; i < Linhas; i++)
            {
                var linha = KitDeInterface.Novo($"Linha {i}", painel);
                KitDeInterface.Ancorar(linha, new Vector2(0, 1), new Vector2(0, -80 - i * AlturaDaLinha), new Vector2(Largura, AlturaDaLinha));
                MontarLinha(i, linha);
            }
        }

        protected virtual void AtualizarConteudo()
        {
            for (int i = 0; i < Linhas; i++) AtualizarLinha(i);
        }

        public bool Aberta => aberta == this;
        public static bool AlgumaAberta => aberta;

        [Inject]
        public void ConstruirJanela(IRepositorioDeProgresso repositorio, ICatalogo catalogo)
        {
            this.repositorio = repositorio;
            this.catalogo = catalogo;
        }

        protected virtual void Start()
        {
            var canvas = KitDeInterface.Canvas();
            faixa = Faixa.DoCanvas(canvas);

            botao = MenuDoMapa.DoCanvas(canvas).Adicionar(Ordem, Icone, Titulo, Pendencia, Alternar);

            var tamanho = Tamanho;
            painel = (RectTransform)KitDeInterface.Painel($"Janela {Titulo}", canvas).transform;
            KitDeInterface.Ancorar(painel, new Vector2(0.5f, 0.5f), new Vector2(-60, 0), tamanho);

            cabecalho = KitDeInterface.Texto("Cabecalho", painel, 34, KitDeInterface.Dourado, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)cabecalho.transform, new Vector2(0, 1), new Vector2(52, -10), new Vector2(tamanho.x - 130, 50));

            var fechar = KitDeInterface.Botao("Fechar", painel, "X", Alternar, 26);
            KitDeInterface.Ancorar((RectTransform)fechar.transform, new Vector2(1, 1), new Vector2(-14, -14), new Vector2(50, 50));

            MontarConteudo(painel);
            painel.gameObject.SetActive(false);
        }

        // Abre a janela fechada ou fecha a aberta (botão, tecla e testes)
        public void Alternar()
        {
            aberta = Aberta ? null : this;
            proximaAtualizacao = 0;
        }

        void OnDisable()
        {
            if (Aberta) aberta = null;
        }

        protected virtual void Update()
        {
            var k = Keyboard.current;
            if (k != null && (k[Tecla].wasPressedThisFrame || (Aberta && k.escapeKey.wasPressedThisFrame))) Alternar();

            if (painel.gameObject.activeSelf != Aberta)
            {
                painel.gameObject.SetActive(Aberta);
                // por cima do HUD e das outras janelas
                if (Aberta) painel.SetAsLastSibling();
                botao.Destacar(Aberta);
            }
            if (!Aberta || Time.unscaledTime < proximaAtualizacao) return;

            proximaAtualizacao = Time.unscaledTime + IntervaloDeAtualizacao;
            cabecalho.text = $"{Titulo} [{Tecla}]      Ouro {FormatadorDeNumeros.Formatar(Progresso.Ouro)}";
            AtualizarConteudo();
        }

        // Atalhos para as linhas
        protected static Text Texto(RectTransform linha, float x, float y, int tamanho, Color cor, float largura = 560, bool negrito = false)
        {
            var t = KitDeInterface.Texto("Texto", linha, tamanho, cor, TextAnchor.MiddleLeft, negrito);
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0, 1), new Vector2(x, -y), new Vector2(largura, tamanho + 10));
            return t;
        }

        protected static Button Botao(RectTransform linha, string rotulo, float xDaDireita, float largura, System.Action aoClicar)
        {
            var b = KitDeInterface.Botao(rotulo, linha, rotulo, aoClicar);
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(1, 1), new Vector2(-xDaDireita, -14), new Vector2(largura, 56));
            return b;
        }

        protected static Barra Barra(RectTransform linha, float x, float y, float largura, Color cor)
        {
            var b = KitDeInterface.NovaBarra("Barra", linha, cor);
            KitDeInterface.Ancorar(b.Rect, new Vector2(0, 1), new Vector2(x, -y), new Vector2(largura, 14));
            return b;
        }
    }
}
