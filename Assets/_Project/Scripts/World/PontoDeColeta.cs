using ALotOfDamage.Core;
using ALotOfDamage.UI;
using ALotOfDamage.Visual;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.World
{
    // Ponto de coleta no mapa (erva, cogumelo ou minério): o ataque do mapa golpeia; depois dos golpes da definição
    // entrega o material (ColetarRecurso), some e rebrota depois de um tempo. O modelo low-poly é feito por código.
    public class PontoDeColeta : MonoBehaviour
    {
        public string recursoId;
        public Material materialBase;   // URP Lit

        ColetarRecurso coletar;
        ICatalogo catalogo;
        DefinicaoDeRecurso recurso;
        Transform modelo;
        int golpes;
        float rebrotaEm = -1, tremorAte;

        public bool Disponivel => modelo && modelo.gameObject.activeSelf;

        [Inject]
        public void Construir(ColetarRecurso coletar, ICatalogo catalogo)
        {
            this.coletar = coletar;
            this.catalogo = catalogo;
        }

        void Start()
        {
            recurso = catalogo?.Recurso(recursoId);
            if (recurso == null) { enabled = false; return; }
            // pisa no chão da ilha, onde quer que o construtor tenha posto
            if (Physics.Raycast(transform.position + Vector3.up * 30, Vector3.down, out var chao, 80f))
                transform.position = chao.point;
            Montar();
        }

        public void Golpear()
        {
            if (!Disponivel) return;
            golpes++;
            tremorAte = Time.time + 0.15f;
            if (!RegrasDaColeta.Esgotou(recurso, golpes)) return;

            int quantidade = coletar.Executar(recursoId);
            Faixa.DoCanvas(KitDeInterface.Canvas()).Mostrar($"+{quantidade} {recurso.NomeDoMaterial}", 1.2f);
            modelo.gameObject.SetActive(false);
            rebrotaEm = Time.time + recurso.SegundosParaRebrotar;
        }

        void Update()
        {
            if (!modelo) return;
            if (!modelo.gameObject.activeSelf)
            {
                if (Time.time >= rebrotaEm)
                {
                    golpes = 0;
                    modelo.gameObject.SetActive(true);
                }
                return;
            }
            // treme a cada golpe e encolhe conforme se esgota
            float restante = 1 - 0.35f * golpes / Mathf.Max(1, recurso.Golpes);
            float tremor = Time.time < tremorAte ? Mathf.Sin(Time.time * 80f) * 0.08f : 0;
            modelo.localScale = Vector3.one * (restante + tremor);
        }

        void Montar()
        {
            ColorUtility.TryParseHtmlString(recurso.Cor, out var cor);
            var materiais = new[]
            {
                new Material(materialBase) { color = cor },
                new Material(materialBase) { color = recurso.Forma == FormaDoRecurso.Minerio ? new Color(0.45f, 0.45f, 0.5f) : new Color(0.85f, 0.8f, 0.65f) },
            };
            if (recurso.Forma == FormaDoRecurso.Minerio)
            {
                materiais[0].EnableKeyword("_EMISSION");
                materiais[0].SetColor("_EmissionColor", cor * 0.8f);
            }

            var m = new Malhas(2);
            switch (recurso.Forma)
            {
                case FormaDoRecurso.Erva:
                    for (int i = 0; i < 9; i++)
                    {
                        float a = i * 0.7f;
                        m.Lamina(0, new Vector3(Mathf.Cos(a) * 0.18f, 0, Mathf.Sin(a) * 0.18f), 0.12f, 0.45f + (i % 3) * 0.1f, a, 0.1f);
                    }
                    for (int i = 0; i < 3; i++)
                        m.Bolha(1, new Vector3(Mathf.Cos(i * 2.1f) * 0.15f, 0.55f, Mathf.Sin(i * 2.1f) * 0.15f), Vector3.one * 0.07f, 90 + i, 0.05f, 3, 5);
                    break;
                case FormaDoRecurso.Cogumelo:
                    for (int i = 0; i < 3; i++)
                    {
                        var c = new Vector3((i - 1) * 0.22f, 0, i == 1 ? 0.1f : -0.05f);
                        float h = i == 1 ? 0.45f : 0.3f;
                        m.Cilindro(1, c, 0.06f, 0.05f, h, 5);
                        m.Bolha(0, c + Vector3.up * h, new Vector3(0.2f, 0.11f, 0.2f) * (i == 1 ? 1.2f : 0.9f), 95 + i, 0.08f, 3, 7);
                    }
                    break;
                default:
                    m.Bolha(1, new Vector3(0, 0.25f, 0), new Vector3(0.5f, 0.35f, 0.45f), 99, 0.25f, 3, 6);
                    m.Cristal(0, new Vector3(0.05f, 0.3f, 0.1f), 0.12f, 0.6f, Quaternion.Euler(10, 0, 15));
                    m.Cristal(0, new Vector3(-0.2f, 0.25f, -0.05f), 0.09f, 0.45f, Quaternion.Euler(-15, 30, -20));
                    m.Cristal(0, new Vector3(0.22f, 0.2f, -0.1f), 0.08f, 0.35f, Quaternion.Euler(5, 60, 30));
                    break;
            }
            var go = new GameObject("Modelo", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = m.Criar(recurso.Id);
            go.GetComponent<MeshRenderer>().sharedMaterials = materiais;
            modelo = go.transform;
        }
    }
}
