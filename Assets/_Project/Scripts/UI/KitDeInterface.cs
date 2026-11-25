using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ALotOfDamage.UI
{
    // Peças da interface montadas por código: painel escuro com moldura dourada, textos com sombra,
    // botões e barras. Direção de arte inspirada no Legend Online (ver CLAUDE.md).
    public static class KitDeInterface
    {
        public static readonly Color Fundo = new Color(0.07f, 0.06f, 0.05f, 0.92f);
        public static readonly Color Dourado = new Color(1f, 0.8f, 0.32f);
        public static readonly Color DouradoClaro = new Color(0.95f, 0.85f, 0.6f);
        public static readonly Color DouradoEscuro = new Color(0.55f, 0.4f, 0.15f);
        public static readonly Color TextoClaro = new Color(0.95f, 0.92f, 0.85f);
        public static readonly Color Verde = new Color(0.45f, 1f, 0.5f);
        public static readonly Color Vermelho = new Color(0.85f, 0.2f, 0.25f);
        public static readonly Color Azul = new Color(0.4f, 0.8f, 1f);

        static Font fonte;
        // Fonte serifada do sistema, no clima medieval; sem nenhuma delas, a fonte padrão da Unity
        public static Font Fonte => fonte ? fonte : fonte = FonteSerifada() ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        static Font FonteSerifada()
        {
            var instaladas = Font.GetOSInstalledFontNames();
            foreach (var nome in new[] { "Georgia", "Palatino Linotype", "Book Antiqua", "Times New Roman", "Noto Serif" })
                if (Array.IndexOf(instaladas, nome) >= 0) return Font.CreateDynamicFontFromOSFont(nome, 32);
            return null;
        }

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
            if (moldura)
            {
                // moldura dourada com cravos; a cor só tinge o sprite, então o padrão é branco
                img.sprite = TemaVisual.Painel;
                img.type = Image.Type.Sliced;
                img.color = cor ?? Color.white;
            }
            else img.color = cor ?? Fundo;
            return img;
        }

        // Placa leve do HUD: translúcida, sem moldura, para não competir com o cenário
        public static RectTransform Placa(string nome, Transform pai)
        {
            var img = Painel(nome, pai, Color.white, false);
            img.sprite = TemaVisual.Placa;
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return (RectTransform)img.transform;
        }

        public static Image Icone(string nome, Transform pai, IconeDoHud icone)
        {
            var img = Painel(nome, pai, Color.white, false);
            img.sprite = IconesDoHud.Do(icone);
            img.raycastTarget = false;
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
            var img = Painel(nome, pai, Color.white, false);
            img.sprite = TemaVisual.Botao;
            img.type = Image.Type.Sliced;
            var botao = img.gameObject.AddComponent<Button>();
            botao.transition = Selectable.Transition.SpriteSwap;
            botao.spriteState = new SpriteState
            {
                highlightedSprite = TemaVisual.BotaoHover,
                selectedSprite = TemaVisual.Botao,
                pressedSprite = TemaVisual.BotaoPressionado,
            };
            botao.onClick.AddListener(() => aoClicar());

            var t = Texto("Rotulo", img.transform, tamanhoDaFonte, Dourado, TextAnchor.MiddleCenter, true);
            Esticar((RectTransform)t.transform);
            t.text = rotulo;
            return botao;
        }

        // Teclado do jogo: nulo enquanto o jogador digita num campo ou uma tela da nuvem está por cima
        public static Keyboard Teclado
        {
            get
            {
                if (SincroniaDaNuvem.Bloqueando) return null;
                var atual = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
                return atual && atual.TryGetComponent<InputField>(out var campo) && campo.isFocused ? null : Keyboard.current;
            }
        }

        public static InputField Campo(string nome, Transform pai, string dica, bool secreto, int limite)
        {
            var img = Painel(nome, pai, Color.white, false);
            img.sprite = TemaVisual.FundoDaBarra;
            img.type = Image.Type.Sliced;
            var campo = img.gameObject.AddComponent<InputField>();
            Text Dentro(string n, Color cor)
            {
                var t = Texto(n, img.transform, 24, cor);
                t.supportRichText = false;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Truncate;
                var rt = (RectTransform)t.transform;
                Esticar(rt);
                rt.offsetMin = new Vector2(16, 4);
                rt.offsetMax = new Vector2(-16, -4);
                return t;
            }
            campo.textComponent = Dentro("Digitado", TextoClaro);
            campo.placeholder = Dentro("Dica", new Color(0.7f, 0.67f, 0.6f));
            ((Text)campo.placeholder).text = dica;
            campo.characterLimit = limite;
            campo.contentType = secreto ? InputField.ContentType.Password : InputField.ContentType.Standard;
            campo.navigation = new Navigation { mode = Navigation.Mode.None };
            return campo;
        }

        public static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public static Barra NovaBarra(string nome, Transform pai, Color cor)
        {
            var fundo = Painel(nome, pai, Color.white, false);
            fundo.sprite = TemaVisual.FundoDaBarra;
            fundo.type = Image.Type.Sliced;
            var cheio = Painel("Cheio", fundo.transform, cor, false);
            var rt = (RectTransform)cheio.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(0, 1);
            // o preenchimento fica dentro do filete da moldura
            rt.offsetMin = new Vector2(3, 3);
            rt.offsetMax = new Vector2(-3, -3);
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
