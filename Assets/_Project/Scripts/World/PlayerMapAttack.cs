using ALotOfDamage.Core;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.World
{
    // Ataque no mapa: acertar o monstro antes de ser visto dá ataque preventivo
    public class PlayerMapAttack : MonoBehaviour
    {
        public float range = 2f;
        public float cooldown = 0.4f;

        float nextAttack;
        float swingTimer;

        void Update()
        {
            if (EncounterState.Active) return;
            if (swingTimer > 0) swingTimer -= Time.deltaTime;

            if (AttackPressed() && Time.time >= nextAttack)
            {
                nextAttack = Time.time + cooldown;
                swingTimer = 0.15f;
                TryHit();
            }
        }

        static bool AttackPressed()
        {
            var k = Keyboard.current;
            if (k != null && (k.fKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)) return true;
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) return true;
            return MobileControls.ConsumeAttack();
        }

        void TryHit()
        {
            MapEnemy best = null;
            float bestDist = range;
            foreach (var e in FindObjectsByType<MapEnemy>(FindObjectsSortMode.None))
            {
                Vector3 d = e.transform.position - transform.position; d.y = 0f;
                if (d.magnitude < bestDist) { bestDist = d.magnitude; best = e; }
            }
            if (best) best.Engage(best.IsChasing ? Advantage.Normal : Advantage.Preemptive);
        }

        void OnGUI()
        {
            var st = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(20, 20, 700, 30),
                $"Ouro {NumberFormatter.Format(PlayerProgress.Gold)}   Dano x{NumberFormatter.Format(PlayerProgress.DamageMultiplier)}   Vitórias {PlayerProgress.Kills}", st);
            GUI.Label(new Rect(20, 50, 700, 30), "F / Espaço: atacar (acerte antes de ser visto = ataque preventivo)",
                new GUIStyle(st) { fontSize = 14, fontStyle = FontStyle.Normal });
            if (EncounterState.InGrace)
                GUI.Label(new Rect(20, 75, 400, 30), "Protegido...", new GUIStyle(st) { fontSize = 14 });
            if (swingTimer > 0)
                GUI.Label(new Rect(Screen.width * 0.5f - 30, Screen.height * 0.5f - 60, 100, 40), "SWING!", st);
        }
    }
}
