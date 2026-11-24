using UnityEngine;
using UnityEngine.EventSystems;

namespace ALotOfDamage.UI
{
    public class BotaoDeAtaque : MonoBehaviour, IPointerDownHandler
    {
        void Start()
        {
            var rt = (RectTransform)transform;
            var icone = KitDeInterface.Icone("Icone", rt, IconeDoHud.Espada);
            KitDeInterface.Ancorar((RectTransform)icone.transform, new Vector2(0.5f, 0.5f), Vector2.zero, rt.sizeDelta * 0.5f);
        }

        public void OnPointerDown(PointerEventData e) => ControlesDeToque.PedirAtaque();
    }
}
