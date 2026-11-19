using UnityEngine;
using UnityEngine.EventSystems;

namespace ALotOfDamage.UI
{
    public class AttackButton : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData e) => MobileControls.RequestAttack();
    }
}
