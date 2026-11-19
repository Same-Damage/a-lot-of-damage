using ALotOfDamage.Core;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.World
{
    // Movimento relativo à câmera isométrica (WASD/setas/gamepad/joystick de toque)
    [RequireComponent(typeof(CharacterController))]
    public class MovimentoDoJogador : MonoBehaviour
    {
        public float velocidade = 6f;
        public float gravidade = -20f;
        public Transform camera_;

        CharacterController controle;
        float velocidadeVertical;

        void Awake()
        {
            controle = GetComponent<CharacterController>();
            if (!camera_ && Camera.main) camera_ = Camera.main.transform;

            // Volta ao ponto onde a batalha começou
            if (EstadoDoEncontro.TemPosicaoDeRetorno)
            {
                controle.enabled = false;
                transform.position = EstadoDoEncontro.PosicaoDeRetorno;
                controle.enabled = true;
            }
        }

        void Update()
        {
            Vector2 entrada = LerEntrada();

            Vector3 frente = camera_ ? camera_.forward : Vector3.forward;
            Vector3 direita = camera_ ? camera_.right : Vector3.right;
            frente.y = 0f; direita.y = 0f;
            frente.Normalize(); direita.Normalize();

            Vector3 movimento = Vector3.ClampMagnitude(frente * entrada.y + direita * entrada.x, 1f);

            velocidadeVertical = controle.isGrounded ? -1f : velocidadeVertical + gravidade * Time.deltaTime;
            controle.Move((movimento * velocidade + Vector3.up * velocidadeVertical) * Time.deltaTime);
        }

        static Vector2 LerEntrada()
        {
            Vector2 v = Vector2.zero;
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1;
                if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1;
                if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1;
                if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1;
            }
            if (Gamepad.current != null) v += Gamepad.current.leftStick.ReadValue();
            v += JoystickVirtual.Valor;
            return Vector2.ClampMagnitude(v, 1f);
        }
    }
}
