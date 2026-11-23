using ALotOfDamage.Core;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.World
{
    // Pinta o chão com a cor da região atual da campanha: o mapa muda conforme se avança
    public class AparenciaDaRegiao : MonoBehaviour
    {
        ConsultarCampanha consultarCampanha;

        [Inject]
        public void Construir(ConsultarCampanha consultarCampanha) => this.consultarCampanha = consultarCampanha;

        void Start()
        {
            var regiao = consultarCampanha.Executar().Regiao;
            if (regiao == null || !ColorUtility.TryParseHtmlString(regiao.CorDoChao, out var cor)) return;
            var bloco = new MaterialPropertyBlock();
            bloco.SetColor("_BaseColor", cor);
            GetComponent<Renderer>().SetPropertyBlock(bloco);
        }
    }
}
