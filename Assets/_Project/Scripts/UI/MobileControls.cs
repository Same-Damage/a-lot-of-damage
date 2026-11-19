using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.UI
{
    // Liga/desliga os controles de toque e guarda o pedido de ataque
    public class MobileControls : MonoBehaviour
    {
        [Tooltip("Mostra os controles no editor/PC para testar com o mouse")]
        public bool showOnDesktop = true;

        static bool attackRequested;

        void Awake()
        {
            bool touch = Application.isMobilePlatform || Touchscreen.current != null;
            gameObject.SetActive(touch || showOnDesktop);
        }

        public static void RequestAttack() => attackRequested = true;

        public static bool ConsumeAttack()
        {
            bool r = attackRequested;
            attackRequested = false;
            return r;
        }
    }
}
