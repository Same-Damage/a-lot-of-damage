using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.AI;

namespace ALotOfDamage.World
{
    // Monstro visível no mapa: vaga, persegue ao ver o jogador e inicia a batalha ao encostar.
    // Anda pelo NavMesh para desviar dos obstáculos.
    [RequireComponent(typeof(NavMeshAgent))]
    public class InimigoDoMapa : MonoBehaviour
    {
        public string idDoInimigo = "inimigo";
        public string nome = "Slime";
        public double multiplicadorDeVida = 1;

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

        void Start()
        {
            if (EstadoDoEncontro.Derrotados.Contains(idDoInimigo)) { Destroy(gameObject); return; }

            agente = GetComponent<NavMeshAgent>();
            agente.updateRotation = false;
            origem = transform.position;
            EscolherDestino();
            var j = GameObject.FindWithTag("Player");
            if (j) jogador = j.transform;
        }

        void Update()
        {
            if (!jogador || EstadoDoEncontro.Ativo)
            {
                if (agente.isOnNavMesh) agente.isStopped = true;
                return;
            }

            Vector3 ateJogador = jogador.position - transform.position;
            ateJogador.y = 0f;
            float distancia = ateJogador.magnitude;

            Perseguindo = RegrasDoEncontro.DevePerseguir(Perseguindo, distancia, raioDeVisao, raioDeDesistencia, EstadoDoEncontro.EmProtecao);

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

        public void Enfrentar(Vantagem vantagem)
        {
            if (jogador) EstadoDoEncontro.Iniciar(idDoInimigo, nome, multiplicadorDeVida, vantagem, jogador.position);
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
