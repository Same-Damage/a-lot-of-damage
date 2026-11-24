using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using static ALotOfDamage.UI.DesenhoDeIcone;

namespace ALotOfDamage.UI
{
    // Ícone de cada tipo de item. Com arte de verdade, ela fica em Resources/Icones/<id do tipo>.png e é usada no lugar;
    // até lá, uma silhueta simples por slot é desenhada por código, na cor da raridade.
    public static class IconesDosItens
    {
        const int Tamanho = 64;
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Do(string tipoId, SlotDeItem slot, Color cor)
        {
            if (cache.TryGetValue(tipoId, out var arte)) return arte;
            arte = Resources.Load<Sprite>("Icones/" + tipoId);
            if (!arte && Resources.Load<Texture2D>("Icones/" + tipoId) is Texture2D t)
                arte = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100);
            if (arte) { cache[tipoId] = arte; return arte; }

            string chave = $"{slot}#{ColorUtility.ToHtmlStringRGB(cor)}";
            if (cache.TryGetValue(chave, out var provisorio)) return provisorio;
            provisorio = DesenhoDeIcone.Criar(Tamanho, cor, (u, v) => Distancia(slot, u, v));
            cache[chave] = provisorio;
            return provisorio;
        }

        // Distância com sinal até a forma (negativa dentro)
        static float Distancia(SlotDeItem slot, float u, float v)
        {
            switch (slot)
            {
                case SlotDeItem.Arma: // lâmina na diagonal com guarda
                    return Mathf.Min(Segmento(u, v, -0.55f, -0.55f, 0.7f, 0.7f) - 0.1f, Segmento(u, v, -0.6f, -0.25f, -0.25f, -0.6f) - 0.08f);
                case SlotDeItem.Elmo: // cúpula com abertura
                    return Mathf.Max(Mathf.Sqrt(u * u + (v + 0.2f) * (v + 0.2f)) - 0.7f, -(v + 0.25f), -Caixa(u, v + 0.05f, 0.35f, 0.08f));
                case SlotDeItem.Peitoral: // tronco com ombros
                    return Mathf.Min(Caixa(u, v + 0.1f, 0.45f, 0.6f) - 0.05f, Caixa(u, v - 0.45f, 0.75f, 0.15f) - 0.05f);
                case SlotDeItem.Luvas: // palma e dedos
                    return Mathf.Min(Caixa(u, v + 0.2f, 0.4f, 0.4f) - 0.08f, Caixa(u, v - 0.4f, 0.4f, 0.25f) - 0.04f);
                case SlotDeItem.Botas: // cano e pé
                    return Mathf.Min(Caixa(u + 0.15f, v + 0.05f, 0.25f, 0.6f) - 0.05f, Caixa(u - 0.1f, v + 0.55f, 0.5f, 0.15f) - 0.05f);
                case SlotDeItem.Cinto: // faixa com fivela
                    return Mathf.Min(Caixa(u, v, 0.8f, 0.14f), Mathf.Abs(Caixa(u, v, 0.22f, 0.26f)) - 0.05f);
                case SlotDeItem.Anel: // aro com pedra
                    return Mathf.Min(Mathf.Abs(Mathf.Sqrt(u * u + (v + 0.15f) * (v + 0.15f)) - 0.45f) - 0.1f,
                        Mathf.Sqrt(u * u + (v - 0.4f) * (v - 0.4f)) - 0.2f);
                default: // colar: corrente em U com pingente
                    float corrente = v > 0.1f ? Mathf.Abs(Mathf.Abs(u) - 0.55f) - 0.05f : Mathf.Abs(Mathf.Sqrt(u * u + (v - 0.1f) * (v - 0.1f)) - 0.55f) - 0.05f;
                    return Mathf.Min(corrente, Mathf.Sqrt(u * u + (v + 0.55f) * (v + 0.55f)) - 0.2f);
            }
        }
    }
}
