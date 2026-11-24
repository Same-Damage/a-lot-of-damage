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
        // Com arte pintada em Resources/Interface/<nome>.png, ela é usada no lugar da gerada por código
        public static Sprite Painel => Fatiado(ref spritePainel, "Painel", () => TexturaDoPainel, Fatia);
        public static Sprite Botao => Fatiado(ref spriteBotao, "Botao", () => TexturaDoBotao, FatiaPequena);
        public static Sprite BotaoHover => Fatiado(ref spriteBotaoHover, "BotaoHover", () => TexturaDoBotaoHover, FatiaPequena);
        public static Sprite BotaoPressionado => Fatiado(ref spriteBotaoPressionado, "BotaoPressionado", () => TexturaDoBotaoPressionado, FatiaPequena);
        public static Sprite FundoDaBarra => Fatiado(ref spriteFundoDaBarra, "FundoDaBarra", () => TexturaDoFundoDaBarra, FatiaPequena);

        // Parte da arte pintada que fica fixa nas bordas ao esticar (9-slice): os cantos ornamentados
        public const float ProporcaoDaFatiaDaArte = 0.12f;

        public static Texture2D ArtePintada(string nome) => Resources.Load<Texture2D>("Interface/" + nome);

        static Sprite Fatiado(ref Sprite cache, string nome, System.Func<Texture2D> gerada, int fatia)
        {
            if (cache) return cache;
            var t = ArtePintada(nome);
            if (t) fatia = Mathf.RoundToInt(Mathf.Min(t.width, t.height) * ProporcaoDaFatiaDaArte);
            else t = gerada();
            cache = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100, 0,
                SpriteMeshType.FullRect, new Vector4(fatia, fatia, fatia, fatia));
            cache.hideFlags = HideFlags.DontSave;
            return cache;
        }

        // Peças leves do HUD, sobre o cenário: placa translúcida de cantos redondos com filete e disco dos botões de ícone
        const int RaioDaPlaca = 18;
        static Texture2D placa, disco, discoClaro;
        static Sprite spritePlaca, spriteDisco, spriteDiscoClaro;

        public static Texture2D TexturaDaPlaca => placa ??= CriarArredondado(64, RaioDaPlaca, new Color(0.05f, 0.045f, 0.06f, 0.72f), new Color(0.85f, 0.66f, 0.3f, 0.5f), 1.5f);
        public static Texture2D TexturaDoDisco => disco ??= CriarArredondado(128, 64, new Color(0.06f, 0.05f, 0.06f, 0.74f), new Color(0.85f, 0.66f, 0.3f, 0.9f), 4f);
        public static Texture2D TexturaDoDiscoClaro => discoClaro ??= CriarArredondado(128, 64, new Color(0.3f, 0.21f, 0.1f, 0.9f), new Color(1f, 0.86f, 0.5f, 1f), 5f);

        public static Sprite Placa => spritePlaca ? spritePlaca : spritePlaca = Sprite.Create(TexturaDaPlaca, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.FullRect, Vector4.one * (RaioDaPlaca + 2));
        public static Sprite Disco => spriteDisco ? spriteDisco : spriteDisco = Inteiro(TexturaDoDisco);
        public static Sprite DiscoClaro => spriteDiscoClaro ? spriteDiscoClaro : spriteDiscoClaro = Inteiro(TexturaDoDiscoClaro);

        // Círculo branco liso (selos e preenchimento da ultimate), tingido pela cor da imagem
        static Sprite circulo;
        public static Sprite Circulo => circulo ? circulo : circulo = Inteiro(CriarArredondado(128, 64, Color.white, Color.white, 1f));

        static Sprite Inteiro(Texture2D t) => Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100);

        static Texture2D CriarArredondado(int tamanho, float raio, Color miolo, Color filete, float espessura)
        {
            var t = Novo(tamanho, tamanho);
            var px = new Color[tamanho * tamanho];
            float meio = tamanho * 0.5f;
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                float d = DesenhoDeIcone.Caixa(x + 0.5f - meio, y + 0.5f - meio, meio - raio, meio - raio) - raio + 1;
                var c = Color.Lerp(miolo, filete, Mathf.Clamp01(d + espessura + 0.5f));
                c.a *= Mathf.Clamp01(0.5f - d);
                px[y * tamanho + x] = c;
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        static readonly System.Collections.Generic.Dictionary<string, Sprite> artesInteiras = new();

        // Arte pintada usada inteira, sem esticar as bordas (molduras das casas, faixa do Poder); null se ainda não existe
        public static Sprite Arte(string nome)
        {
            if (artesInteiras.TryGetValue(nome, out var s)) return s;
            var t = ArtePintada(nome);
            s = t ? Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100) : null;
            artesInteiras[nome] = s;
            return s;
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
