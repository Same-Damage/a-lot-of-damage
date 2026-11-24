using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Follower low-poly provisório, feito por código pela família (humanoide, besta, ave, dragão, mítico, espírito, constructo)
    // e pelas cores de cada um, até chegar a arte. Raro ou melhor ganha uma joia flutuante na cor da raridade.
    public class FollowerLowPoly : MonoBehaviour
    {
        public Material materialBase;   // URP Lit

        const int Pele = 0, Detalhe = 1, Olho = 2, Pupila = 3, Brilho = 4, Partes = 5;

        public bool Andando { get; set; }
        public string IdMontado { get; private set; }

        // Quem voa fica no alto ao lado do herói; quem anda fica no chão
        public static bool Voa(FamiliaDoFollower f) =>
            f == FamiliaDoFollower.Ave || f == FamiliaDoFollower.Dragao || f == FamiliaDoFollower.Espirito;

        FamiliaDoFollower familia;
        Transform corpo, cabeca, cauda, joia, asaEsquerda, asaDireita, bracoEsquerdo, bracoDireito;
        readonly List<Transform> pernasA = new List<Transform>(), pernasB = new List<Transform>(), orbitas = new List<Transform>();
        Material[] materiais;
        readonly List<Object> criados = new List<Object>();
        Vector3 posicaoDoCorpo;

        public void Montar(DefinicaoDeFollower f)
        {
            Limpar();
            if (f == null) return;
            IdMontado = f.Id;
            familia = f.Familia;
            ColorUtility.TryParseHtmlString(f.Cor, out var cor);
            ColorUtility.TryParseHtmlString(f.CorSecundaria, out var cor2);
            ColorUtility.TryParseHtmlString(RegrasDosItens.CorDaRaridade(f.Raridade), out var corDaRaridade);
            bool espirito = familia == FamiliaDoFollower.Espirito;
            materiais = new[]
            {
                Novo(cor, espirito ? 0.9f : 0), Novo(cor2, espirito ? 1.2f : 0), Novo(new Color(0.95f, 0.95f, 0.9f)),
                Novo(new Color(0.06f, 0.05f, 0.05f)), Novo(corDaRaridade, 1.6f),
            };

            switch (familia)
            {
                case FamiliaDoFollower.Humanoide: Humanoide(); break;
                case FamiliaDoFollower.Besta: Quadrupede(false, false); break;
                case FamiliaDoFollower.Mitico: Quadrupede(true, true); break;
                case FamiliaDoFollower.Dragao: Quadrupede(true, true, dragao: true); break;
                case FamiliaDoFollower.Ave: Ave(); break;
                case FamiliaDoFollower.Espirito: Espirito(); break;
                default: Constructo(); break;
            }
            posicaoDoCorpo = corpo.localPosition;

            if (f.Raridade >= Raridade.Raro)
                joia = Peca("Joia", transform, new Vector3(0, AlturaDaJoia(), 0), m =>
                    m.Cristal(Brilho, Vector3.zero, 0.09f + 0.03f * (f.Raridade - Raridade.Raro), 0.3f, Quaternion.identity));

            transform.localScale = Vector3.one * Mathf.Max(0.3f, f.Escala);
        }

        float AlturaDaJoia() => familia switch
        {
            FamiliaDoFollower.Humanoide => 1.75f,
            FamiliaDoFollower.Constructo => 1.6f,
            FamiliaDoFollower.Espirito => 1.1f,
            FamiliaDoFollower.Ave => 1.0f,
            _ => 1.5f,
        };

        void Update()
        {
            if (!corpo) return;
            float t = Time.time;
            float passo = Andando ? Mathf.Sin(t * 9f) : 0;
            float respira = Mathf.Sin(t * 2.2f);

            corpo.localPosition = posicaoDoCorpo + new Vector3(0, Voa(familia) ? respira * 0.06f : Mathf.Abs(passo) * 0.04f + respira * 0.015f, 0);
            foreach (var p in pernasA) p.localRotation = Quaternion.Euler(passo * 30f, 0, 0);
            foreach (var p in pernasB) p.localRotation = Quaternion.Euler(-passo * 30f, 0, 0);
            if (bracoEsquerdo) bracoEsquerdo.localRotation = Quaternion.Euler(-passo * 25f, 0, 8);
            if (bracoDireito) bracoDireito.localRotation = Quaternion.Euler(passo * 25f, 0, -8);
            if (cabeca) cabeca.localRotation = Quaternion.Euler(respira * 3f, Mathf.Sin(t * 0.7f) * 10f, 0);
            if (cauda) cauda.localRotation = Quaternion.Euler(0, Mathf.Sin(t * 3f) * 18f, 0);

            // asas batem rápido em quem voa; em quem anda só se abrem devagar
            float batida = Voa(familia) ? Mathf.Sin(t * 10f) * 35f : Mathf.Sin(t * 1.5f) * 8f;
            if (asaEsquerda) asaEsquerda.localRotation = Quaternion.Euler(0, 0, 15 + batida);
            if (asaDireita) asaDireita.localRotation = Quaternion.Euler(0, 0, -15 - batida);

            for (int i = 0; i < orbitas.Count; i++)
            {
                float a = t * 2f + i * Mathf.PI * 2 / orbitas.Count;
                orbitas[i].localPosition = new Vector3(Mathf.Cos(a) * 0.55f, 0.55f + Mathf.Sin(a * 2) * 0.12f, Mathf.Sin(a) * 0.55f);
            }
            if (joia) joia.localRotation = Quaternion.Euler(0, t * 90f, 0);
        }

        void OnDestroy() => Limpar();

        void Limpar()
        {
            foreach (var o in criados) if (o) Destroy(o);
            criados.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            pernasA.Clear(); pernasB.Clear(); orbitas.Clear();
            corpo = cabeca = cauda = joia = asaEsquerda = asaDireita = bracoEsquerdo = bracoDireito = null;
            IdMontado = null;
        }

        Material Novo(Color cor, float brilhoProprio = 0)
        {
            var m = new Material(materialBase) { color = cor };
            m.SetFloat("_Smoothness", 0.3f);
            if (brilhoProprio > 0)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", cor * brilhoProprio);
            }
            criados.Add(m);
            return m;
        }

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
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterials = materiais;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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

        // ---------- famílias ----------

        // Companheiro de pé: tronco, cabeça grande (proporção chibi), braços e pernas
        void Humanoide()
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 0.75f, 0), m =>
            {
                m.Bolha(Pele, new Vector3(0, 0.15f, 0), new Vector3(0.28f, 0.34f, 0.2f), 41, 0.08f, 4, 7);
                m.Bolha(Detalhe, new Vector3(0, -0.12f, 0), new Vector3(0.3f, 0.1f, 0.22f), 42, 0.05f, 3, 7);   // cinto
            });
            cabeca = Peca("Cabeca", corpo, new Vector3(0, 0.62f, 0), m =>
            {
                m.Bolha(Detalhe, new Vector3(0, 0.12f, 0), new Vector3(0.26f, 0.27f, 0.25f), 43, 0.06f, 5, 8);
                m.Bolha(Pele, new Vector3(0, 0.28f, -0.03f), new Vector3(0.28f, 0.14f, 0.27f), 44, 0.12f, 3, 8);   // cabelo ou elmo
                Olhos(m, 0.09f, 0.1f, 0.21f, 0.055f);
            });
            Transform Braco(float lado) => Peca("Braco", corpo, new Vector3(lado * 0.33f, 0.38f, 0), m =>
                m.Cilindro(Pele, new Vector3(0, -0.42f, 0), 0.07f, 0.09f, 0.42f, 5));
            bracoEsquerdo = Braco(-1);
            bracoDireito = Braco(1);
            for (int lado = -1; lado <= 1; lado += 2)
            {
                var perna = Peca("Perna", corpo, new Vector3(lado * 0.13f, -0.18f, 0), m =>
                    m.Cilindro(Detalhe, new Vector3(0, -0.55f, 0), 0.1f, 0.09f, 0.55f, 5));
                (lado < 0 ? pernasA : pernasB).Add(perna);
            }
        }

        // Besta de quatro patas; míticos e dragões ganham asas, chifre e cauda longa
        void Quadrupede(bool asas, bool chifre, bool dragao = false)
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 0.7f, 0), m =>
                m.Bolha(Pele, Vector3.zero, new Vector3(0.34f, 0.32f, 0.62f), 51, 0.12f, 4, 7));

            float pescoco = dragao ? 0.45f : 0.2f;
            cabeca = Peca("Cabeca", corpo, new Vector3(0, 0.22f + pescoco * 0.5f, 0.58f), m =>
            {
                if (dragao) m.Cilindro(Pele, new Vector3(0, -0.35f, -0.12f), 0.13f, 0.1f, 0.4f, 5);
                m.Bolha(Pele, new Vector3(0, 0.1f, 0.1f), new Vector3(0.26f, 0.24f, 0.3f), 52, 0.1f, 4, 7);
                m.Bolha(Detalhe, new Vector3(0, 0.03f, 0.38f), new Vector3(0.14f, 0.11f, 0.2f), 53, 0.08f, 3, 6);
                for (int lado = -1; lado <= 1; lado += 2)
                    m.Cilindro(chifre ? Detalhe : Pele, new Vector3(lado * 0.13f, 0.27f, 0.02f), chifre ? 0.05f : 0.08f, 0, chifre ? 0.32f : 0.2f, 4);
                if (chifre && !dragao) m.Cilindro(Brilho, new Vector3(0, 0.25f, 0.2f), 0.04f, 0, 0.35f, 4);   // chifre central
                Olhos(m, 0.11f, 0.17f, 0.3f, 0.055f);
            });

            cauda = Peca("Cauda", corpo, new Vector3(0, 0.12f, -0.58f), m =>
                m.Bolha(dragao ? Pele : Detalhe, new Vector3(0, 0.05f, dragao ? -0.45f : -0.22f), new Vector3(0.1f, 0.1f, dragao ? 0.55f : 0.28f), 54, 0.1f, 3, 6));

            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -0.2f : 0.2f, z = i < 2 ? 0.38f : -0.38f;
                var perna = Peca("Perna", corpo, new Vector3(x, -0.12f, z), m =>
                    m.Cilindro(Detalhe, new Vector3(0, -0.58f, 0), 0.08f, 0.11f, 0.58f, 5));
                (i == 0 || i == 3 ? pernasA : pernasB).Add(perna);
            }

            if (asas) Asas(new Vector3(0, 0.25f, 0.05f), dragao ? 0.95f : 0.7f);
        }

        void Ave()
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 0.5f, 0), m =>
            {
                m.Bolha(Pele, Vector3.zero, new Vector3(0.24f, 0.26f, 0.34f), 61, 0.08f, 4, 7);
                m.Bolha(Detalhe, new Vector3(0, -0.06f, 0.08f), new Vector3(0.18f, 0.2f, 0.22f), 62, 0.05f, 3, 6);   // peito
            });
            cabeca = Peca("Cabeca", corpo, new Vector3(0, 0.26f, 0.26f), m =>
            {
                m.Bolha(Pele, Vector3.zero, Vector3.one * 0.17f, 63, 0.06f, 4, 7);
                m.Cristal(Brilho, new Vector3(0, -0.03f, 0.12f), 0.05f, 0.18f, Quaternion.Euler(90, 0, 0));   // bico
                Olhos(m, 0.08f, 0.04f, 0.12f, 0.045f);
            });
            cauda = Peca("Cauda", corpo, new Vector3(0, -0.02f, -0.3f), m =>
                FolhaDupla(m, Detalhe, new Vector3(-0.06f, 0, 0), new Vector3(0.06f, 0, 0), new Vector3(0.14f, 0.05f, -0.32f), new Vector3(-0.14f, 0.05f, -0.32f)));
            Asas(new Vector3(0, 0.08f, 0), 0.6f);
        }

        void Espirito()
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 0.6f, 0), m =>
            {
                m.Bolha(Pele, Vector3.zero, Vector3.one * 0.3f, 71, 0.04f, 5, 9);
                m.Bolha(Detalhe, Vector3.zero, Vector3.one * 0.2f, 72, 0.02f, 4, 8);
                Olhos(m, 0.09f, 0.05f, 0.25f, 0.05f);
            });
            for (int i = 0; i < 3; i++)
            {
                var o = Peca("Orbita", transform, Vector3.zero, m => m.Bolha(Detalhe, Vector3.zero, Vector3.one * 0.07f, 73 + i, 0.02f, 3, 6));
                orbitas.Add(o);
            }
        }

        void Constructo()
        {
            corpo = Peca("Corpo", transform, new Vector3(0, 0.8f, 0), m =>
            {
                m.Bolha(Pele, new Vector3(0, 0.2f, 0), new Vector3(0.38f, 0.36f, 0.3f), 81, 0.2f, 3, 5);
                m.Bolha(Pele, new Vector3(0, 0.72f, 0.04f), new Vector3(0.22f, 0.2f, 0.2f), 82, 0.15f, 3, 5);
                for (int lado = -1; lado <= 1; lado += 2)
                    m.Bolha(Brilho, new Vector3(lado * 0.08f, 0.75f, 0.2f), Vector3.one * 0.045f, 83, 0, 3, 5);
                m.Cristal(Detalhe, new Vector3(0, 0.25f, 0.3f), 0.09f, 0.25f, Quaternion.Euler(70, 0, 0));
            });
            Transform Braco(float lado) => Peca("Braco", corpo, new Vector3(lado * 0.45f, 0.42f, 0), m =>
            {
                m.Bolha(Detalhe, new Vector3(0, -0.3f, 0), new Vector3(0.13f, 0.3f, 0.13f), 84, 0.15f, 3, 5);
                m.Bolha(Pele, new Vector3(0, -0.66f, 0.03f), new Vector3(0.16f, 0.14f, 0.16f), 85, 0.2f, 3, 5);
            });
            bracoEsquerdo = Braco(-1);
            bracoDireito = Braco(1);
            for (int lado = -1; lado <= 1; lado += 2)
            {
                var perna = Peca("Perna", corpo, new Vector3(lado * 0.2f, -0.1f, 0), m =>
                    m.Cilindro(Detalhe, new Vector3(0, -0.7f, 0), 0.15f, 0.13f, 0.7f, 5));
                (lado < 0 ? pernasA : pernasB).Add(perna);
            }
        }

        // Asa: placa fina de duas faces saindo para o lado (sinal −1 esquerda, +1 direita), presa no ombro
        void Asas(Vector3 base_, float tamanho)
        {
            Transform Asa(float sinal) => Peca(sinal < 0 ? "AsaEsquerda" : "AsaDireita", corpo, base_ + new Vector3(sinal * 0.15f, 0, 0), m =>
                FolhaDupla(m, Detalhe,
                    new Vector3(0, 0, 0.18f), new Vector3(sinal * tamanho, 0.15f, 0.05f),
                    new Vector3(sinal * tamanho * 0.8f, 0.05f, -0.3f), new Vector3(0, 0, -0.2f)));
            asaEsquerda = Asa(-1);
            asaDireita = Asa(1);
        }

        static void FolhaDupla(Malhas m, int parte, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            m.Quad(parte, a, b, c, d);
            m.Quad(parte, d, c, b, a);
        }
    }
}
