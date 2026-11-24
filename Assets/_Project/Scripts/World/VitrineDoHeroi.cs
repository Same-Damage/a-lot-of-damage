using ALotOfDamage.UI;
using ALotOfDamage.Visual;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Herói em 3D dentro da janela de inventário: uma cópia do herói ativo, num estúdio longe do mapa,
    // filmada por uma câmera própria numa textura. A câmera só liga com a janela aberta.
    public class VitrineDoHeroi : MonoBehaviour
    {
        public HeroisDaCena herois;
        public JanelaDeInventario janela;
        public BrilhoDasArmas brilho;
        public Vector3 estudio = new Vector3(0, -500, 0);
        public float alturaDaCamera = 1.25f, distancia = 4.6f, campoDeVisao = 30f;

        Camera cameraDaVitrine;
        RenderTexture textura;
        GameObject copia;
        PersonagemVisual copiaDe;
        int versaoCopiada = -1;
        float giro = 180;   // de frente para a câmera

        void Start()
        {
            textura = new RenderTexture(512, 768, 24) { name = "Vitrine do herói" };
            var go = new GameObject("Câmera da vitrine");
            go.transform.SetParent(transform, false);
            cameraDaVitrine = go.AddComponent<Camera>();
            cameraDaVitrine.targetTexture = textura;
            cameraDaVitrine.clearFlags = CameraClearFlags.SolidColor;
            cameraDaVitrine.backgroundColor = Color.clear;   // fundo transparente: o herói aparece sobre o couro da janela
            cameraDaVitrine.fieldOfView = campoDeVisao;
            cameraDaVitrine.nearClipPlane = 0.1f;
            cameraDaVitrine.farClipPlane = distancia + 3;
            cameraDaVitrine.transform.position = estudio + new Vector3(0, alturaDaCamera, -distancia);
            cameraDaVitrine.transform.LookAt(estudio + Vector3.up * alturaDaCamera);
            cameraDaVitrine.enabled = false;

            janela.TexturaDaVitrine = textura;
            janela.AoGirarVitrine += graus => giro += graus;
        }

        void Update()
        {
            bool aberta = janela.EstaAberta;
            cameraDaVitrine.enabled = aberta;
            if (!aberta) return;

            var ativo = HeroiAtivo();
            // copia de novo quando o herói ou a arma equipada mudam
            if (ativo != copiaDe || (brilho && brilho.Versao != versaoCopiada)) Copiar(ativo);
            if (copia) copia.transform.rotation = Quaternion.Euler(0, giro, 0);
        }

        PersonagemVisual HeroiAtivo()
        {
            foreach (var h in herois.herois) if (h && h.gameObject.activeInHierarchy) return h;
            return null;
        }

        // A cópia leva o modelo, as armas e o Animator (fica na animação parada); sai o que mexe no herói de verdade
        void Copiar(PersonagemVisual heroi)
        {
            if (copia) Destroy(copia);
            copiaDe = heroi;
            if (!heroi) return;
            copia = Instantiate(heroi.gameObject, estudio, Quaternion.identity);
            copia.name = "Herói da vitrine";
            copia.transform.localScale = heroi.transform.lossyScale;
            foreach (var p in copia.GetComponentsInChildren<PersonagemVisual>()) Destroy(p);
            if (brilho)
            {
                brilho.AplicarEm(copia);
                versaoCopiada = brilho.Versao;
            }
        }

        void OnDestroy()
        {
            if (textura) textura.Release();
        }
    }
}
