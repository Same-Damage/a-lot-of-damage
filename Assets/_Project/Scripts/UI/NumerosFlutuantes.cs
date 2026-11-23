using UnityEngine;
using UnityEngine.UI;

namespace ALotOfDamage.UI
{
    // Números de dano que saltam e somem. Pool fixo: nada é criado ou destruído durante a luta.
    public class NumerosFlutuantes : MonoBehaviour
    {
        const int Capacidade = 40;
        const float Duracao = 0.9f;

        struct Numero
        {
            public Text texto;
            public RectTransform rt;
            public Vector2 inicio;
            public float desde;
            public float escala;
            public Color cor;
        }

        readonly Numero[] numeros = new Numero[Capacidade];
        int proximo;

        public static NumerosFlutuantes Criar(Transform canvas)
        {
            var raiz = KitDeInterface.Novo("Numeros de Dano", canvas);
            KitDeInterface.Esticar(raiz);
            var n = raiz.gameObject.AddComponent<NumerosFlutuantes>();
            for (int i = 0; i < Capacidade; i++)
            {
                var t = KitDeInterface.Texto("Numero", raiz, 40, Color.white, TextAnchor.MiddleCenter, true);
                var rt = (RectTransform)t.transform;
                KitDeInterface.Ancorar(rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 90));
                t.gameObject.SetActive(false);
                n.numeros[i] = new Numero { texto = t, rt = rt, desde = -10 };
            }
            return n;
        }

        // posicao: deslocamento a partir do centro da tela, em pixels de referência (1920x1080)
        public void Mostrar(string texto, Color cor, int tamanho, Vector2 posicao)
        {
            ref var n = ref numeros[proximo];
            proximo = (proximo + 1) % Capacidade;
            n.texto.text = texto;
            n.texto.fontSize = tamanho;
            n.cor = cor;
            n.inicio = posicao;
            n.desde = Time.unscaledTime;
            n.escala = 1.6f;
            n.texto.gameObject.SetActive(true);
        }

        void Update()
        {
            float agora = Time.unscaledTime;
            for (int i = 0; i < Capacidade; i++)
            {
                ref var n = ref numeros[i];
                if (!n.texto.gameObject.activeSelf) continue;
                float t = (agora - n.desde) / Duracao;
                if (t >= 1) { n.texto.gameObject.SetActive(false); continue; }

                // Salta grande e assenta, depois sobe e some
                float escala = Mathf.Lerp(n.escala, 1f, Mathf.Clamp01(t * 6));
                n.rt.localScale = new Vector3(escala, escala, 1);
                n.rt.anchoredPosition = n.inicio + new Vector2(0, t * 120);
                var c = n.cor;
                c.a = 1 - Mathf.Clamp01((t - 0.6f) / 0.4f);
                n.texto.color = c;
            }
        }
    }
}
