using UnityEngine;
using UnityEngine.EventSystems;

namespace ALotOfDamage.UI
{
    // Joystick de toque: arrastar a partir do fundo move o "knob"
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform background;
        public RectTransform knob;

        public static Vector2 Value { get; private set; }

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, e.position, e.pressEventCamera, out var local))
                return;

            float radius = background.rect.width * 0.5f;
            Vector2 v = Vector2.ClampMagnitude(local / radius, 1f);
            Value = v;
            knob.anchoredPosition = v * radius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            Value = Vector2.zero;
            knob.anchoredPosition = Vector2.zero;
        }

        void OnDisable() => Value = Vector2.zero;
    }
}
