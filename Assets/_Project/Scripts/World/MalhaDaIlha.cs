using ALotOfDamage.Core;
using ALotOfDamage.Visual;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Ilha de terra: topo plano onde se anda, morros na borda e a praia descendo até o fundo do mar
    // (RegrasDoCenario.Altura). O topo usa a textura do chão; morro de fora e praia, a areia.
    public static class MalhaDaIlha
    {
        public const int Topo = 0, Praia = 1;
        const int Lados = 96;
        public const float MetrosPorLadrilho = 24;

        public static Mesh Criar(ParametrosDaIlha p, int semente)
        {
            var m = new Malhas(2);

            // anéis do topo: poucos na área plana, mais na borda onde o relevo muda
            var raios = new float[27];
            for (int i = 0; i <= 8; i++) raios[i] = p.RaioAndavel * i / 8f;
            for (int i = 1; i <= 10; i++) raios[8 + i] = p.RaioAndavel + (p.RaioDaBorda - p.RaioAndavel) * i / 10f;
            const int borda = 18;
            for (int i = 1; i <= 6; i++) raios[borda + i] = p.RaioDaBorda + (p.RaioDaPraia - p.RaioDaBorda) * i / 6f;
            raios[25] = p.RaioDaPraia + 12;
            raios[26] = p.RaioDaPraia + 40;

            var topo = new Vector3[raios.Length, Lados];
            for (int a = 0; a < raios.Length; a++)
            for (int l = 0; l < Lados; l++)
            {
                // linha da praia com recortes para não parecer um círculo perfeito
                float r = raios[a];
                if (a > borda) r *= 0.94f + Ruido.Valor(semente + 3, l) * 0.12f;
                float ang = l * Mathf.PI * 2 / Lados;
                float x = Mathf.Cos(ang) * r, z = Mathf.Sin(ang) * r;
                topo[a, l] = new Vector3(x, RegrasDoCenario.Altura(p, x, z, semente), z);
            }
            for (int a = 0; a < raios.Length - 1; a++)
            for (int l = 0; l < Lados; l++)
            {
                int n = (l + 1) % Lados;
                if (a == 0) m.Triangulo(Topo, topo[0, 0], topo[1, n], topo[1, l]);
                else m.Quad(a < borda ? Topo : Praia, topo[a, l], topo[a, n], topo[a + 1, n], topo[a + 1, l]);
            }

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
