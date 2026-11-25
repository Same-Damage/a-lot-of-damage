using System.Collections.Generic;
using UnityEngine;
using static ALotOfDamage.UI.DesenhoDeIcone;

namespace ALotOfDamage.UI
{
    public enum IconeDoHud { Inventario, Upgrades, Forja, Followers, Conquistas, Bestiario, Ascensao, Reliquias, Taverna, Expedicoes, Menu, Heroi, Ouro, Espada, Ultimate, Fugir }

    // Ícones do HUD desenhados por código; arte de verdade em Resources/Icones/hud_<nome>.png é usada no lugar
    public static class IconesDoHud
    {
        const int Tamanho = 96;
        static readonly Dictionary<IconeDoHud, Sprite> cache = new Dictionary<IconeDoHud, Sprite>();

        public static Sprite Do(IconeDoHud icone)
        {
            if (cache.TryGetValue(icone, out var s) && s) return s;
            var t = Resources.Load<Texture2D>("Icones/hud_" + icone.ToString().ToLowerInvariant());
            s = t ? Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100)
                : DesenhoDeIcone.Criar(Tamanho, icone == IconeDoHud.Ouro ? KitDeInterface.Dourado : KitDeInterface.TextoClaro, (u, v) => Distancia(icone, u, v));
            cache[icone] = s;
            return s;
        }

        static float Min(float a, float b, float c) => Mathf.Min(a, Mathf.Min(b, c));

        static float Distancia(IconeDoHud icone, float u, float v)
        {
            switch (icone)
            {
                case IconeDoHud.Inventario: // mochila com alça e bolso
                {
                    float saco = Caixa(u, v + 0.2f, 0.42f, 0.4f) - 0.2f;
                    float alca = Mathf.Abs(Circulo(u, v - 0.42f, 0.26f)) - 0.08f;
                    float bolso = Mathf.Abs(Caixa(u, v + 0.35f, 0.24f, 0.14f) - 0.06f) - 0.05f;
                    return Mathf.Min(Mathf.Max(saco, -bolso), Mathf.Max(alca, -saco));
                }
                case IconeDoHud.Upgrades: // duas setas para cima
                {
                    float w = Mathf.Abs(u);
                    return Mathf.Min(Segmento(w, v, 0, 0.72f, 0.6f, 0.16f), Segmento(w, v, 0, 0.08f, 0.6f, -0.48f)) - 0.16f;
                }
                case IconeDoHud.Forja: // martelo
                    return Mathf.Min(Segmento(u, v, -0.6f, -0.7f, 0.2f, 0.25f) - 0.12f, Segmento(u, v, -0.12f, 0.6f, 0.58f, 0f) - 0.24f);
                case IconeDoHud.Followers: // pegada
                {
                    float w = Mathf.Abs(u);
                    return Min(Circulo(u, (v + 0.32f) * 1.15f, 0.42f), Circulo(w - 0.26f, v - 0.5f, 0.2f), Circulo(w - 0.64f, v - 0.12f, 0.18f));
                }
                case IconeDoHud.Conquistas: // taça
                {
                    float w = Mathf.Abs(u);
                    float copo = Mathf.Max(Circulo(u, (v - 0.55f) * 0.62f, 0.5f), v - 0.62f);
                    float asas = Mathf.Max(Mathf.Abs(Circulo(w - 0.52f, v - 0.3f, 0.22f)) - 0.07f, 0.45f - w);
                    float pe = Mathf.Min(Caixa(u, v + 0.4f, 0.1f, 0.25f), Caixa(u, v + 0.68f, 0.38f, 0.1f));
                    return Min(copo, asas, pe);
                }
                case IconeDoHud.Expedicoes: // bandeira no mastro
                {
                    float mastro = Caixa(u + 0.45f, v, 0.07f, 0.8f);
                    float pano = Mathf.Max(Caixa(u - 0.05f, v - 0.38f, 0.45f, 0.3f), (u - 0.1f) * 0.6f - Mathf.Abs(v - 0.38f) * 0.8f - 0.12f);
                    return Mathf.Min(mastro, pano);
                }
                case IconeDoHud.Taverna: // caneca com alça e espuma
                {
                    float copo = Caixa(u + 0.12f, v + 0.12f, 0.36f, 0.5f) - 0.08f;
                    float alca = Mathf.Max(Mathf.Abs(Circulo(u - 0.42f, v + 0.1f, 0.24f)) - 0.08f, 0.3f - u);
                    float espuma = Mathf.Min(Circulo(u + 0.3f, v - 0.5f, 0.2f), Circulo(u + 0.02f, v - 0.55f, 0.22f));
                    return Min(copo, alca, Mathf.Min(espuma, Circulo(u + 0.5f, v - 0.42f, 0.14f)));
                }
                case IconeDoHud.Reliquias: // amuleto: losango com pedra no meio e argola em cima
                {
                    float losango = (Mathf.Abs(u) + Mathf.Abs(v + 0.12f)) * 0.75f - 0.5f;
                    float pedra = Circulo(u, v + 0.12f, 0.2f);
                    float argola = Mathf.Abs(Circulo(u, v - 0.62f, 0.16f)) - 0.06f;
                    return Mathf.Min(Mathf.Max(losango, -pedra + 0.06f), Mathf.Min(pedra, argola));
                }
                case IconeDoHud.Bestiario: // livro com marcador
                {
                    float capa = Caixa(u, v, 0.5f, 0.64f) - 0.1f;
                    float lombada = Caixa(u + 0.3f, v, 0.04f, 0.8f);
                    float olho = Circulo(u - 0.16f, v - 0.1f, 0.2f);
                    return Mathf.Max(capa, Mathf.Max(-lombada, -olho));
                }
                case IconeDoHud.Ascensao: // estrela de quatro pontas
                {
                    float w = Mathf.Abs(u), h = Mathf.Abs(v);
                    return (Mathf.Pow(w, 0.6f) + Mathf.Pow(h, 0.6f) - Mathf.Pow(0.92f, 0.6f)) * 0.55f;
                }
                case IconeDoHud.Menu: // grade de quatro
                    return Caixa(Mathf.Abs(u) - 0.38f, Mathf.Abs(v) - 0.38f, 0.2f, 0.2f) - 0.06f;
                case IconeDoHud.Heroi: // cabeça e ombros
                    return Mathf.Min(Circulo(u, v - 0.34f, 0.34f), Mathf.Max(Circulo(u, v + 0.78f, 0.68f), -(v + 0.72f)));
                case IconeDoHud.Ouro: // moeda com miolo
                    return Mathf.Max(Circulo(u, v, 0.85f), -(Mathf.Abs(Circulo(u, v, 0.58f)) - 0.07f));
                case IconeDoHud.Espada:
                    return Min(Segmento(u, v, -0.25f, -0.25f, 0.7f, 0.7f) - 0.11f, Segmento(u, v, -0.5f, 0f, 0f, -0.5f) - 0.1f, Segmento(u, v, -0.3f, -0.3f, -0.68f, -0.68f) - 0.09f);
                case IconeDoHud.Ultimate: // raio
                    return Mathf.Min(Segmento(u, v, 0.22f, 0.78f, -0.28f, -0.02f), Mathf.Min(Segmento(u, v, -0.28f, -0.02f, 0.28f, 0.06f), Segmento(u, v, 0.28f, 0.06f, -0.18f, -0.78f))) - 0.13f;
                default: // fugir: seta de volta
                    return Mathf.Min(Segmento(u, v, -0.6f, 0, 0.65f, 0), Mathf.Min(Segmento(u, v, -0.6f, 0, -0.12f, 0.46f), Segmento(u, v, -0.6f, 0, -0.12f, -0.46f))) - 0.13f;
            }
        }
    }
}
