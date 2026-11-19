using ALotOfDamage.Combat;
using ALotOfDamage.UI;
using ALotOfDamage.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ALotOfDamage.EditorTools
{
    // Menu que monta as cenas de teste com placeholders
    public static class PrototypeSceneBuilder
    {
        const string Folder = "Assets/_Project/Scenes";

        // Usado em linha de comando (-executeMethod)
        [MenuItem("A Lot of Damage/Recriar todas as cenas")]
        public static void CreateAll()
        {
            CreateBattleScene();
            CreateMapScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{Folder}/Mapa.unity", true),
                new EditorBuildSettingsScene($"{Folder}/Batalha.unity", true),
            };
        }

        [MenuItem("A Lot of Damage/Criar cena: Mapa")]
        public static void CreateMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Random.InitState(42);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Chao";
            ground.transform.localScale = new Vector3(10, 1, 10);
            Paint(ground, new Color(0.35f, 0.55f, 0.3f));

            // Obstáculos longe do ponto inicial para perceber o movimento da câmera
            for (int i = 0; i < 25; i++)
            {
                Vector3 p;
                do p = new Vector3(Random.Range(-40, 40), 0.5f, Random.Range(-40, 40)); while (p.magnitude < 6);
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "Obstaculo";
                c.transform.position = p;
                Paint(c, new Color(0.5f, 0.45f, 0.4f));
            }

            var player = new GameObject("Player") { tag = "Player" };
            player.transform.position = new Vector3(0, 1, 0);
            player.AddComponent<CharacterController>();
            var move = player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerMapAttack>();

            // Placeholder do sprite: um quad virado para a câmera
            var sprite = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sprite.name = "Sprite (placeholder)";
            Object.DestroyImmediate(sprite.GetComponent<Collider>());
            sprite.transform.SetParent(player.transform, false);
            sprite.transform.localScale = new Vector3(1f, 2f, 1f);
            sprite.AddComponent<Billboard>();
            Paint(sprite, new Color(0.3f, 0.5f, 1f));

            // Monstros: nome, multiplicador de vida, cor, escala
            AddEnemies("slime", "Slime", 1, new Color(0.3f, 0.9f, 0.3f), 0.8f, 4, 12f);
            AddEnemies("lobo", "Lobo", 4, new Color(0.6f, 0.6f, 0.65f), 1f, 3, 20f);
            AddEnemies("golem", "Golem", 20, new Color(0.55f, 0.35f, 0.2f), 1.6f, 2, 30f);

            var cam = Camera.main;
            var iso = cam.gameObject.AddComponent<IsoCameraDeadZone>();
            iso.target = player.transform;
            move.cameraTransform = cam.transform;

            CreateMobileControls();
            Save(scene, "Mapa");
        }

        static void AddEnemies(string id, string name, double hp, Color color, float scale, int count, float distance)
        {
            for (int i = 0; i < count; i++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2);
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.name = $"{name} {i + 1}";
                go.transform.position = new Vector3(Mathf.Cos(ang) * distance, scale, Mathf.Sin(ang) * distance);
                go.transform.localScale = Vector3.one * scale;
                Paint(go, color);

                var e = go.AddComponent<MapEnemy>();
                e.enemyId = $"{id}_{i + 1}";
                e.displayName = name;
                e.hpMultiplier = hp;
            }
        }

        static void CreateMobileControls()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("Controles Mobile");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<MobileControls>();

            var knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            // Joystick no canto inferior esquerdo
            var bg = NewImage("Joystick", canvasGo.transform, knobSprite, new Color(1, 1, 1, 0.25f), 260);
            bg.anchorMin = bg.anchorMax = new Vector2(0, 0);
            bg.anchoredPosition = new Vector2(220, 220);
            var knob = NewImage("Knob", bg, knobSprite, new Color(1, 1, 1, 0.7f), 110);
            knob.GetComponent<Image>().raycastTarget = false;
            var joy = bg.gameObject.AddComponent<VirtualJoystick>();
            joy.background = bg;
            joy.knob = knob;

            // Botão de ataque no canto inferior direito
            var atk = NewImage("Botao Ataque", canvasGo.transform, knobSprite, new Color(1f, 0.4f, 0.3f, 0.6f), 200);
            atk.anchorMin = atk.anchorMax = new Vector2(1, 0);
            atk.anchoredPosition = new Vector2(-200, 200);
            atk.gameObject.AddComponent<AttackButton>();
        }

        static RectTransform NewImage(string name, Transform parent, Sprite sprite, Color color, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(size, size);
            return rt;
        }

        [MenuItem("A Lot of Damage/Criar cena: Batalha")]
        public static void CreateBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            cam.transform.position = new Vector3(0, 2, -6);
            cam.transform.rotation = Quaternion.Euler(10, 0, 0);

            var enemyRoot = new GameObject("Inimigo");
            enemyRoot.transform.position = new Vector3(0, 1, 0);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(enemyRoot.transform, false);

            var combat = new GameObject("Combate").AddComponent<ClickCombat>();
            combat.enemyVisual = body.transform;

            Save(scene, "Batalha");
        }

        // Material simples por objeto (URP Lit)
        static void Paint(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (!r) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) return;
            var mat = new Material(shader) { color = color };
            r.sharedMaterial = mat;
        }

        static void Save(UnityEngine.SceneManagement.Scene scene, string name)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            EditorSceneManager.SaveScene(scene, $"{Folder}/{name}.unity");
        }
    }
}
