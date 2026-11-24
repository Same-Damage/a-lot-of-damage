using UnityEngine;
using UnityEngine.UI;

namespace ALotOfDamage.UI
{
    // Faixa de destaque no alto da tela ("VITÓRIA!", "MARCO!", nome da ultimate), some sozinha
    public class Faixa : MonoBehaviour
    {
        Text texto;
        Image fundo;
        float ate;

        const float AlturaMinima = 80;

        // Uma faixa por canvas, compartilhada por quem precisar
        public static Faixa DoCanvas(Transform canvas, float altura = 0.78f)
        {
            var existente = canvas.Find("Faixa");
            if (existente) return existente.GetComponent<Faixa>();
            var img = KitDeInterface.Placa("Faixa", canvas).GetComponent<Image>();
            KitDeInterface.Ancorar((RectTransform)img.transform, new Vector2(0.5f, altura), Vector2.zero, new Vector2(760, AlturaMinima));
            var f = img.gameObject.AddComponent<Faixa>();
            f.fundo = img;
            f.texto = KitDeInterface.Texto("Texto", img.transform, 34, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            f.texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            KitDeInterface.Esticar((RectTransform)f.texto.transform);
            img.gameObject.SetActive(false);
            return f;
        }

        public void Mostrar(string mensagem, float segundos = 1.8f)
        {
            texto.text = mensagem;
            // mensagens de várias linhas (vitória com item) crescem a placa
            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Max(AlturaMinima, texto.preferredHeight + 32));
            ate = Time.unscaledTime + segundos;
            gameObject.SetActive(true);
        }

        // Mantém visível até alguém esconder (ex.: vitória até a troca de cena)
        public void Fixar(string mensagem) => Mostrar(mensagem, float.PositiveInfinity);

        void Update()
        {
            float resta = ate - Time.unscaledTime;
            if (resta <= 0) { gameObject.SetActive(false); return; }
            float alfa = Mathf.Clamp01(resta / 0.3f);
            texto.canvasRenderer.SetAlpha(alfa);
            fundo.canvasRenderer.SetAlpha(alfa);
        }
    }
}
