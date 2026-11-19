using UnityEngine;

namespace ALotOfDamage.World
{
    // Mantém o sprite virado para a câmera (estilo HD-2D), sem inclinar no eixo Y
    public class Billboard : MonoBehaviour
    {
        public bool lockVertical = true;
        Camera cam;

        void LateUpdate()
        {
            if (!cam) cam = Camera.main;
            if (!cam) return;

            if (lockVertical)
            {
                Vector3 f = cam.transform.forward;
                f.y = 0f;
                if (f.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(f);
            }
            else
            {
                transform.rotation = cam.transform.rotation;
            }
        }
    }
}
