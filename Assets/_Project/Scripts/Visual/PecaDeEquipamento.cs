using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Arma ou acessório rígido preso a um osso do herói. A arma mostra o item equipado no slot Arma:
    // some sem item, e com item ganha a cor da raridade no brilho de borda e na emissão do PersonagemPintado.
    public class PecaDeEquipamento : MonoBehaviour
    {
        [Tooltip("Arma da mão: aparece só com item no slot Arma. Acessórios (aljava) ficam sempre")]
        public bool ehArma;
        public float bordaSemBrilho = 0.5f;
        public float bordaNoMaximo = 3f;
        public float emissaoNoMaximo = 2.5f;

        static readonly int ForcaDaBorda = Shader.PropertyToID("_ForcaDaBorda");
        static readonly int CorDaBorda = Shader.PropertyToID("_CorDaBorda");
        static readonly int CorDaEmissao = Shader.PropertyToID("_CorDaEmissao");
        static readonly int ForcaDaEmissao = Shader.PropertyToID("_ForcaDaEmissao");

        Renderer[] renderers;
        MaterialPropertyBlock bloco;
        Color corAtual = Color.clear;
        float brilhoAtual = -1;

        // cor da raridade e brilho de 0 a quase 1 (RegrasDeAparencia.BrilhoDoItem)
        public void Mostrar(Color cor, float brilho)
        {
            if (cor == corAtual && Mathf.Approximately(brilho, brilhoAtual)) return;
            corAtual = cor;
            brilhoAtual = brilho;
            if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);
            bloco ??= new MaterialPropertyBlock();
            bloco.SetFloat(ForcaDaBorda, Mathf.Lerp(bordaSemBrilho, bordaNoMaximo, brilho));
            bloco.SetColor(CorDaBorda, Color.Lerp(Color.white, cor, 0.5f + 0.5f * brilho) * (1 + brilho));
            bloco.SetColor(CorDaEmissao, cor);
            bloco.SetFloat(ForcaDaEmissao, brilho * emissaoNoMaximo);
            foreach (var r in renderers) r.SetPropertyBlock(bloco);
        }
    }
}
