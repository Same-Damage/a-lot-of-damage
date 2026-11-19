using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Monstro visível no mapa: vaga, persegue ao ver o jogador e inicia a batalha ao encostar
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

        void Start()
        {
            if (EstadoDoEncontro.Derrotados.Contains(idDoInimigo)) { Destroy(gameObject); return; }

            origem = transform.position;
            EscolherDestino();
            var j = GameObject.FindWithTag("Player");
            if (j) jogador = j.transform;
        }

        void Update()
        {
            if (!jogador || EstadoDoEncontro.Ativo) return;

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
            Vector3 d = destino - transform.position; d.y = 0f;
            if (d.sqrMagnitude < 0.05f) { espera = Random.Range(1f, 3f); EscolherDestino(); }
        }

        void MoverPara(Vector3 alvo, float velocidade)
        {
            alvo.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, alvo, velocidade * Time.deltaTime);
        }

        void EscolherDestino()
        {
            Vector2 r = Random.insideUnitCircle * raioDeVagar;
            destino = origem + new Vector3(r.x, 0f, r.y);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, raioDeVisao);
        }
    }
}
