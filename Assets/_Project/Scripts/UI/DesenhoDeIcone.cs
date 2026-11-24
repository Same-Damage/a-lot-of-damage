using System;
using UnityEngine;

namespace ALotOfDamage.UI
{
    // Desenha ícones por código a partir de uma função de distância com sinal (negativa dentro da forma),
    // em coordenadas de -1 a 1, com y para cima
    public static class DesenhoDeIcone
    {
        public static Sprite Criar(int tamanho, Color cor, Func<float, float, float> distancia)
        {
            var t = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, true) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[tamanho * tamanho];
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                float u = (x + 0.5f) / tamanho * 2 - 1, v = (y + 0.5f) / tamanho * 2 - 1;
                float a = Mathf.Clamp01(0.5f - distancia(u, v) * tamanho * 0.5f);
                // sombreado simples: mais claro em cima, para parecer volume
                var c = Color.Lerp(cor * 0.55f, Color.Lerp(cor, Color.white, 0.35f), (v + 1) * 0.5f);
                c.a = a;
                px[y * tamanho + x] = c;
            }
            t.SetPixels32(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), 100);
        }

        public static float Circulo(float u, float v, float raio) => Mathf.Sqrt(u * u + v * v) - raio;

        public static float Caixa(float u, float v, float meiaLargura, float meiaAltura)
        {
            float dx = Mathf.Abs(u) - meiaLargura, dy = Mathf.Abs(v) - meiaAltura;
            return Mathf.Max(dx, dy) > 0 ? new Vector2(Mathf.Max(dx, 0), Mathf.Max(dy, 0)).magnitude : Mathf.Max(dx, dy);
        }

        public static float Segmento(float u, float v, float ax, float ay, float bx, float by)
        {
            var p = new Vector2(u - ax, v - ay);
            var ab = new Vector2(bx - ax, by - ay);
            float t = Mathf.Clamp01(Vector2.Dot(p, ab) / ab.sqrMagnitude);
            return (p - ab * t).magnitude;
        }
    }
}
