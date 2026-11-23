using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Ilha flutuante: topo plano onde se anda, morros na borda (RegrasDoCenario.Altura)
    // e por baixo um penhasco de rocha afinando até a ponta, sobre o mar de nuvens.
    public static class MalhaDaIlha
    {
        public const int Topo = 0, Penhasco = 1;
        const int Lados = 96;
        public const float MetrosPorLadrilho = 24;

        public static Mesh Criar(ParametrosDaIlha p, int semente)
        {
            var m = new Malhas(2);

            // anéis do topo: poucos na área plana, mais na borda onde o relevo muda
            var raios = new float[19];
            for (int i = 0; i <= 8; i++) raios[i] = p.RaioAndavel * i / 8f;
            for (int i = 1; i <= 10; i++) raios[8 + i] = p.RaioAndavel + (p.RaioDaBorda - p.RaioAndavel) * i / 10f;

            var topo = new Vector3[raios.Length, Lados];
            for (int a = 0; a < raios.Length; a++)
            for (int l = 0; l < Lados; l++)
            {
                // borda com recortes para não parecer um círculo perfeito
                float r = raios[a];
                if (a == raios.Length - 1) r *= 0.96f + Ruido.Valor(semente + 3, l) * 0.06f;
                float ang = l * Mathf.PI * 2 / Lados;
                float x = Mathf.Cos(ang) * r, z = Mathf.Sin(ang) * r;
                topo[a, l] = new Vector3(x, RegrasDoCenario.Altura(p, x, z, semente), z);
            }
            for (int a = 0; a < raios.Length - 1; a++)
            for (int l = 0; l < Lados; l++)
            {
                int n = (l + 1) % Lados;
                if (a == 0) m.Triangulo(Topo, topo[0, 0], topo[1, n], topo[1, l]);
                else m.Quad(Topo, topo[a, l], topo[a, n], topo[a + 1, n], topo[a + 1, l]);
            }

            // penhasco: anéis descendo e fechando até a ponta, com rocha irregular
            float[] fator = { 1f, 0.97f, 0.88f, 0.7f, 0.45f, 0.18f };
            float[] fundo = { 0, -2.5f, -8, -15, -22, -28 };
            var borda = raios.Length - 1;
            var anel = new Vector3[fator.Length, Lados];
            for (int f = 0; f < fator.Length; f++)
            for (int l = 0; l < Lados; l++)
            {
                if (f == 0) { anel[0, l] = topo[borda, l]; continue; }
                float ang = l * Mathf.PI * 2 / Lados;
                float r = p.RaioDaBorda * fator[f] * (0.9f + Ruido.Valor(semente + f * 11, l) * 0.2f);
                float y = fundo[f] + (Ruido.Valor(semente + f * 7, l + 300) - 0.5f) * 2.5f;
                anel[f, l] = new Vector3(Mathf.Cos(ang) * r, y, Mathf.Sin(ang) * r);
            }
            for (int f = 0; f < fator.Length - 1; f++)
            for (int l = 0; l < Lados; l++)
            {
                int n = (l + 1) % Lados;
                m.Quad(Penhasco, anel[f, l], anel[f, n], anel[f + 1, n], anel[f + 1, l]);
            }
            var ponta = new Vector3(0, fundo[fundo.Length - 1] - 6, 0);
            int ultimo = fator.Length - 1;
            for (int l = 0; l < Lados; l++)
                m.Triangulo(Penhasco, anel[ultimo, l], anel[ultimo, (l + 1) % Lados], ponta);

            // UV em metros do mundo para a textura do chão ladrilhar igual em toda a ilha
            var malha = m.Criar("Ilha");
            var vertices = malha.vertices;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2(vertices[i].x, vertices[i].z) / MetrosPorLadrilho;
            malha.uv = uv;
            return malha;
        }
    }
}
