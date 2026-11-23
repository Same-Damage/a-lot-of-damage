using UnityEngine;

namespace ALotOfDamage.World
{
    // Follower no mapa: acompanha o jogador a uma distância, voando com um leve sobe e desce
    public class SeguidorNoMapa : MonoBehaviour
    {
        public Transform alvo;
        public float distancia = 1.6f;
        public float altura = 1.8f;
        public float suavidade = 0.25f;
        public float amplitudeDoVoo = 0.15f;

        Vector3 velocidade;

        void Start()
        {
            if (!alvo)
            {
                var jogador = GameObject.FindWithTag("Player");
                if (jogador) alvo = jogador.transform;
            }
            if (alvo) transform.position = alvo.position + new Vector3(-distancia, altura, 0);
        }

        void LateUpdate()
        {
            if (!alvo) return;

            // Fica do lado de onde já está, a "distancia" do jogador
            Vector3 lado = transform.position - alvo.position;
            lado.y = 0;
            if (lado.sqrMagnitude < 0.01f) lado = Vector3.left;
            Vector3 destino = alvo.position + lado.normalized * distancia;
            destino.y = alvo.position.y + altura + Mathf.Sin(Time.time * 3f) * amplitudeDoVoo;

            transform.position = Vector3.SmoothDamp(transform.position, destino, ref velocidade, suavidade);
        }
    }
}
