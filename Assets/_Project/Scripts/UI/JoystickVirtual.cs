using UnityEngine;
using UnityEngine.EventSystems;

namespace ALotOfDamage.UI
{
    // Joystick de toque: arrastar a partir do fundo move o pino
    public class JoystickVirtual : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform fundo;
        public RectTransform pino;

        public static Vector2 Valor { get; private set; }

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(fundo, e.position, e.pressEventCamera, out var local))
                return;

            float raio = fundo.rect.width * 0.5f;
            Vector2 v = Vector2.ClampMagnitude(local / raio, 1f);
            Valor = v;
            pino.anchoredPosition = v * raio;
        }

        public void OnPointerUp(PointerEventData e)
        {
            Valor = Vector2.zero;
            pino.anchoredPosition = Vector2.zero;
        }

        void OnDisable() => Valor = Vector2.zero;
    }
}
