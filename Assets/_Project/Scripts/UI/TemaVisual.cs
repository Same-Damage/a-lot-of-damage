using UnityEngine;

namespace ALotOfDamage.UI
{
    // Sprites ornamentados do KitDeInterface, gerados por código (sem arte de terceiros):
    // moldura dourada chanfrada com sulco e cravos em losango nos cantos, fundo com grão de couro,
    // botões em bronze e fundo de barra. Cada sprite é criado uma vez só.
    public static class TemaVisual
    {
        static readonly Color OuroClaro = new Color(1f, 0.88f, 0.55f);
        static readonly Color OuroEscuro = new Color(0.45f, 0.32f, 0.12f);
        static readonly Color CorDoPainel = new Color(0.09f, 0.065f, 0.05f, 0.94f);

        const int Borda = 12;
        // Cravo em losango no canto das molduras (centro em pixels a partir da borda da textura)
        public const int CentroDoCravo = 8;
        const int RaioDoCravo = 7;
        // Fatia do 9-slice: cobre a moldura e o cravo, para os cantos nunca esticarem
        const int Fatia = CentroDoCravo + RaioDoCravo + 3;
        const int FatiaPequena = 6;

        static Texture2D painel, botao, botaoHover, botaoPressionado, fundoDaBarra;
        static Sprite spritePainel, spriteBotao, spriteBotaoHover, spriteBotaoPressionado, spriteFundoDaBarra;

        public static Texture2D TexturaDoPainel => painel ??= CriarMoldura(96, CorDoPainel, new Color(0.05f, 0.035f, 0.025f, 0.95f));
        public static Texture2D TexturaDoBotao => botao ??= CriarMoldura(32, new Color(0.42f, 0.28f, 0.12f), new Color(0.22f, 0.13f, 0.05f), 4, false);
        public static Texture2D TexturaDoBotaoHover => botaoHover ??= CriarMoldura(32, new Color(0.55f, 0.38f, 0.16f), new Color(0.3f, 0.18f, 0.07f), 4, false);
        public static Texture2D TexturaDoBotaoPressionado => botaoPressionado ??= CriarMoldura(32, new Color(0.16f, 0.1f, 0.04f), new Color(0.25f, 0.16f, 0.06f), 4, false);
        public static Texture2D TexturaDoFundoDaBarra => fundoDaBarra ??= CriarMoldura(32, new Color(0.05f, 0.04f, 0.03f, 0.95f), new Color(0.1f, 0.07f, 0.05f, 0.95f), 4, false);

        // Sprites fatiados para Image.Type.Sliced
        public static Sprite Painel => Fatiado(ref spritePainel, TexturaDoPainel, Fatia);
        public static Sprite Botao => Fatiado(ref spriteBotao, TexturaDoBotao, FatiaPequena);
        public static Sprite BotaoHover => Fatiado(ref spriteBotaoHover, TexturaDoBotaoHover, FatiaPequena);
        public static Sprite BotaoPressionado => Fatiado(ref spriteBotaoPressionado, TexturaDoBotaoPressionado, FatiaPequena);
        public static Sprite FundoDaBarra => Fatiado(ref spriteFundoDaBarra, TexturaDoFundoDaBarra, FatiaPequena);

        static Sprite Fatiado(ref Sprite cache, Texture2D t, int fatia)
        {
            if (cache) return cache;
            cache = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100, 0,
                SpriteMeshType.FullRect, new Vector4(fatia, fatia, fatia, fatia));
            cache.hideFlags = HideFlags.DontSave;
            return cache;
        }

        static Texture2D Novo(int l, int a)
        {
            return new Texture2D(l, a, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
        }

        // Ruído de valor por hash: sempre o mesmo resultado para o mesmo pixel, sem usar Random
        public static float Ruido(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }

        // Ruído em duas escalas, para parecer couro ou metal batido
        static float Grao(int x, int y) => Ruido(x, y) * 0.5f + Ruido(x / 3, y / 3) * 0.5f;

        static Color Metal(float brilho, int x, int y)
        {
            var c = Color.Lerp(OuroEscuro, OuroClaro, Mathf.Clamp01(brilho));
            float g = 0.88f + Grao(x, y) * 0.24f;
            return new Color(c.r * g, c.g * g, c.b * g);
        }

        // Moldura de 9 fatias: linha escura por fora, faixa dourada chanfrada com um sulco gravado,
        // filete dourado por dentro, cravos em losango nos cantos e o miolo com grão de couro e sombra nas bordas.
        static Texture2D CriarMoldura(int tamanho, Color miolo, Color mioloBaixo, int borda = Borda, bool cravos = true)
        {
            var t = Novo(tamanho, tamanho);
            var px = new Color[tamanho * tamanho];
            float faixa = borda - 4;
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                int dx = Mathf.Min(x, tamanho - 1 - x), dy = Mathf.Min(y, tamanho - 1 - y);
                int d = Mathf.Min(dx, dy);
                Color c;
                if (d == 0) c = new Color(0.04f, 0.02f, 0.01f);
                else if (d < borda - 3)
                {
                    float pos = (d - 1) / Mathf.Max(1f, faixa - 1);
                    // chanfro: claro do lado de fora, escuro do lado de dentro, com sulco no meio
                    float brilho = Mathf.Lerp(1f, 0.35f, pos);
                    if (borda > 8 && Mathf.Abs(pos - 0.5f) < 0.12f) brilho *= 0.45f;
                    if (y > tamanho / 2) brilho += 0.1f;
                    c = Metal(brilho, x, y);
                }
                else if (d < borda - 1) c = new Color(0.06f, 0.035f, 0.015f);
                else if (d == borda - 1) c = Metal(0.55f, x, y);
                else
                {
                    var b = Color.Lerp(mioloBaixo, miolo, (float)y / tamanho);
                    float grao = 0.82f + Grao(x, y) * 0.3f;
                    // sombra perto da moldura, como um painel afundado
                    float sombra = Mathf.Clamp01((d - borda) / 10f) * 0.35f + 0.65f;
                    c = new Color(b.r * grao * sombra, b.g * grao * sombra, b.b * grao * sombra, b.a);
                }

                if (cravos && dx < Fatia && dy < Fatia)
                {
                    int l = Mathf.Abs(dx - CentroDoCravo) + Mathf.Abs(dy - CentroDoCravo);
                    if (l <= RaioDoCravo)
                    {
                        float luz = 1f - (float)l / RaioDoCravo;
                        c = l == RaioDoCravo ? new Color(0.08f, 0.04f, 0.01f) : Metal(0.55f + luz * 0.5f, x, y);
                    }
                }
                px[y * tamanho + x] = c;
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }
    }
}
