using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Monstro low-poly provisório (gosma, quadrúpede ou golem), feito por código e animado por RegrasDeAnimacao.Monstro.
    // Fica no lugar da arte pintada até ela chegar; o dono só liga Andando e diz para onde olhar.
    public class MonstroLowPoly : MonoBehaviour
    {
        public Material materialBase;   // URP Lit
        public float rapidezAoVirar = 10f;

        const int Pele = 0, Detalhe = 1, Olho = 2, Pupila = 3, Brilho = 4, Partes = 5;

        public bool Andando { get; set; }

        FormaDoMonstro forma;
        Transform corpo, cabeca, cauda;
        readonly List<Transform> pernasA = new List<Transform>(), pernasB = new List<Transform>();
        Transform bracoEsquerdo, bracoDireito;
        Vector3 posicaoDoCorpo;
        Material[] materiais;
        readonly List<Object> criados = new List<Object>();
        float desfase;
        Quaternion olhando = Quaternion.identity;

        // começa olhando para onde a cena deixou
        void Awake() => olhando = transform.localRotation;

        public void Montar(FormaDoMonstro formaDoMonstro, Color cor)
        {
            Limpar();
            forma = formaDoMonstro;
            // cada monstro começa o ciclo num ponto diferente, para o bando não andar sincronizado
            desfase = Ruido.Valor(Mathf.RoundToInt(transform.position.x * 10), Mathf.RoundToInt(transform.position.z * 10)) * 3;
            materiais = new[]
            {
                Novo(cor), Novo(cor * 0.62f), Novo(new Color(0.95f, 0.95f, 0.9f)), Novo(new Color(0.06f, 0.05f, 0.05f)),
                Novo(forma == FormaDoMonstro.Golem ? new Color(0.4f, 0.9f, 1f) : Color.white, 1.3f),
            };
            switch (forma)
            {
                case FormaDoMonstro.Gosma: Gosma(); break;
                case FormaDoMonstro.Quadrupede: Quadrupede(); break;
                case FormaDoMonstro.Golem: Golem(); break;
            }
            posicaoDoCorpo = corpo.localPosition;
        }

        public void OlharPara(Vector3 direcao)
        {
            direcao.y = 0;
            if (direcao.sqrMagnitude > 0.0004f) olhando = Quaternion.LookRotation(direcao);
        }

        void Update()
        {
            if (!corpo) return;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, olhando, rapidezAoVirar * Time.deltaTime);

            var p = RegrasDeAnimacao.Monstro(forma, Time.time + desfase, Andando);
            corpo.localPosition = posicaoDoCorpo + new Vector3(0, p.Elevacao, 0);
            corpo.localScale = new Vector3(p.Largura, p.Altura, p.Largura);
            corpo.localRotation = Quaternion.Euler(0, 0, p.Inclinacao);
            foreach (var perna in pernasA) perna.localRotation = Quaternion.Euler(p.PernasA, 0, 0);
            foreach (var perna in pernasB) perna.localRotation = Quaternion.Euler(p.PernasB, 0, 0);
            if (bracoEsquerdo) bracoEsquerdo.localRotation = Quaternion.Euler(p.Bracos, 0, 0);
            if (bracoDireito) bracoDireito.localRotation = Quaternion.Euler(-p.Bracos, 0, 0);
            if (cabeca) cabeca.localRotation = Quaternion.Euler(p.Cabeca, 0, 0);
            if (cauda) cauda.localRotation = Quaternion.Euler(0, p.Cauda, 0);
        }

        void OnDestroy() => Limpar();

        void Limpar()
        {
            foreach (var o in criados) if (o) Destroy(o);
            criados.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            pernasA.Clear();
            pernasB.Clear();
            corpo = cabeca = cauda = bracoEsquerdo = bracoDireito = null;
        }

        Material Novo(Color cor, float brilhoProprio = 0)
        {
            var m = new Material(materialBase) { color = cor };
            m.SetFloat("_Smoothness", 0.25f);
            if (brilhoProprio > 0)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", cor * brilhoProprio);
            }
            criados.Add(m);
            return m;
        }

        // Peça com malha própria, para poder girar sozinha
        Transform Peca(string nome, Transform pai, Vector3 posicao, System.Action<Malhas> desenhar)
        {
            var go = new GameObject(nome, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(pai, false);
            go.transform.localPosition = posicao;
            var m = new Malhas(Partes);
            desenhar(m);
            var malha = m.Criar(nome);
            criados.Add(malha);
            go.GetComponent<MeshFilter>().sharedMesh = malha;
            go.GetComponent<MeshRenderer>().sharedMaterials = materiais;
            return go.transform;
        }

        static void Olhos(Malhas m, float x, float y, float z, float raio)
        {
            for (int lado = -1; lado <= 1; lado += 2)
            {
                m.Bolha(Olho, new Vector3(lado * x, y, z), Vector3.one * raio, 3, 0.02f, 4, 8);
                m.Bolha(Pupila, new Vector3(lado * x, y + raio * 0.1f, z + raio * 0.65f), Vector3.one * raio * 0.5f, 4, 0.02f, 3, 6);
            }
        }

        // ---------- formas ----------

        void Gosma()
        {
            // pivô na base, para amassar contra o chão
            corpo = Peca("Corpo", transform, Vector3.zero, m =>
            {
                m.Bolha(Pele, new Vector3(0, 0.5f, 0), new Vector3(0.62f, 0.5f, 0.62f), 11, 0.05f, 5, 10);
                Olhos(m, 0.2f, 0.62f, 0.5f, 0.13f);
            });
        }

        void Quadrupede()
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 0.78f, 0), m =>
                m.Bolha(Pele, Vector3.zero, new Vector3(0.38f, 0.36f, 0.75f), 21, 0.12f, 4, 7));

            cabeca = Peca("Cabeca", corpo, new Vector3(0, 0.25f, 0.7f), m =>
            {
                m.Bolha(Pele, new Vector3(0, 0.1f, 0.12f), new Vector3(0.3f, 0.28f, 0.32f), 22, 0.1f, 4, 7);
                m.Bolha(Detalhe, new Vector3(0, 0.02f, 0.42f), new Vector3(0.15f, 0.13f, 0.24f), 23, 0.08f, 3, 6);
                m.Bolha(Pupila, new Vector3(0, 0.07f, 0.64f), Vector3.one * 0.06f, 24, 0, 3, 5);
                for (int lado = -1; lado <= 1; lado += 2)
                    m.Cilindro(Detalhe, new Vector3(lado * 0.15f, 0.3f, 0.05f), 0.09f, 0, 0.26f, 4);
                Olhos(m, 0.13f, 0.2f, 0.36f, 0.065f);
            });

            cauda = Peca("Cauda", corpo, new Vector3(0, 0.15f, -0.7f), m =>
                m.Bolha(Detalhe, new Vector3(0, 0.1f, -0.25f), new Vector3(0.11f, 0.11f, 0.32f), 25, 0.1f, 3, 6));

            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -0.22f : 0.22f, z = i < 2 ? 0.45f : -0.45f;
                var perna = Peca("Perna", corpo, new Vector3(x, -0.15f, z), m =>
                    m.Cilindro(Detalhe, new Vector3(0, -0.63f, 0), 0.09f, 0.12f, 0.63f, 5));
                // diagonais andam juntas: frente-esquerda com trás-direita
                (i == 0 || i == 3 ? pernasA : pernasB).Add(perna);
            }
        }

        void Golem()
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 1f, 0), m =>
            {
                m.Bolha(Pele, new Vector3(0, 0.35f, 0), new Vector3(0.75f, 0.65f, 0.55f), 31, 0.25f, 4, 7);
                m.Bolha(Pele, new Vector3(0, 1.05f, 0.1f), new Vector3(0.32f, 0.28f, 0.3f), 32, 0.2f, 3, 6);
                for (int lado = -1; lado <= 1; lado += 2)
                    m.Bolha(Brilho, new Vector3(lado * 0.12f, 1.08f, 0.36f), Vector3.one * 0.07f, 33, 0, 3, 5);
                // cristais brilhando no peito
                m.Cristal(Brilho, new Vector3(0, 0.45f, 0.45f), 0.14f, 0.45f, Quaternion.Euler(70, 0, 0));
                m.Cristal(Brilho, new Vector3(0.22f, 0.6f, 0.4f), 0.09f, 0.3f, Quaternion.Euler(65, 30, 0));
            });

            Transform Braco(float lado) => Peca("Braco", corpo, new Vector3(lado * 0.85f, 0.65f, 0), m =>
            {
                m.Bolha(Detalhe, new Vector3(0, -0.45f, 0), new Vector3(0.26f, 0.5f, 0.26f), 34, 0.2f, 3, 6);
                m.Bolha(Pele, new Vector3(0, -1.0f, 0.05f), new Vector3(0.32f, 0.28f, 0.32f), 35, 0.25f, 3, 6);
            });
            bracoEsquerdo = Braco(-1);
            bracoDireito = Braco(1);

            for (int lado = -1; lado <= 1; lado += 2)
            {
                var perna = Peca("Perna", corpo, new Vector3(lado * 0.35f, -0.15f, 0), m =>
                    m.Cilindro(Detalhe, new Vector3(0, -0.85f, 0), 0.3f, 0.26f, 0.85f, 6));
                (lado < 0 ? pernasA : pernasB).Add(perna);
            }
        }
    }
}
