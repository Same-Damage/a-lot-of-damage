using ALotOfDamage.Combat;
using ALotOfDamage.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ALotOfDamage.EditorTools
{
    // Menu que monta as cenas de teste com placeholders
    public static class PrototypeSceneBuilder
    {
        const string Folder = "Assets/_Project/Scenes";

        // Usado em linha de comando (-executeMethod)
        public static void CreateAll()
        {
            CreateBattleScene();
            CreateMapScene();
        }

        [MenuItem("A Lot of Damage/Criar cena: Mapa (câmera + movimento)")]
        public static void CreateMapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Chao";
            ground.transform.localScale = new Vector3(10, 1, 10);

            // Alguns obstáculos para perceber o movimento da câmera
            for (int i = 0; i < 25; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "Obstaculo";
                c.transform.position = new Vector3(Random.Range(-40, 40), 0.5f, Random.Range(-40, 40));
            }

            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1, 0);
            var cc = player.AddComponent<CharacterController>();
            cc.center = new Vector3(0, 0, 0);
            var move = player.AddComponent<PlayerMovement>();

            // Placeholder do sprite: um quad virado para a câmera
            var sprite = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sprite.name = "Sprite (placeholder)";
            Object.DestroyImmediate(sprite.GetComponent<Collider>());
            sprite.transform.SetParent(player.transform, false);
            sprite.transform.localScale = new Vector3(1f, 2f, 1f);
            sprite.AddComponent<Billboard>();

            var cam = Camera.main;
            var iso = cam.gameObject.AddComponent<IsoCameraDeadZone>();
            iso.target = player.transform;
            move.cameraTransform = cam.transform;

            Save(scene, "Mapa");
        }

        [MenuItem("A Lot of Damage/Criar cena: Batalha (combate clicker)")]
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

        static void Save(UnityEngine.SceneManagement.Scene scene, string name)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            EditorSceneManager.SaveScene(scene, $"{Folder}/{name}.unity");
        }
    }
}
