using UnityEngine;

namespace ALotOfDamage.UI
{
    // Kit visual das telas provisórias em OnGUI: painéis escuros com moldura dourada,
    // cabeçalho vermelho, botões dourados, orbes e números com contorno.
    // As texturas são geradas por código (sem arte de terceiros) e criadas uma vez só.
    public static class TemaVisual
    {
        public static readonly Color Ouro = new Color(0.86f, 0.68f, 0.32f);
        public static readonly Color OuroClaro = new Color(1f, 0.88f, 0.55f);
        public static readonly Color OuroEscuro = new Color(0.45f, 0.32f, 0.12f);
        public static readonly Color CorDoPainel = new Color(0.09f, 0.065f, 0.05f, 0.94f);
        public static readonly Color VermelhoEscuro = new Color(0.45f, 0.08f, 0.07f);
        public static readonly Color Vida = new Color(0.8f, 0.12f, 0.1f);
        public static readonly Color Ciano = new Color(0.35f, 0.85f, 1f);
        public static readonly Color Verde = new Color(0.45f, 1f, 0.45f);
        public static readonly Color Laranja = new Color(1f, 0.55f, 0.12f);
        public static readonly Color CorDoTexto = new Color(0.93f, 0.89f, 0.8f);

        const int Borda = 12;

        static Texture2D slot, suporte, painel, cabecalho, botao, botaoHover, botaoPressionado, orbe, anel, fundoDaBarra, branca;
        static Font serifada;
        static GUIStyle estiloDoSlot, titulo, texto, botaoEstilo, numero, estiloDoPainel, estiloDoCabecalho, estiloDaBarra;
        static readonly GUIContent conteudo = new GUIContent();

        // ---------- texturas ----------

        public static Texture2D TexturaDoPainel => painel ??= CriarMoldura(96, CorDoPainel, new Color(0.05f, 0.035f, 0.025f, 0.95f));
        public static Texture2D TexturaDoCabecalho => cabecalho ??= CriarMoldura(96, new Color(0.6f, 0.12f, 0.08f), VermelhoEscuro);
        public static Texture2D TexturaDoBotao => botao ??= CriarMoldura(32, new Color(0.42f, 0.28f, 0.12f), new Color(0.22f, 0.13f, 0.05f), 4, false);
        public static Texture2D TexturaDoBotaoHover => botaoHover ??= CriarMoldura(32, new Color(0.55f, 0.38f, 0.16f), new Color(0.3f, 0.18f, 0.07f), 4, false);
        public static Texture2D TexturaDoBotaoPressionado => botaoPressionado ??= CriarMoldura(32, new Color(0.16f, 0.1f, 0.04f), new Color(0.25f, 0.16f, 0.06f), 4, false);
        public static Texture2D TexturaDoSlot => slot ??= CriarSlot(48);
        public static Texture2D TexturaDoSuporte => suporte ??= CriarSuporte(160);
        public static Texture2D TexturaDoOrbe => orbe ??= CriarOrbe(128);
        public static Texture2D TexturaDoAnel => anel ??= CriarAnel(128);
        static Texture2D FundoDaBarra => fundoDaBarra ??= CriarMoldura(32, new Color(0.05f, 0.04f, 0.03f, 0.95f), new Color(0.1f, 0.07f, 0.05f, 0.95f), 4, false);
        static Texture2D Branca => branca ??= Preencher(Novo(2, 2), Color.white);

        static Texture2D Novo(int l, int a)
        {
            return new Texture2D(l, a, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave,
            };
        }

        static Texture2D Preencher(Texture2D t, Color c)
        {
            var px = new Color[t.width * t.height];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        // Cravo em losango no canto das molduras (centro em pixels a partir da borda da textura)
        public const int CentroDoCravo = 8;
        const int RaioDoCravo = 7;
        const int Fatia = CentroDoCravo + RaioDoCravo + 3;

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

        // Slot de item: moldura bronze fina e fundo afundado, mais escuro perto da borda
        static Texture2D CriarSlot(int tamanho)
        {
            var t = Novo(tamanho, tamanho);
            var px = new Color[tamanho * tamanho];
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                int d = Mathf.Min(Mathf.Min(x, y), Mathf.Min(tamanho - 1 - x, tamanho - 1 - y));
                Color c;
                if (d == 0) c = new Color(0.03f, 0.02f, 0.01f);
                else if (d < 4) c = Metal(d == 1 ? 0.95f : 0.5f, x, y);
                else if (d == 4) c = new Color(0.02f, 0.01f, 0.005f);
                else
                {
                    float v = Mathf.Lerp(0.03f, 0.12f, Mathf.Clamp01((d - 4) / 10f)) * (0.85f + Grao(x, y) * 0.3f);
                    c = new Color(v * 1.15f, v * 0.9f, v * 0.7f);
                }
                px[y * tamanho + x] = c;
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        // Suporte de ferro em volta do orbe: anel grosso com filetes dourados e 8 garras para fora
        static Texture2D CriarSuporte(int tamanho)
        {
            var t = Novo(tamanho, tamanho);
            var px = new Color[tamanho * tamanho];
            float r = tamanho * 0.5f;
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                float vx = x + 0.5f - r, vy = y + 0.5f - r;
                float d = Mathf.Sqrt(vx * vx + vy * vy) / r;
                float garra = Mathf.Pow(Mathf.Abs(Mathf.Cos(Mathf.Atan2(vy, vx) * 4)), 12);
                float fora = 0.86f + garra * 0.13f;
                Color c = Color.clear;
                if (d > 0.7f && d < fora)
                {
                    float meio = (d - 0.7f) / (fora - 0.7f);
                    bool filete = d < 0.73f || (d > 0.79f && d < 0.82f);
                    float ferro = (0.16f + (1 - Mathf.Abs(meio - 0.4f)) * 0.14f) * (0.8f + Grao(x, y) * 0.4f);
                    c = filete ? Metal(0.9f - meio * 0.3f + vy / r * 0.2f, x, y) : new Color(ferro, ferro * 0.95f, ferro * 0.9f);
                    if (garra > 0.5f && meio > 0.55f) c = Metal(0.4f + garra * 0.5f + vy / r * 0.15f, x, y);
                    c.a = Mathf.Clamp01((fora - d) * r) * Mathf.Clamp01((d - 0.7f) * r);
                }
                px[y * tamanho + x] = c;
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        static Texture2D CriarOrbe(int tamanho)
        {
            var t = Novo(tamanho, tamanho);
            var px = new Color[tamanho * tamanho];
            float r = tamanho * 0.5f;
            var luz = new Vector2(tamanho * 0.38f, tamanho * 0.66f);
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, new Vector2(r, r)) / r;
                float alfa = Mathf.Clamp01((1 - d) * r);
                // sombra nas bordas e reflexo em cima, para parecer uma esfera de vidro
                float sombra = Mathf.Lerp(1f, 0.35f, d * d);
                float reflexo = Mathf.Clamp01(1 - Vector2.Distance(p, luz) / (r * 0.45f));
                float v = Mathf.Clamp01(sombra + reflexo * reflexo * 0.8f);
                px[y * tamanho + x] = new Color(v, v, v, alfa);
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        static Texture2D CriarAnel(int tamanho)
        {
            var t = Novo(tamanho, tamanho);
            var px = new Color[tamanho * tamanho];
            float r = tamanho * 0.5f;
            for (int y = 0; y < tamanho; y++)
            for (int x = 0; x < tamanho; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float dentro = r * 0.86f;
                float alfa = Mathf.Clamp01(d - dentro + 1) * Mathf.Clamp01(r - d);
                float brilho = 1 - Mathf.Abs(d - (dentro + r) * 0.5f) / ((r - dentro) * 0.5f);
                var c = Color.Lerp(OuroEscuro, OuroClaro, Mathf.Clamp01(brilho) * (0.6f + 0.4f * y / tamanho));
                c.a = alfa;
                px[y * tamanho + x] = c;
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }

        // ---------- estilos ----------

        // Fonte serifada do sistema para títulos e números; se nenhuma existir, a Unity usa a padrão
        static Font Serifada => serifada ??= Font.CreateDynamicFontFromOSFont(
            new[] { "Georgia", "Palatino Linotype", "Book Antiqua", "Times New Roman", "Noto Serif", "serif" }, 20);

        public static GUIStyle EstiloTitulo => titulo ??= new GUIStyle(GUI.skin.label)
        {
            font = Serifada, fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = OuroClaro },
        };

        public static GUIStyle EstiloTexto => texto ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 15, wordWrap = false, normal = { textColor = CorDoTexto },
        };

        public static GUIStyle EstiloBotao => botaoEstilo ??= new GUIStyle(GUI.skin.button)
        {
            font = Serifada, fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(5, 5, 5, 5),
            normal = { background = TexturaDoBotao, textColor = OuroClaro },
            hover = { background = TexturaDoBotaoHover, textColor = Color.white },
            active = { background = TexturaDoBotaoPressionado, textColor = Ouro },
        };

        public static GUIStyle EstiloNumero => numero ??= new GUIStyle(GUI.skin.label)
        {
            font = Serifada, fontSize = 30, fontStyle = FontStyle.BoldAndItalic, alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Overflow,
        };

        static GUIStyle EstiloDoSlot => estiloDoSlot ??= new GUIStyle { normal = { background = TexturaDoSlot }, border = new RectOffset(8, 8, 8, 8) };
        static GUIStyle EstiloDaBarra => estiloDaBarra ??= new GUIStyle { normal = { background = FundoDaBarra }, border = new RectOffset(4, 4, 4, 4) };
        static GUIStyle EstiloDoPainel => estiloDoPainel ??= new GUIStyle { normal = { background = TexturaDoPainel }, border = new RectOffset(Fatia, Fatia, Fatia, Fatia) };
        static GUIStyle EstiloDoCabecalho => estiloDoCabecalho ??= new GUIStyle(EstiloTitulo)
        {
            alignment = TextAnchor.MiddleCenter, fontSize = 18,
            normal = { background = TexturaDoCabecalho, textColor = OuroClaro },
            border = new RectOffset(Fatia, Fatia, Fatia, Fatia),
        };

        // ---------- desenho ----------

        public static float Fracao(double valor) => double.IsNaN(valor) ? 0f : Mathf.Clamp01((float)valor);

        // Painel com cabeçalho vermelho centralizado; devolve a área interna livre
        public static Rect Painel(Rect area, string tituloDoPainel)
        {
            GUI.Box(area, GUIContent.none, EstiloDoPainel);
            if (string.IsNullOrEmpty(tituloDoPainel)) return Recuar(area, Borda + 4);

            float largura = Mathf.Min(area.width * 0.6f, 280);
            var cab = new Rect(area.x + (area.width - largura) * 0.5f, area.y - 16, largura, 38);
            conteudo.text = tituloDoPainel;
            GUI.Box(cab, conteudo, EstiloDoCabecalho);
            var dentro = Recuar(area, Borda + 4);
            dentro.yMin += 22;
            return dentro;
        }

        public static string Inicial(string nome) => string.IsNullOrEmpty(nome) ? "?" : nome.Substring(0, 1);

        public static Rect Recuar(Rect r, float m) => new Rect(r.x + m, r.y + m, r.width - 2 * m, r.height - 2 * m);

        public static bool Botao(Rect r, string rotulo) => GUI.Button(r, rotulo, EstiloBotao);

        // Botão redondo de fechar no canto do painel
        public static bool BotaoFechar(Rect painelArea)
        {
            var r = new Rect(painelArea.xMax - 30, painelArea.y - 10, 36, 36);
            Desenhar(r, TexturaDoOrbe, VermelhoEscuro);
            Desenhar(r, TexturaDoAnel, Color.white);
            TextoComContorno(r, "X", EstiloTitulo, OuroClaro, TextAnchor.MiddleCenter, 18);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        public static void Barra(Rect r, float progresso, Color cor)
        {
            GUI.Box(r, GUIContent.none, EstiloDaBarra);
            var dentro = Recuar(r, 3);
            dentro.width *= Fracao(progresso);
            Desenhar(dentro, Branca, cor);
            // brilho na metade de cima
            Desenhar(new Rect(dentro.x, dentro.y, dentro.width, dentro.height * 0.4f), Branca, new Color(1, 1, 1, 0.22f));
            // pedras douradas nas pontas
            float g = r.height + 6;
            Desenhar(new Rect(r.x - g * 0.5f, r.center.y - g * 0.5f, g, g), TexturaDoOrbe, Ouro);
            Desenhar(new Rect(r.xMax - g * 0.5f, r.center.y - g * 0.5f, g, g), TexturaDoOrbe, Ouro);
        }

        // Slot de item com moldura de bronze; o rótulo faz as vezes de ícone até chegar a arte
        public static void Slot(Rect r, string rotulo, Color cor)
        {
            GUI.Box(r, GUIContent.none, EstiloDoSlot);
            if (!string.IsNullOrEmpty(rotulo))
                TextoComContorno(r, rotulo, EstiloNumero, cor, TextAnchor.MiddleCenter, Mathf.RoundToInt(r.height * 0.5f));
        }

        // Orbe de vidro que enche de baixo para cima, com anel dourado e o valor embaixo
        public static void Orbe(Rect r, float cheio, Color cor, string valor)
        {
            Desenhar(r, TexturaDoOrbe, new Color(0.08f, 0.05f, 0.04f));
            float f = Fracao(cheio);
            float altura = r.height * f;
            GUI.BeginGroup(new Rect(r.x, r.yMax - altura, r.width, altura));
            Desenhar(new Rect(0, altura - r.height, r.width, r.height), TexturaDoOrbe, cor);
            GUI.EndGroup();
            Desenhar(r, TexturaDoAnel, Color.white);
            // suporte de ferro com garras: o vão interno tem 70% do tamanho
            float s = r.width / 0.72f;
            Desenhar(new Rect(r.center.x - s * 0.5f, r.center.y - s * 0.5f, s, s), TexturaDoSuporte, Color.white);
            if (valor != null)
                TextoComContorno(new Rect(r.x - 40, r.yMax + r.width * 0.12f, r.width + 80, 26), valor, EstiloTitulo, CorDoTexto, TextAnchor.MiddleCenter, 16);
        }

        // Faixa com o nome da habilidade no centro da tela, com linhas douradas nas pontas
        public static void FaixaDeHabilidade(float centroX, float y, string nome, float alfa = 1f)
        {
            var e = EstiloTitulo;
            int tamanhoAntes = e.fontSize;
            e.fontSize = 30;
            conteudo.text = nome;
            float largura = e.CalcSize(conteudo).x + 80;
            e.fontSize = tamanhoAntes;

            var r = new Rect(centroX - largura * 0.5f, y, largura, 52);
            Desenhar(r, Branca, new Color(0.04f, 0.02f, 0.01f, 0.65f * alfa));
            Desenhar(new Rect(r.x - 30, r.yMax - 3, r.width + 60, 2), Branca, new Color(Ouro.r, Ouro.g, Ouro.b, alfa));
            Desenhar(new Rect(r.x, r.y, r.width, 1), Branca, new Color(OuroEscuro.r, OuroEscuro.g, OuroEscuro.b, alfa));
            Desenhar(new Rect(r.x - 36, r.yMax - 8, 12, 12), TexturaDoOrbe, new Color(Ouro.r, Ouro.g, Ouro.b, alfa));
            Desenhar(new Rect(r.xMax + 24, r.yMax - 8, 12, 12), TexturaDoOrbe, new Color(Ouro.r, Ouro.g, Ouro.b, alfa));
            var cor = OuroClaro; cor.a = alfa;
            TextoComContorno(r, nome, EstiloTitulo, cor, TextAnchor.MiddleCenter, 30);
        }

        // Texto com contorno escuro (número de dano, poder, combo)
        public static void TextoComContorno(Rect r, string valor, GUIStyle estilo, Color cor, TextAnchor alinhamento, int tamanho)
        {
            int tamanhoAntes = estilo.fontSize;
            var alinhamentoAntes = estilo.alignment;
            var corAntes = estilo.normal.textColor;
            estilo.fontSize = tamanho;
            estilo.alignment = alinhamento;
            conteudo.text = valor;

            estilo.normal.textColor = new Color(0.08f, 0.03f, 0f, cor.a * 0.9f);
            float o = Mathf.Max(1.5f, tamanho / 16f);
            GUI.Label(new Rect(r.x - o, r.y, r.width, r.height), conteudo, estilo);
            GUI.Label(new Rect(r.x + o, r.y, r.width, r.height), conteudo, estilo);
            GUI.Label(new Rect(r.x, r.y - o, r.width, r.height), conteudo, estilo);
            GUI.Label(new Rect(r.x, r.y + o, r.width, r.height), conteudo, estilo);
            estilo.normal.textColor = cor;
            GUI.Label(r, conteudo, estilo);

            estilo.fontSize = tamanhoAntes;
            estilo.alignment = alinhamentoAntes;
            estilo.normal.textColor = corAntes;
        }

        public static float LarguraDoTexto(string valor, GUIStyle estilo, int tamanho)
        {
            int antes = estilo.fontSize;
            estilo.fontSize = tamanho;
            conteudo.text = valor;
            float l = estilo.CalcSize(conteudo).x;
            estilo.fontSize = antes;
            return l;
        }

        public static void Separador(Rect r)
        {
            Desenhar(r, Branca, new Color(0, 0, 0, 0.28f));
            Desenhar(new Rect(r.x + 8, r.yMax - 1, r.width - 16, 1), Branca, new Color(Ouro.r, Ouro.g, Ouro.b, 0.45f));
        }

        static void Desenhar(Rect r, Texture t, Color cor)
        {
            var antes = GUI.color;
            GUI.color = cor;
            GUI.DrawTexture(r, t);
            GUI.color = antes;
        }
    }
}
