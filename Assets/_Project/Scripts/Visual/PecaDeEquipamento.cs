using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Arma ou acessório rígido preso a um osso do herói; o brilho mostra o poder da forja no brilho de borda do PersonagemPintado
    public class PecaDeEquipamento : MonoBehaviour
    {
        public Color corSemForja = new Color(1f, 0.86f, 0.55f);
        public Color corNoMaximo = new Color(1f, 0.95f, 0.6f);
        public float bordaSemForja = 0.5f;
        public float bordaNoMaximo = 3f;

        static readonly int ForcaDaBorda = Shader.PropertyToID("_ForcaDaBorda");
        static readonly int CorDaBorda = Shader.PropertyToID("_CorDaBorda");

        Renderer[] renderers;
        MaterialPropertyBlock bloco;
        float brilhoAtual = -1;

        // 0 sem forja, perto de 1 muito forjada (RegrasDeAparencia.BrilhoDaArma)
        public float Brilho
        {
            set
            {
                if (Mathf.Approximately(value, brilhoAtual)) return;
                brilhoAtual = value;
                if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);
                bloco ??= new MaterialPropertyBlock();
                bloco.SetFloat(ForcaDaBorda, Mathf.Lerp(bordaSemForja, bordaNoMaximo, value));
                bloco.SetColor(CorDaBorda, Color.Lerp(corSemForja, corNoMaximo, value) * (1 + value));
                foreach (var r in renderers) r.SetPropertyBlock(bloco);
            }
        }
    }
}
