using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // História: quando há uma cena nova (ProximaCena), mostra as falas uma a uma numa caixa embaixo da tela,
    // com o nome de quem fala; avança com clique, Enter ou o botão A do controle, e marca a cena como vista no fim
    public class CaixaDeDialogo : MonoBehaviour
    {
        ProximaCena proxima;
        MarcarCenaVista marcar;

        RectTransform caixa;
        Text nome, texto, dica;
        DefinicaoDeCena cena;
        int fala;
        float proximaConsulta;

        [Inject]
        public void Construir(ProximaCena proxima, MarcarCenaVista marcar)
        {
            this.proxima = proxima;
            this.marcar = marcar;
        }

        void Start()
        {
            var canvas = KitDeInterface.Canvas();
            var painel = KitDeInterface.Painel("Diálogo", canvas);
            caixa = (RectTransform)painel.transform;
            KitDeInterface.Ancorar(caixa, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1180, 230));
            painel.gameObject.AddComponent<Button>().onClick.AddListener(Avancar);

            nome = KitDeInterface.Texto("Quem", caixa, 30, KitDeInterface.Dourado, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)nome.transform, new Vector2(0, 1), new Vector2(48, -22), new Vector2(1080, 40));
            texto = KitDeInterface.Texto("Fala", caixa, 26, KitDeInterface.TextoClaro, TextAnchor.UpperLeft);
            texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            KitDeInterface.Ancorar((RectTransform)texto.transform, new Vector2(0, 1), new Vector2(48, -72), new Vector2(1080, 110));
            dica = KitDeInterface.Texto("Dica", caixa, 18, KitDeInterface.DouradoEscuro, TextAnchor.LowerRight);
            KitDeInterface.Ancorar((RectTransform)dica.transform, new Vector2(1, 0), new Vector2(-40, 18), new Vector2(500, 26));
            caixa.gameObject.SetActive(false);
        }

        void Update()
        {
            if (cena == null)
            {
                if (proxima == null || Time.unscaledTime < proximaConsulta) return;
                proximaConsulta = Time.unscaledTime + 1f;
                cena = proxima.Executar();
                if (cena == null || cena.Falas.Count == 0) { cena = null; return; }
                fala = 0;
                caixa.gameObject.SetActive(true);
                caixa.SetAsLastSibling();
                Mostrar();
                return;
            }

            var k = KitDeInterface.Teclado;
            bool avancar = (k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame))
                || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (avancar) Avancar();
        }

        void Mostrar()
        {
            var f = cena.Falas[fala];
            nome.text = f.Quem;
            texto.text = f.Texto;
            dica.text = fala < cena.Falas.Count - 1 ? "▶ clique ou Enter" : "▶ clique ou Enter para fechar";
        }

        void Avancar()
        {
            if (cena == null) return;
            fala++;
            if (fala < cena.Falas.Count) { Mostrar(); return; }
            marcar.Executar(cena.Id);
            cena = null;
            caixa.gameObject.SetActive(false);
            proximaConsulta = Time.unscaledTime + 0.5f;
        }
    }
}
