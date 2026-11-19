using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.Combat
{
    // Protótipo do combate clicker: auto-ataque + click com combo + ponto fraco + ultimate.
    // HUD em OnGUI só para testar a sensação; trocar por UI de verdade depois.
    public class ClickCombat : MonoBehaviour
    {
        [Header("Jogador")]
        public double autoDamage = 5;
        public float autoInterval = 1f;
        public double clickDamage = 3;
        [Range(0, 1)] public float critChance = 0.1f;
        public double critMultiplier = 5;
        public int maxClicksPerSecond = 15;

        [Header("Combo")]
        public float comboTimeout = 1f;
        public float comboBonusPerHit = 0.02f; // +2% por hit

        [Header("Ponto fraco")]
        public float weakPointEvery = 4f;
        public float weakPointDuration = 1.2f;
        public double weakPointMultiplier = 10;

        [Header("Ultimate")]
        public float ultChargePerClick = 2f;
        public double ultMultiplier = 50;

        [Header("Inimigo")]
        public double baseEnemyHp = 100;
        public double hpGrowth = 1.55; // vida cresce por estágio

        public Transform enemyVisual;

        int stage = 1;
        double enemyHp, enemyMaxHp, gold;
        int combo;
        float lastClick, autoTimer, weakTimer, weakLeft, ult;
        readonly Queue<float> clickTimes = new Queue<float>();
        readonly List<Popup> popups = new List<Popup>();
        float shake;

        struct Popup { public string text; public Vector2 pos; public float t; public Color color; public int size; }

        void Start() => SpawnEnemy();

        void SpawnEnemy()
        {
            enemyMaxHp = baseEnemyHp * System.Math.Pow(hpGrowth, stage - 1);
            enemyHp = enemyMaxHp;
        }

        void Update()
        {
            float now = Time.time;

            autoTimer += Time.deltaTime;
            if (autoTimer >= autoInterval) { autoTimer = 0; Hit(autoDamage, false, Color.white, 22); }

            if (combo > 0 && now - lastClick > comboTimeout) combo = 0;

            weakTimer += Time.deltaTime;
            if (weakLeft > 0) weakLeft -= Time.deltaTime;
            else if (weakTimer >= weakPointEvery) { weakTimer = 0; weakLeft = weakPointDuration; }

            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame) OnClick(now);

            var k = Keyboard.current;
            if (k != null && k.spaceKey.wasPressedThisFrame && ult >= 100) FireUltimate();

            if (shake > 0) shake -= Time.deltaTime;
            if (enemyVisual)
                enemyVisual.localPosition = shake > 0 ? (Vector3)Random.insideUnitCircle * 0.08f : Vector3.zero;
        }

        void OnClick(float now)
        {
            // Limite anti-autoclicker
            while (clickTimes.Count > 0 && now - clickTimes.Peek() > 1f) clickTimes.Dequeue();
            if (clickTimes.Count >= maxClicksPerSecond) return;
            clickTimes.Enqueue(now);

            combo++;
            lastClick = now;
            ult = Mathf.Min(100, ult + ultChargePerClick);

            double dmg = clickDamage * (1 + combo * comboBonusPerHit);
            bool crit = Random.value < critChance;
            Color color = new Color(1f, 0.85f, 0.3f);
            int size = 26;

            if (weakLeft > 0) { dmg *= weakPointMultiplier; color = new Color(0.4f, 1f, 1f); size = 40; ult = Mathf.Min(100, ult + 10); }
            if (crit) { dmg *= critMultiplier; color = new Color(1f, 0.3f, 0.3f); size += 10; }

            Hit(dmg, true, color, size);
        }

        void FireUltimate()
        {
            ult = 0;
            Hit(clickDamage * ultMultiplier * (1 + combo * comboBonusPerHit), true, new Color(0.8f, 0.4f, 1f), 56);
        }

        void Hit(double dmg, bool fromClick, Color color, int size)
        {
            enemyHp -= dmg;
            if (fromClick) shake = 0.08f;

            popups.Add(new Popup
            {
                text = NumberFormatter.Format(dmg),
                pos = new Vector2(Screen.width * 0.5f + Random.Range(-80, 80), Screen.height * 0.45f),
                t = 0, color = color, size = size
            });

            if (enemyHp <= 0)
            {
                gold += enemyMaxHp * 0.5;
                stage++;
                // Upgrade automático só para o protótipo mostrar números crescendo
                autoDamage *= 1.4; clickDamage *= 1.4;
                SpawnEnemy();
            }
        }

        void OnGUI()
        {
            var big = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(20, 20, 600, 30), $"Estágio {stage}   Ouro {NumberFormatter.Format(gold)}", big);

            float w = Screen.width * 0.5f;
            Rect bar = new Rect(Screen.width * 0.25f, 70, w, 24);
            GUI.Box(bar, "");
            GUI.color = new Color(0.85f, 0.2f, 0.25f);
            GUI.Box(new Rect(bar.x, bar.y, w * (float)System.Math.Max(0, enemyHp / enemyMaxHp), bar.height), "");
            GUI.color = Color.white;
            GUI.Label(new Rect(bar.x + 8, bar.y, w, 24), $"{NumberFormatter.Format(System.Math.Max(0, enemyHp))} / {NumberFormatter.Format(enemyMaxHp)}");

            GUI.Label(new Rect(20, Screen.height - 90, 600, 30), combo > 1 ? $"Combo x{combo}" : "", big);
            GUI.Label(new Rect(20, Screen.height - 55, 600, 30), ult >= 100 ? "ULTIMATE PRONTA! [Espaço]" : $"Ultimate {ult:0}%", big);
            if (weakLeft > 0)
                GUI.Label(new Rect(Screen.width * 0.5f - 120, 110, 300, 40), "PONTO FRACO! CLIQUE!", new GUIStyle(big) { fontSize = 28 });

            for (int i = popups.Count - 1; i >= 0; i--)
            {
                var p = popups[i];
                p.t += Time.deltaTime;
                if (p.t > 0.9f) { popups.RemoveAt(i); continue; }
                popups[i] = p;
                var st = new GUIStyle(big) { fontSize = p.size, normal = { textColor = new Color(p.color.r, p.color.g, p.color.b, 1 - p.t / 0.9f) } };
                GUI.Label(new Rect(p.pos.x, p.pos.y - p.t * 90, 400, 60), p.text, st);
            }
        }
    }
}
