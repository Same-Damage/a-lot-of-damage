using UnityEngine;

namespace ALotOfDamage.Visual
{
    // O que o mapa e o combate pedem ao visual de um personagem, seja boneco de recorte 2D ou modelo 3D animado
    public abstract class PersonagemVisual : MonoBehaviour
    {
        // Chamado todo quadro pelo movimento: velocidade real no mundo, a máxima e a entrada do jogador
        public abstract void Mover(Vector3 velocidadeNoMundo, float velocidadeMaxima, Vector2 entrada);

        public abstract void Golpear(float velocidade = 1f);

        public abstract void LevarDano();
    }
}
