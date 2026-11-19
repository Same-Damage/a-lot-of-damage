using UnityEngine;

namespace ALotOfDamage.World
{
    // Câmera isométrica que só se move quando o alvo sai da zona morta central
    public class IsoCameraDeadZone : MonoBehaviour
    {
        public Transform target;

        [Header("Ângulo")]
        public float pitch = 45f;
        public float yaw = 45f;
        public float distance = 18f;

        [Header("Zona morta (unidades do mundo no plano XZ)")]
        public Vector2 deadZone = new Vector2(4f, 2.5f);
        public float smoothTime = 0.25f;

        Vector3 focus;
        Vector3 velocity;

        void Start()
        {
            if (target) focus = target.position;
            ApplyTransform(true);
        }

        void LateUpdate()
        {
            if (!target) return;

            // Diferença no espaço da câmera (direita / frente no chão)
            Quaternion flat = Quaternion.Euler(0f, yaw, 0f);
            Vector3 local = Quaternion.Inverse(flat) * (target.position - focus);

            Vector3 shift = Vector3.zero;
            if (local.x > deadZone.x) shift.x = local.x - deadZone.x;
            else if (local.x < -deadZone.x) shift.x = local.x + deadZone.x;
            if (local.z > deadZone.y) shift.z = local.z - deadZone.y;
            else if (local.z < -deadZone.y) shift.z = local.z + deadZone.y;

            focus += flat * shift;
            focus.y = target.position.y;
            ApplyTransform(false);
        }

        void ApplyTransform(bool instant)
        {
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desired = focus - rot * Vector3.forward * distance;
            transform.rotation = rot;
            transform.position = instant ? desired
                : Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(focus, Quaternion.Euler(0f, yaw, 0f), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(deadZone.x * 2, 0.1f, deadZone.y * 2));
        }
    }
}
