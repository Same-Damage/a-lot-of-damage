using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ALotOfDamage.UI
{
    // Peças da interface montadas por código: painel escuro com moldura dourada, textos com sombra,
    // botões e barras. Direção de arte inspirada no Legend Online (ver CLAUDE.md).
    public static class KitDeInterface
    {
        public static readonly Color Fundo = new Color(0.07f, 0.06f, 0.05f, 0.92f);
        public static readonly Color FundoDoBotao = new Color(0.18f, 0.13f, 0.08f, 0.95f);
        public static readonly Color Dourado = new Color(1f, 0.8f, 0.32f);
        public static readonly Color DouradoEscuro = new Color(0.55f, 0.4f, 0.15f);
        public static readonly Color TextoClaro = new Color(0.95f, 0.92f, 0.85f);
        public static readonly Color Verde = new Color(0.45f, 1f, 0.5f);
        public static readonly Color Vermelho = new Color(0.85f, 0.2f, 0.25f);
        public static readonly Color Azul = new Color(0.4f, 0.8f, 1f);

        static Font fonte;
        public static Font Fonte => fonte ? fonte : fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Canvas único da cena para a interface do jogo (separado do canvas dos controles de toque)
        public static RectTransform Canvas()
        {
            var existente = GameObject.Find("Interface");
            if (existente) return (RectTransform)existente.transform;

            var go = new GameObject("Interface", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var escalador = go.AddComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920, 1080);
            escalador.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();

            if (!UnityEngine.Object.FindAnyObjectByType<EventSystem>())
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            return (RectTransform)go.transform;
        }

        // Posiciona pelo canto/ponto de âncora (0..1); pivot igual à âncora
        public static RectTransform Ancorar(RectTransform rt, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = ancora;
            rt.anchoredPosition = posicao;
            rt.sizeDelta = tamanho;
            return rt;
        }

        public static RectTransform Novo(string nome, Transform pai)
        {
            var go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);
            return (RectTransform)go.transform;
        }

        public static Image Painel(string nome, Transform pai, Color? cor = null, bool moldura = true)
        {
            var rt = Novo(nome, pai);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = cor ?? Fundo;
            if (moldura)
            {
                var o = rt.gameObject.AddComponent<Outline>();
                o.effectColor = Dourado;
                o.effectDistance = new Vector2(2, -2);
            }
            return img;
        }

        public static Text Texto(string nome, Transform pai, int tamanho, Color cor, TextAnchor alinhamento = TextAnchor.MiddleLeft, bool negrito = false)
        {
            var rt = Novo(nome, pai);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Fonte;
            t.fontSize = tamanho;
            t.color = cor;
            t.alignment = alinhamento;
            t.fontStyle = negrito ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var sombra = rt.gameObject.AddComponent<Shadow>();
            sombra.effectColor = new Color(0, 0, 0, 0.8f);
            sombra.effectDistance = new Vector2(2, -2);
            return t;
        }

        public static Button Botao(string nome, Transform pai, string rotulo, Action aoClicar, int tamanhoDaFonte = 22)
        {
            var img = Painel(nome, pai, FundoDoBotao);
            var botao = img.gameObject.AddComponent<Button>();
            var cores = botao.colors;
            cores.highlightedColor = new Color(1.3f, 1.2f, 1f);
            cores.pressedColor = new Color(0.7f, 0.6f, 0.5f);
            botao.colors = cores;
            botao.onClick.AddListener(() => aoClicar());

            var t = Texto("Rotulo", img.transform, tamanhoDaFonte, Dourado, TextAnchor.MiddleCenter, true);
            Esticar((RectTransform)t.transform);
            t.text = rotulo;
            return botao;
        }

        public static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public static Barra NovaBarra(string nome, Transform pai, Color cor)
        {
            var fundo = Painel(nome, pai, new Color(0, 0, 0, 0.6f));
            var cheio = Painel("Cheio", fundo.transform, cor, false);
            var rt = (RectTransform)cheio.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0, 1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return new Barra(fundo, rt);
        }
    }

    public class Barra
    {
        public readonly Image Fundo;
        readonly RectTransform cheio;
        float atual = -1;

        public Barra(Image fundo, RectTransform cheio)
        {
            Fundo = fundo;
            this.cheio = cheio;
        }

        public RectTransform Rect => (RectTransform)Fundo.transform;

        public void Definir(float fracao)
        {
            fracao = Mathf.Clamp01(fracao);
            if (Mathf.Approximately(fracao, atual)) return;
            atual = fracao;
            cheio.anchorMax = new Vector2(fracao, 1);
        }
    }
}
