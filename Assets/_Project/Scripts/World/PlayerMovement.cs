using ALotOfDamage.Core;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.World
{
    // Movimento relativo à câmera isométrica (WASD/setas/gamepad/joystick de toque)
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 6f;
        public float gravity = -20f;
        public Transform cameraTransform;

        CharacterController controller;
        float verticalVelocity;

        public Vector3 LastMoveDirection { get; private set; }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;

            // Volta ao ponto onde a batalha começou
            if (EncounterState.HasReturnPosition)
            {
                controller.enabled = false;
                transform.position = EncounterState.ReturnPosition;
                controller.enabled = true;
            }
        }

        void Update()
        {
            Vector2 input = ReadInput();

            Vector3 forward = cameraTransform ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform ? cameraTransform.right : Vector3.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();

            Vector3 move = Vector3.ClampMagnitude(forward * input.y + right * input.x, 1f);
            if (move.sqrMagnitude > 0.01f) LastMoveDirection = move;

            verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + gravity * Time.deltaTime;
            controller.Move((move * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        static Vector2 ReadInput()
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
            v += VirtualJoystick.Value;
            return Vector2.ClampMagnitude(v, 1f);
        }
    }
}
