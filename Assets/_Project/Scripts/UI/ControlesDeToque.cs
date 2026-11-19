using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.UI
{
    // Liga/desliga os controles de toque e guarda o pedido de ataque
    public class ControlesDeToque : MonoBehaviour
    {
        [Tooltip("Mostra os controles no editor/PC para testar com o mouse")]
        public bool mostrarNoDesktop = true;

        static bool ataquePedido;

        void Awake()
        {
            bool toque = Application.isMobilePlatform || Touchscreen.current != null;
            gameObject.SetActive(toque || mostrarNoDesktop);
        }

        public static void PedirAtaque() => ataquePedido = true;

        public static bool ConsumirAtaque()
        {
            bool r = ataquePedido;
            ataquePedido = false;
            return r;
        }
    }
}
