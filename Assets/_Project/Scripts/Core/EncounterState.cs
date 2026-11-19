using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALotOfDamage.Core
{
    public enum Advantage { Normal, Preemptive, Ambush }

    // Dados que passam do mapa para a batalha e voltam
    public static class EncounterState
    {
        public const string MapScene = "Mapa";
        public const string BattleScene = "Batalha";

        public static bool Active { get; private set; }
        public static string EnemyId { get; private set; }
        public static string EnemyName { get; private set; }
        public static double HpMultiplier { get; private set; } = 1;
        public static Advantage Advantage { get; private set; }

        public static bool HasReturnPosition { get; private set; }
        public static Vector3 ReturnPosition { get; private set; }

        // Tempo sem emboscada ao voltar para o mapa
        public static float GraceUntil { get; private set; }

        public static readonly HashSet<string> Defeated = new HashSet<string>();

        public static void Begin(string enemyId, string enemyName, double hpMultiplier, Advantage advantage, Vector3 playerPosition)
        {
            if (Active) return;
            Active = true;
            EnemyId = enemyId;
            EnemyName = enemyName;
            HpMultiplier = hpMultiplier;
            Advantage = advantage;
            ReturnPosition = playerPosition;
            HasReturnPosition = true;
            SceneManager.LoadScene(BattleScene);
        }

        public static void Finish(bool won)
        {
            if (won && EnemyId != null) Defeated.Add(EnemyId);
            Active = false;
            GraceUntil = Time.realtimeSinceStartup + 2.5f;
            SceneManager.LoadScene(MapScene);
        }

        public static bool InGrace => Time.realtimeSinceStartup < GraceUntil;
    }

    // Progresso do jogador entre cenas (provisório até existir save/servidor)
    public static class PlayerProgress
    {
        public static double Gold;
        public static double DamageMultiplier = 1;
        public static int Kills;
    }
}
