using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Sombra oval no chão embaixo do personagem 2D, para ele "pisar" no cenário 3D
    // (os personagens pintados não projetam sombra de verdade). Fica deitada no chão,
    // fora do billboard, então não gira junto com o desenho.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SombraDeContato : MonoBehaviour
    {
        const int Tamanho = 64;
        static Sprite sprite;

        public float largura = 1.1f;
        public float profundidade = 0.55f;
        [Range(0, 1)] public float opacidade = 0.45f;

        void Awake()
        {
            var sr = GetComponent<SpriteRenderer>();
            sr.sprite = Gerar();
            sr.color = new Color(0, 0, 0, opacidade);
            sr.sortingOrder = -100;
            sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            transform.localRotation = Quaternion.Euler(90, 0, 0);
            transform.localScale = new Vector3(largura, profundidade, 1);
        }

        // Degradê radial gerado uma vez e compartilhado por todas as sombras
        static Sprite Gerar()
        {
            if (sprite) return sprite;
            var tex = new Texture2D(Tamanho, Tamanho, TextureFormat.Alpha8, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[Tamanho * Tamanho];
            float meio = (Tamanho - 1) / 2f;
            for (int y = 0; y < Tamanho; y++)
            for (int x = 0; x < Tamanho; x++)
            {
                float d = Mathf.Sqrt((x - meio) * (x - meio) + (y - meio) * (y - meio)) / meio;
                byte a = (byte)(Mathf.Clamp01(1 - d * d) * 255);
                pixels[y * Tamanho + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            sprite = Sprite.Create(tex, new Rect(0, 0, Tamanho, Tamanho), new Vector2(0.5f, 0.5f), Tamanho);
            return sprite;
        }
    }
}
