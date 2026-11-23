using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.Visual;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace ALotOfDamage.World
{
    // Ponto de monstro no mapa: o tipo vem da região atual da campanha, então o mapa muda conforme se avança.
    // O monstro vaga, persegue ao ver o jogador e inicia a batalha ao encostar, andando pelo NavMesh.
    [RequireComponent(typeof(NavMeshAgent))]
    public class InimigoDoMapa : MonoBehaviour
    {
        FluxoDeEncontro encontro;
        ConsultarCampanha consultarCampanha;
        ICatalogo catalogo;

        [Inject]
        public void Construir(FluxoDeEncontro encontro, ConsultarCampanha consultarCampanha, ICatalogo catalogo)
        {
            this.encontro = encontro;
            this.consultarCampanha = consultarCampanha;
            this.catalogo = catalogo;
        }

        public string idDoInimigo = "inimigo";
        public int indiceDoPonto;
        [Tooltip("Ponto do chefe: só aparece nos estágios de chefe")]
        public bool pontoDoChefe;
        public MonstroLowPoly modelo;

        public string Tipo { get; private set; }

        [Header("Movimento")]
        public float raioDeVagar = 5f;
        public float velocidadeVagando = 1.5f;
        public float velocidadePerseguindo = 4.5f;

        [Header("Detecção")]
        public float raioDeVisao = 6f;
        public float raioDeDesistencia = 10f;
        public float raioDeToque = 1.1f;

        public bool Perseguindo { get; private set; }

        Transform jogador;
        Vector3 origem, destino;
        float espera;
        NavMeshAgent agente;
        bool escondido;
        Renderer[] visuais;

        void Start()
        {
            agente = GetComponent<NavMeshAgent>();
            agente.updateRotation = false;
            origem = transform.position;
            EscolherDestino();
            var j = GameObject.FindWithTag("Player");
            if (j) jogador = j.transform;
            var campanha = consultarCampanha.Executar();
            Tipo = RegrasDaCampanha.MonstroDoPonto(campanha.Regiao, indiceDoPonto, pontoDoChefe);
            var monstro = catalogo.Monstro(Tipo);
            if (monstro == null || (pontoDoChefe && !campanha.EstagioDeChefe))
            {
                gameObject.SetActive(false);
                return;
            }
            Aparencia(monstro);
            visuais = GetComponentsInChildren<Renderer>();
            Mostrar(!encontro.EstaDerrotado(idDoInimigo));
        }

        void Aparencia(DefinicaoDeMonstro m)
        {
            float escala = m.Escala * (pontoDoChefe ? 1.6f : 1f);
            transform.localScale = Vector3.one * escala;
            agente.baseOffset = escala;
            if (!ColorUtility.TryParseHtmlString(m.Cor, out var cor)) cor = Color.magenta;
            if (pontoDoChefe) cor = Color.Lerp(cor, new Color(0.7f, 0.1f, 0.1f), 0.5f);
            if (modelo) modelo.Montar(m.Forma, cor);
            name = (pontoDoChefe ? "Chefe " : "") + m.Nome;
        }

        // Derrotado fica escondido até renascer
        void Mostrar(bool visivel)
        {
            escondido = !visivel;
            foreach (var r in visuais) r.enabled = visivel;
        }

        void Update()
        {
            if (escondido)
            {
                if (encontro.EstaDerrotado(idDoInimigo)) { Parar(); return; }
                Mostrar(true);
            }
            if (modelo)
            {
                modelo.Andando = agente.velocity.sqrMagnitude > 0.04f;
                modelo.OlharPara(agente.velocity);
            }
            if (!jogador || encontro.Ativo) { Parar(); return; }

            Vector3 ateJogador = jogador.position - transform.position;
            ateJogador.y = 0f;
            float distancia = ateJogador.magnitude;

            Perseguindo = RegrasDoEncontro.DevePerseguir(Perseguindo, distancia, raioDeVisao, raioDeDesistencia, encontro.EmProtecao);

            if (Perseguindo)
            {
                MoverPara(jogador.position, velocidadePerseguindo);
                if (distancia < raioDeToque) Enfrentar(RegrasDoEncontro.AoSerAlcancado());
            }
            else
            {
                Vagar();
            }
        }

        public bool Disponivel => !escondido;

        public void Enfrentar(Vantagem vantagem)
        {
            if (jogador && !escondido) encontro.Iniciar(idDoInimigo, Tipo, pontoDoChefe, vantagem, jogador.position);
        }

        void Vagar()
        {
            if (espera > 0) { espera -= Time.deltaTime; return; }
            MoverPara(destino, velocidadeVagando);
            if (agente.isOnNavMesh && !agente.pathPending && agente.remainingDistance < 0.3f)
            {
                espera = Random.Range(1f, 3f);
                EscolherDestino();
            }
        }

        void Parar()
        {
            if (agente.isOnNavMesh) agente.isStopped = true;
        }

        void MoverPara(Vector3 alvo, float velocidade)
        {
            if (!agente.isOnNavMesh) return;
            agente.isStopped = false;
            agente.speed = velocidade;
            agente.SetDestination(alvo);
        }

        void EscolherDestino()
        {
            Vector2 r = Random.insideUnitCircle * raioDeVagar;
            destino = origem + new Vector3(r.x, 0f, r.y);
            // Ponto sorteado em cima de um obstáculo vira o ponto andável mais próximo
            if (NavMesh.SamplePosition(destino, out var ponto, raioDeVagar, NavMesh.AllAreas)) destino = ponto.position;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, raioDeVisao);
        }
    }
}
