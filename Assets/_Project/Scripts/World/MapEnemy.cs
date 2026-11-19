using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.World
{
    // Monstro visível no mapa: vaga, persegue ao ver o jogador e inicia a batalha ao encostar
    public class MapEnemy : MonoBehaviour
    {
        public string enemyId = "inimigo";
        public string displayName = "Slime";
        public double hpMultiplier = 1;

        [Header("Movimento")]
        public float wanderRadius = 5f;
        public float wanderSpeed = 1.5f;
        public float chaseSpeed = 4.5f;

        [Header("Detecção")]
        public float detectRadius = 6f;
        public float giveUpRadius = 10f;
        public float touchRadius = 1.1f;

        public bool IsChasing { get; private set; }

        Transform player;
        Vector3 home, wanderTarget;
        float waitTimer;

        void Start()
        {
            if (EncounterState.Defeated.Contains(enemyId)) { Destroy(gameObject); return; }

            home = transform.position;
            PickWanderTarget();
            var p = GameObject.FindWithTag("Player");
            if (p) player = p.transform;
        }

        void Update()
        {
            if (!player || EncounterState.Active) return;

            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;

            if (!IsChasing && dist < detectRadius && !EncounterState.InGrace) IsChasing = true;
            else if (IsChasing && dist > giveUpRadius) IsChasing = false;

            if (IsChasing)
            {
                MoveTowards(player.position, chaseSpeed);
                if (dist < touchRadius) Engage(Advantage.Ambush);
            }
            else
            {
                Wander();
            }
        }

        public void Engage(Advantage advantage)
        {
            if (player) EncounterState.Begin(enemyId, displayName, hpMultiplier, advantage, player.position);
        }

        void Wander()
        {
            if (waitTimer > 0) { waitTimer -= Time.deltaTime; return; }
            MoveTowards(wanderTarget, wanderSpeed);
            Vector3 d = wanderTarget - transform.position; d.y = 0f;
            if (d.sqrMagnitude < 0.05f) { waitTimer = Random.Range(1f, 3f); PickWanderTarget(); }
        }

        void MoveTowards(Vector3 target, float speed)
        {
            target.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }

        void PickWanderTarget()
        {
            Vector2 r = Random.insideUnitCircle * wanderRadius;
            wanderTarget = home + new Vector3(r.x, 0f, r.y);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
    }
}
