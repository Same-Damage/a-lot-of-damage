using System.Collections.Generic;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Construtor de malhas low-poly com sombreamento chapado (cada triângulo com a própria normal),
    // o visual "facetado" dos jogos isométricos estilizados.
    public class Malhas
    {
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<int>[] partes;

        // Posição, giro e escala aplicados a tudo o que for adicionado depois
        public Matrix4x4 Transformacao = Matrix4x4.identity;

        public Malhas(int quantidadeDePartes)
        {
            partes = new List<int>[quantidadeDePartes];
            for (int i = 0; i < partes.Length; i++) partes[i] = new List<int>();
        }

        public void Triangulo(int parte, Vector3 a, Vector3 b, Vector3 c)
        {
            int i = vertices.Count;
            vertices.Add(Transformacao.MultiplyPoint3x4(a));
            vertices.Add(Transformacao.MultiplyPoint3x4(b));
            vertices.Add(Transformacao.MultiplyPoint3x4(c));
            partes[parte].Add(i); partes[parte].Add(i + 1); partes[parte].Add(i + 2);
        }

        public void Quad(int parte, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Triangulo(parte, a, b, c);
            Triangulo(parte, a, c, d);
        }

        public Mesh Criar(string nome)
        {
            var m = new Mesh { name = nome, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            m.SetVertices(vertices);
            m.subMeshCount = partes.Length;
            for (int i = 0; i < partes.Length; i++) m.SetTriangles(partes[i], i);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        // ---------- formas ----------

        // Prisma ou cone (raioDeCima = 0) em volta do eixo Y
        public void Cilindro(int parte, Vector3 baseCentro, float raioDeBaixo, float raioDeCima, float altura, int lados, float giro = 0)
        {
            for (int i = 0; i < lados; i++)
            {
                float a0 = giro + i * Mathf.PI * 2 / lados, a1 = giro + (i + 1) * Mathf.PI * 2 / lados;
                Vector3 d0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), d1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                Vector3 b0 = baseCentro + d0 * raioDeBaixo, b1 = baseCentro + d1 * raioDeBaixo;
                Vector3 topo = baseCentro + Vector3.up * altura;
                if (raioDeCima <= 0.001f)
                    Triangulo(parte, b0, topo, b1);
                else
                {
                    Vector3 c0 = topo + d0 * raioDeCima, c1 = topo + d1 * raioDeCima;
                    Quad(parte, b0, c0, c1, b1);
                    Triangulo(parte, topo, c1, c0);
                }
            }
        }

        // Esfera irregular de poucos lados (copa, pedra, arbusto); "ruido" deforma cada vértice
        public void Bolha(int parte, Vector3 centro, Vector3 raio, int semente, float ruido = 0.18f, int faixas = 4, int lados = 7)
        {
            var anel = new Vector3[faixas + 1, lados];
            for (int f = 0; f <= faixas; f++)
            for (int l = 0; l < lados; l++)
            {
                float lat = Mathf.PI * f / faixas - Mathf.PI / 2, lon = Mathf.PI * 2 * l / lados + f * 0.4f;
                float k = 1 + (Ruido.Valor(semente + f * 31, l * 17) - 0.5f) * 2 * ruido;
                var p = new Vector3(Mathf.Cos(lat) * Mathf.Cos(lon), Mathf.Sin(lat), Mathf.Cos(lat) * Mathf.Sin(lon)) * k;
                anel[f, l] = centro + Vector3.Scale(p, raio);
            }
            for (int f = 0; f < faixas; f++)
            for (int l = 0; l < lados; l++)
            {
                int n = (l + 1) % lados;
                if (f == 0) Triangulo(parte, anel[0, 0], anel[1, l], anel[1, n]);
                else if (f == faixas - 1) Triangulo(parte, anel[f, l], anel[faixas, 0], anel[f, n]);
                else Quad(parte, anel[f, l], anel[f + 1, l], anel[f + 1, n], anel[f, n]);
            }
        }

        // Cristal: losango alongado de seis lados
        public void Cristal(int parte, Vector3 baseCentro, float raio, float altura, Quaternion inclinacao)
        {
            const int lados = 6;
            Vector3 baixo = baseCentro, cima = baseCentro + inclinacao * Vector3.up * altura;
            Vector3 meio = baseCentro + inclinacao * Vector3.up * altura * 0.3f;
            for (int i = 0; i < lados; i++)
            {
                float a0 = i * Mathf.PI * 2 / lados, a1 = (i + 1) * Mathf.PI * 2 / lados;
                Vector3 m0 = meio + inclinacao * new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)) * raio;
                Vector3 m1 = meio + inclinacao * new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * raio;
                Triangulo(parte, baixo, m0, m1);
                Triangulo(parte, m0, cima, m1);
            }
        }

        // Folha fina de grama, com as duas faces
        public void Lamina(int parte, Vector3 baseCentro, float largura, float altura, float angulo, float curva)
        {
            var lado = new Vector3(Mathf.Cos(angulo), 0, Mathf.Sin(angulo)) * largura * 0.5f;
            var ponta = baseCentro + Vector3.up * altura + new Vector3(-lado.z, 0, lado.x).normalized * curva;
            Triangulo(parte, baseCentro - lado, ponta, baseCentro + lado);
            Triangulo(parte, baseCentro + lado, ponta, baseCentro - lado);
        }
    }

    // Ruído por hash: o mesmo valor para os mesmos números, sem depender de Random
    public static class Ruido
    {
        public static float Valor(int x, int y)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFF) / 65535f;
            }
        }
    }
}
