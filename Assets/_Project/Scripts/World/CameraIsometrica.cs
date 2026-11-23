using UnityEngine;

namespace ALotOfDamage.World
{
    // Câmera isométrica que só se move quando o alvo sai da zona morta central
    public class CameraIsometrica : MonoBehaviour
    {
        public Transform alvo;

        [Header("Ângulo")]
        public float inclinacao = 45f;
        public float rotacao = 45f;
        public float distancia = 18f;

        [Header("Zona morta (unidades do mundo no plano XZ)")]
        public Vector2 zonaMorta = new Vector2(4f, 2.5f);
        public float suavizacao = 0.25f;

        Vector3 foco;
        Vector3 velocidade;

        void Start()
        {
            if (alvo) foco = alvo.position;
            Posicionar(true);
        }

        void LateUpdate()
        {
            if (!alvo) return;

            // Diferença no espaço da câmera (direita / frente no chão)
            Quaternion plano = Quaternion.Euler(0f, rotacao, 0f);
            Vector3 local = Quaternion.Inverse(plano) * (alvo.position - foco);

            Vector3 desvio = Vector3.zero;
            if (local.x > zonaMorta.x) desvio.x = local.x - zonaMorta.x;
            else if (local.x < -zonaMorta.x) desvio.x = local.x + zonaMorta.x;
            if (local.z > zonaMorta.y) desvio.z = local.z - zonaMorta.y;
            else if (local.z < -zonaMorta.y) desvio.z = local.z + zonaMorta.y;

            foco += plano * desvio;
            foco.y = alvo.position.y;
            Posicionar(false);
        }

        void Posicionar(bool imediato)
        {
            Quaternion rot = Quaternion.Euler(inclinacao, rotacao, 0f);
            Vector3 desejado = foco - rot * Vector3.forward * distancia;
            transform.rotation = rot;
            transform.position = imediato ? desejado
                : Vector3.SmoothDamp(transform.position, desejado, ref velocidade, suavizacao);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(foco, Quaternion.Euler(0f, rotacao, 0f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(zonaMorta.x * 2, 0.1f, zonaMorta.y * 2));
        }
    }
}
