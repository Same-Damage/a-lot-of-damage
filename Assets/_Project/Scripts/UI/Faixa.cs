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

        // Uma faixa por canvas, compartilhada por quem precisar
        public static Faixa DoCanvas(Transform canvas, float altura = 0.78f)
        {
            var existente = canvas.Find("Faixa");
            if (existente) return existente.GetComponent<Faixa>();
            var img = KitDeInterface.Painel("Faixa", canvas);
            KitDeInterface.Ancorar((RectTransform)img.transform, new Vector2(0.5f, altura), Vector2.zero, new Vector2(900, 110));
            var f = img.gameObject.AddComponent<Faixa>();
            f.fundo = img;
            f.texto = KitDeInterface.Texto("Texto", img.transform, 40, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            f.texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            KitDeInterface.Esticar((RectTransform)f.texto.transform);
            img.gameObject.SetActive(false);
            return f;
        }

        public void Mostrar(string mensagem, float segundos = 1.8f)
        {
            texto.text = mensagem;
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
