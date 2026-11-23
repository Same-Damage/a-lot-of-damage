using UnityEngine;
using UnityEngine.EventSystems;

namespace ALotOfDamage.UI
{
    public class BotaoDeAtaque : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData e) => ControlesDeToque.PedirAtaque();
    }
}
