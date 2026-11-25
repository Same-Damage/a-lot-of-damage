using ALotOfDamage.Visual;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Praça da cidade, low-poly e numa malha só: mar, ilha, calçada, fonte, taverna, ferraria, casas, barracas, postes e árvores
    public static class ModeloDaCidade
    {
        public static readonly string[] Cores =
        {
            "#4C9A57", "#CFC4AE", "#B8AB94", "#E9DCC3", "#A8483C", "#4C6FA8", "#7A5233", "#59B7D9", "#F2C14E", "#3E7C46", "#D8C08A", "#6E6A66", "#2F7FB0",
        };
        const int Grama = 0, Pedra = 1, PedraEscura = 2, Parede = 3, Telha = 4, TelhaAzul = 5, Madeira = 6, Agua = 7, Ouro = 8, Folha = 9, Areia = 10, Ferro = 11, Mar = 12;

        public static Mesh Criar(float raio)
        {
            var m = new Malhas(Cores.Length);
            m.Cilindro(Mar, new Vector3(0, -3f, 0), 160, 160, 0.5f, 32);
            m.Cilindro(Areia, new Vector3(0, -3f, 0), raio + 16, raio + 8, 2.6f, 32);
            m.Cilindro(Grama, new Vector3(0, -1f, 0), raio + 8, raio + 6, 1f, 32);
            m.Cilindro(PedraEscura, new Vector3(0, -0.3f, 0), 17.6f, 17.4f, 0.34f, 24);
            m.Cilindro(Pedra, new Vector3(0, -0.3f, 0), 16.6f, 16.5f, 0.38f, 24);
            Fonte(m);
            Casa(m, 0, 22, 9, 6, 4.2f, Telha, true);        // taverna
            Casa(m, 62, 22, 7, 5.5f, 3.6f, TelhaAzul, false);
            Casa(m, 118, 22.5f, 6, 5, 3.4f, Telha, false);
            Casa(m, 242, 22.5f, 6, 5, 3.4f, TelhaAzul, false);
            Casa(m, 298, 22, 7.5f, 5.5f, 3.6f, Telha, false);
            Ferraria(m);
            for (int i = 0; i < 8; i++) Poste(m, i * 45 + 22.5f, 13.5f);
            for (int i = 0; i < 4; i++) Barraca(m, 40 + i * 80 + (i > 1 ? 40 : 0), 14.2f);
            for (int i = 0; i < 26; i++) Arvore(m, i * 137.5f, raio + 1.5f + (i % 3) * 1.6f, 0.85f + (i % 4) * 0.16f);
            for (int i = 0; i < 8; i++) Arvore(m, i * 45 + 30, 27.5f, 0.8f + (i % 3) * 0.12f);
            m.Transformacao = Matrix4x4.identity;
            return m.Criar("Cidade");
        }

        // Origem no ponto da circunferência, olhando para o centro da praça
        static void Em(Malhas m, float graus, float distancia, float escala = 1)
        {
            var r = Quaternion.Euler(0, graus, 0);
            m.Transformacao = Matrix4x4.TRS(r * new Vector3(0, 0, distancia), r * Quaternion.Euler(0, 180, 0), Vector3.one * escala);
        }

        static void Bloco(Malhas m, int parte, Vector3 centroDaBase, Vector3 t)
        {
            Vector3 P(float x, float y, float z) => centroDaBase + new Vector3(x * t.x / 2, y * t.y, z * t.z / 2);
            m.Quad(parte, P(-1, 0, -1), P(-1, 1, -1), P(1, 1, -1), P(1, 0, -1));
            m.Quad(parte, P(1, 0, 1), P(1, 1, 1), P(-1, 1, 1), P(-1, 0, 1));
            m.Quad(parte, P(-1, 0, 1), P(-1, 1, 1), P(-1, 1, -1), P(-1, 0, -1));
            m.Quad(parte, P(1, 0, -1), P(1, 1, -1), P(1, 1, 1), P(1, 0, 1));
            m.Quad(parte, P(-1, 1, -1), P(-1, 1, 1), P(1, 1, 1), P(1, 1, -1));
        }

        static void Casa(Malhas m, float graus, float distancia, float largura, float fundo, float altura, int telha, bool placa)
        {
            Em(m, graus, distancia);
            Bloco(m, PedraEscura, Vector3.zero, new Vector3(largura + 0.3f, 0.6f, fundo + 0.3f));
            Bloco(m, Parede, Vector3.zero, new Vector3(largura, altura, fundo));
            foreach (float x in new[] { -1f, 1f }) foreach (float z in new[] { -1f, 1f })
                Bloco(m, Madeira, new Vector3(x * largura / 2, 0, z * fundo / 2), new Vector3(0.35f, altura, 0.35f));
            float l = largura / 2 + 0.6f, f = fundo / 2 + 0.6f, topo = altura + fundo * 0.5f;
            Vector3 a = new Vector3(-l, altura, f), b = new Vector3(l, altura, f), c = new Vector3(l, altura, -f), d = new Vector3(-l, altura, -f);
            Vector3 e = new Vector3(-l, topo, 0), g = new Vector3(l, topo, 0);
            m.Quad(telha, b, g, e, a);
            m.Quad(telha, d, e, g, c);
            m.Triangulo(Madeira, a, e, d);
            m.Triangulo(Madeira, c, g, b);
            m.Quad(Madeira, a, d, c, b);
            float frente = fundo / 2;
            Bloco(m, Madeira, new Vector3(0, 0, frente), new Vector3(1.5f, 2.5f, 0.2f));
            foreach (float x in new[] { -0.32f, 0.32f })
            {
                Bloco(m, Madeira, new Vector3(x * largura, 1.3f, frente), new Vector3(1.3f, 1.3f, 0.16f));
                Bloco(m, Ouro, new Vector3(x * largura, 1.45f, frente), new Vector3(1f, 1f, 0.24f));
            }
            if (!placa) return;
            Bloco(m, Madeira, new Vector3(0, 2.8f, frente + 0.3f), new Vector3(3f, 0.9f, 0.2f));
            Bloco(m, Ouro, new Vector3(0, 3.05f, frente + 0.36f), new Vector3(1.6f, 0.4f, 0.2f));
        }

        static void Ferraria(Malhas m)
        {
            Em(m, 180, 23);
            Bloco(m, PedraEscura, new Vector3(0, 0, -1.6f), new Vector3(7, 3.4f, 1));
            foreach (float x in new[] { -3.2f, 3.2f }) Bloco(m, Madeira, new Vector3(x, 0, 2f), new Vector3(0.4f, 3f, 0.4f));
            m.Quad(Telha, new Vector3(4, 2.9f, 2.8f), new Vector3(4, 4.2f, -2.4f), new Vector3(-4, 4.2f, -2.4f), new Vector3(-4, 2.9f, 2.8f));
            m.Quad(Madeira, new Vector3(-4, 2.85f, 2.8f), new Vector3(-4, 4.15f, -2.4f), new Vector3(4, 4.15f, -2.4f), new Vector3(4, 2.85f, 2.8f));
            Bloco(m, Madeira, new Vector3(1.4f, 0, 0.8f), new Vector3(0.9f, 0.8f, 0.9f));
            Bloco(m, Ferro, new Vector3(1.4f, 0.8f, 0.8f), new Vector3(1.5f, 0.45f, 0.6f));
            Bloco(m, Pedra, new Vector3(-1.4f, 0, -0.4f), new Vector3(2, 1.1f, 1.6f));
            Bloco(m, Ouro, new Vector3(-1.4f, 1.05f, -0.4f), new Vector3(1.4f, 0.2f, 1f));
        }

        static void Fonte(Malhas m)
        {
            m.Transformacao = Matrix4x4.identity;
            m.Cilindro(PedraEscura, Vector3.zero, 3.4f, 3.2f, 0.8f, 12);
            m.Cilindro(Agua, new Vector3(0, 0.1f, 0), 2.8f, 2.8f, 0.74f, 12);
            m.Cilindro(Pedra, Vector3.zero, 0.8f, 0.5f, 2.4f, 8);
            m.Cilindro(PedraEscura, new Vector3(0, 2.2f, 0), 0.6f, 1.5f, 0.5f, 10);
            m.Cilindro(Agua, new Vector3(0, 2.3f, 0), 1.2f, 1.2f, 0.42f, 10);
            m.Cilindro(Ouro, new Vector3(0, 2.7f, 0), 0.35f, 0, 1.3f, 6);
        }

        static void Poste(Malhas m, float graus, float distancia)
        {
            Em(m, graus, distancia);
            m.Cilindro(Ferro, Vector3.zero, 0.16f, 0.1f, 3f, 6);
            m.Cilindro(Ouro, new Vector3(0, 3f, 0), 0.24f, 0.36f, 0.5f, 6);
            m.Cilindro(Ferro, new Vector3(0, 3.5f, 0), 0.42f, 0, 0.35f, 6);
        }

        static void Barraca(Malhas m, float graus, float distancia)
        {
            Em(m, graus, distancia);
            Bloco(m, Madeira, Vector3.zero, new Vector3(2.8f, 0.9f, 1f));
            foreach (float x in new[] { -1.3f, 1.3f }) Bloco(m, Madeira, new Vector3(x, 0, -0.5f), new Vector3(0.16f, 2.4f, 0.16f));
            int cor = graus > 150 ? TelhaAzul : Telha;
            m.Quad(cor, new Vector3(1.6f, 2f, 0.9f), new Vector3(1.6f, 2.6f, -0.8f), new Vector3(-1.6f, 2.6f, -0.8f), new Vector3(-1.6f, 2f, 0.9f));
            m.Quad(cor, new Vector3(-1.6f, 2f, 0.9f), new Vector3(-1.6f, 2.6f, -0.8f), new Vector3(1.6f, 2.6f, -0.8f), new Vector3(1.6f, 2f, 0.9f));
            for (int i = 0; i < 3; i++) m.Cilindro(i == 1 ? Ouro : Folha, new Vector3(-0.8f + i * 0.8f, 0.9f, 0), 0.26f, 0.12f, 0.34f, 6);
        }

        static void Arvore(Malhas m, float graus, float distancia, float escala)
        {
            Em(m, graus, distancia, escala);
            m.Cilindro(Madeira, Vector3.zero, 0.3f, 0.22f, 1.3f, 6);
            m.Cilindro(Folha, new Vector3(0, 1.1f, 0), 1.5f, 0, 2f, 7, 0.4f);
            m.Cilindro(Folha, new Vector3(0, 2.2f, 0), 1.15f, 0, 1.8f, 7);
        }
    }
}
