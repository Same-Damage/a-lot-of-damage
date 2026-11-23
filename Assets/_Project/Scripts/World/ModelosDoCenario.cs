using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Árvores, pinheiros, pedras, cristais, ruínas, arbustos e grama em low-poly, feitos por código.
    // Todos vão para uma malha só com uma parte por material, para o mapa inteiro custar poucos draw calls.
    public static class ModelosDoCenario
    {
        public const int Folhagem = 0, Tronco = 1, Rocha = 2, Cristal = 3, Grama = 4, Partes = 5;

        public static void Adicionar(Malhas m, ItemDoCenario item, int semente)
        {
            int s = semente * 7919 + Mathf.RoundToInt(item.X * 13) * 131 + Mathf.RoundToInt(item.Z * 17);
            m.Transformacao = Matrix4x4.TRS(new Vector3(item.X, item.Y, item.Z), Quaternion.Euler(0, item.Rotacao, 0), Vector3.one * item.Escala);
            switch (item.Tipo)
            {
                case TipoDeItemDoCenario.Arvore: Arvore(m, s); break;
                case TipoDeItemDoCenario.Pinheiro: Pinheiro(m, s); break;
                case TipoDeItemDoCenario.Pedra: Pedra(m, s); break;
                case TipoDeItemDoCenario.Cristal: Cristais(m, s); break;
                case TipoDeItemDoCenario.Ruina: Ruina(m, s); break;
                case TipoDeItemDoCenario.Arbusto: Arbusto(m, s); break;
                case TipoDeItemDoCenario.Grama: Tufo(m, s); break;
            }
            m.Transformacao = Matrix4x4.identity;
        }

        static float R(int s, int i) => Ruido.Valor(s, i);

        static void Arvore(Malhas m, int s)
        {
            float altura = 2.2f + R(s, 1) * 1.2f;
            m.Cilindro(Tronco, Vector3.down * 0.2f, 0.32f, 0.2f, altura + 0.2f, 6);
            // copa em três bolhas, a de cima menor
            m.Bolha(Folhagem, new Vector3(0, altura + 0.6f, 0), new Vector3(1.7f, 1.3f, 1.7f), s);
            m.Bolha(Folhagem, new Vector3(0.7f, altura + 0.1f, 0.3f), new Vector3(1.1f, 0.9f, 1.1f), s + 1);
            m.Bolha(Folhagem, new Vector3(-0.4f, altura + 1.5f, -0.2f), new Vector3(1.0f, 0.8f, 1.0f), s + 2);
        }

        static void Pinheiro(Malhas m, int s)
        {
            float altura = 3.2f + R(s, 1) * 1.6f;
            m.Cilindro(Tronco, Vector3.down * 0.2f, 0.25f, 0.18f, 1.2f, 5);
            float y = 0.8f, raio = 1.5f;
            for (int i = 0; i < 3; i++)
            {
                float h = altura * (0.5f - i * 0.08f);
                m.Cilindro(Folhagem, new Vector3(0, y, 0), raio, 0, h, 7, i * 0.45f);
                y += h * 0.55f;
                raio *= 0.72f;
            }
        }

        static void Pedra(Malhas m, int s)
        {
            float l = 0.8f + R(s, 1) * 0.7f;
            m.Bolha(Rocha, new Vector3(0, l * 0.25f, 0), new Vector3(l, l * 0.7f, l * 0.85f), s, 0.28f, 3, 6);
            if (R(s, 2) > 0.5f)
                m.Bolha(Rocha, new Vector3(l * 0.9f, 0.1f, 0.3f), new Vector3(l * 0.45f, l * 0.35f, l * 0.4f), s + 5, 0.3f, 3, 5);
        }

        static void Cristais(Malhas m, int s)
        {
            m.Bolha(Rocha, Vector3.zero, new Vector3(0.9f, 0.35f, 0.9f), s, 0.25f, 3, 6);
            for (int i = 0; i < 3; i++)
            {
                float ang = i * 120 + R(s, i) * 40;
                var incl = Quaternion.Euler(i == 0 ? 0 : 22, ang, 0);
                var pos = Quaternion.Euler(0, ang, 0) * new Vector3(i == 0 ? 0 : 0.35f, 0.1f, 0);
                m.Cristal(Cristal, pos, i == 0 ? 0.32f : 0.22f, i == 0 ? 2.4f : 1.5f + R(s, i + 3) * 0.5f, incl);
            }
        }

        static void Ruina(Malhas m, int s)
        {
            // pilar de oito lados quebrado em altura irregular, com base e um bloco caído ao lado
            float altura = 1.8f + R(s, 1) * 2.4f;
            m.Cilindro(Rocha, Vector3.down * 0.1f, 0.95f, 0.95f, 0.4f, 8, Mathf.PI / 8);
            m.Cilindro(Rocha, new Vector3(0, 0.3f, 0), 0.6f, 0.55f, altura, 8, Mathf.PI / 8);
            m.Cilindro(Rocha, new Vector3(0, 0.3f + altura, 0), 0.62f, 0.3f, 0.35f + R(s, 2) * 0.4f, 5);
            if (R(s, 3) > 0.35f)
            {
                var antes = m.Transformacao;
                m.Transformacao = antes * Matrix4x4.TRS(new Vector3(1.4f, 0.25f, 0.4f), Quaternion.Euler(0, 30, 90), Vector3.one);
                m.Cilindro(Rocha, Vector3.zero, 0.45f, 0.42f, 1.1f, 8);
                m.Transformacao = antes;
            }
        }

        static void Arbusto(Malhas m, int s)
        {
            m.Bolha(Folhagem, new Vector3(0, 0.35f, 0), new Vector3(0.8f, 0.6f, 0.8f), s, 0.22f, 3, 6);
            m.Bolha(Folhagem, new Vector3(0.5f, 0.25f, 0.2f), new Vector3(0.5f, 0.45f, 0.5f), s + 3, 0.22f, 3, 6);
        }

        static void Tufo(Malhas m, int s)
        {
            for (int i = 0; i < 4; i++)
            {
                var pos = new Vector3((R(s, i) - 0.5f) * 0.5f, 0, (R(s, i + 9) - 0.5f) * 0.5f);
                m.Lamina(Grama, pos, 0.14f, 0.35f + R(s, i + 4) * 0.35f, i * 0.8f + R(s, 2), 0.12f);
            }
        }
    }
}
