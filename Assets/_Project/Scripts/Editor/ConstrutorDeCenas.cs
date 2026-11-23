using ALotOfDamage.Combat;
using ALotOfDamage.Core;
using ALotOfDamage.UI;
using ALotOfDamage.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ALotOfDamage.Ferramentas
{
    // Menu que monta as cenas de protótipo com placeholders
    public static class ConstrutorDeCenas
    {
        const string Pasta = "Assets/_Project/Scenes";

        // Também usado em linha de comando (-executeMethod)
        [MenuItem("A Lot of Damage/Recriar todas as cenas")]
        public static void CriarTodas()
        {
            CriarBalanceamento();
            CriarBatalha();
            CriarMapa();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{Pasta}/Mapa.unity", true),
                new EditorBuildSettingsScene($"{Pasta}/Batalha.unity", true),
            };
        }

        // Cria os assets de balanceamento com os valores padrão, se ainda não existirem
        [MenuItem("A Lot of Damage/Criar assets de balanceamento")]
        public static void CriarBalanceamento()
        {
            CriarAssetSeFaltar<BalanceamentoDeUpgrades>();
            CriarAssetSeFaltar<BalanceamentoDaForja>();
            CriarAssetSeFaltar<BalanceamentoDoBestiario>();
            AssetDatabase.SaveAssets();
        }

        static void CriarAssetSeFaltar<T>() where T : ScriptableObject
        {
            const string pasta = "Assets/_Project/Data/Resources";
            string caminho = $"{pasta}/{typeof(T).Name}.asset";
            if (AssetDatabase.LoadAssetAtPath<T>(caminho)) return;

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data")) AssetDatabase.CreateFolder("Assets/_Project", "Data");
            if (!AssetDatabase.IsValidFolder(pasta)) AssetDatabase.CreateFolder("Assets/_Project/Data", "Resources");
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), caminho);
        }

        [MenuItem("A Lot of Damage/Criar cena: Mapa")]
        public static void CriarMapa()
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Random.InitState(42);

            var chao = GameObject.CreatePrimitive(PrimitiveType.Plane);
            chao.name = "Chao";
            chao.transform.localScale = new Vector3(10, 1, 10);
            Pintar(chao, new Color(0.35f, 0.55f, 0.3f));

            // Obstáculos longe do ponto inicial para perceber o movimento da câmera
            for (int i = 0; i < 25; i++)
            {
                Vector3 p;
                do p = new Vector3(Random.Range(-40, 40), 0.5f, Random.Range(-40, 40)); while (p.magnitude < 6);
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "Obstaculo";
                c.transform.position = p;
                Pintar(c, new Color(0.5f, 0.45f, 0.4f));
            }

            var jogador = new GameObject("Jogador") { tag = "Player" };
            jogador.transform.position = new Vector3(0, 1, 0);
            jogador.AddComponent<CharacterController>();
            var movimento = jogador.AddComponent<MovimentoDoJogador>();
            jogador.AddComponent<AtaqueNoMapa>();

            // Placeholder do sprite: um quad virado para a câmera
            var sprite = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sprite.name = "Sprite (placeholder)";
            Object.DestroyImmediate(sprite.GetComponent<Collider>());
            sprite.transform.SetParent(jogador.transform, false);
            sprite.transform.localScale = new Vector3(1f, 2f, 1f);
            sprite.AddComponent<Billboard>();
            Pintar(sprite, new Color(0.3f, 0.5f, 1f));

            // Monstros: id, nome, multiplicador de vida, cor, escala, quantidade, distância do início
            CriarInimigos("slime", "Slime", 1, new Color(0.3f, 0.9f, 0.3f), 0.8f, 4, 12f);
            CriarInimigos("lobo", "Lobo", 4, new Color(0.6f, 0.6f, 0.65f), 1f, 3, 20f);
            CriarInimigos("golem", "Golem", 20, new Color(0.55f, 0.35f, 0.2f), 1.6f, 2, 30f);

            var cam = Camera.main;
            var iso = cam.gameObject.AddComponent<CameraIsometrica>();
            iso.alvo = jogador.transform;
            movimento.camera_ = cam.transform;

            CriarControlesDeToque();
            var janelas = new GameObject("Janelas");
            janelas.AddComponent<JanelaDeUpgrades>();
            janelas.AddComponent<JanelaDaForja>();
            janelas.AddComponent<JanelaDoBestiario>();
            Salvar(cena, "Mapa");
        }

        static void CriarInimigos(string id, string nome, double vida, Color cor, float escala, int quantidade, float distancia)
        {
            for (int i = 0; i < quantidade; i++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2);
                var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.name = $"{nome} {i + 1}";
                go.transform.position = new Vector3(Mathf.Cos(ang) * distancia, escala, Mathf.Sin(ang) * distancia);
                go.transform.localScale = Vector3.one * escala;
                Pintar(go, cor);

                var e = go.AddComponent<InimigoDoMapa>();
                e.idDoInimigo = $"{id}_{i + 1}";
                e.tipo = id;
                e.nome = nome;
                e.multiplicadorDeVida = vida;
            }
        }

        static void CriarControlesDeToque()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();

            var canvasGo = new GameObject("Controles de Toque");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var escalador = canvasGo.AddComponent<CanvasScaler>();
            escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escalador.referenceResolution = new Vector2(1920, 1080);
            escalador.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<ControlesDeToque>();

            var circulo = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            // Joystick no canto inferior esquerdo
            var fundo = NovaImagem("Joystick", canvasGo.transform, circulo, new Color(1, 1, 1, 0.25f), 260);
            fundo.anchorMin = fundo.anchorMax = new Vector2(0, 0);
            fundo.anchoredPosition = new Vector2(220, 220);
            var pino = NovaImagem("Pino", fundo, circulo, new Color(1, 1, 1, 0.7f), 110);
            pino.GetComponent<Image>().raycastTarget = false;
            var joy = fundo.gameObject.AddComponent<JoystickVirtual>();
            joy.fundo = fundo;
            joy.pino = pino;

            // Botão de ataque no canto inferior direito
            var ataque = NovaImagem("Botao de Ataque", canvasGo.transform, circulo, new Color(1f, 0.4f, 0.3f, 0.6f), 200);
            ataque.anchorMin = ataque.anchorMax = new Vector2(1, 0);
            ataque.anchoredPosition = new Vector2(-200, 200);
            ataque.gameObject.AddComponent<BotaoDeAtaque>();
        }

        static RectTransform NovaImagem(string nome, Transform pai, Sprite sprite, Color cor, float tamanho)
        {
            var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(pai, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = cor;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(tamanho, tamanho);
            return rt;
        }

        [MenuItem("A Lot of Damage/Criar cena: Batalha")]
        public static void CriarBatalha()
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            cam.transform.position = new Vector3(0, 2, -6);
            cam.transform.rotation = Quaternion.Euler(10, 0, 0);

            var raiz = new GameObject("Inimigo");
            raiz.transform.position = new Vector3(0, 1, 0);
            var corpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            corpo.transform.SetParent(raiz.transform, false);

            var combate = new GameObject("Combate").AddComponent<CombateClicker>();
            combate.visualDoInimigo = corpo.transform;

            Salvar(cena, "Batalha");
        }

        // Material simples por objeto (URP Lit)
        static void Pintar(GameObject go, Color cor)
        {
            var r = go.GetComponent<Renderer>();
            if (!r) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) return;
            r.sharedMaterial = new Material(shader) { color = cor };
        }

        static void Salvar(UnityEngine.SceneManagement.Scene cena, string nome)
        {
            if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            EditorSceneManager.SaveScene(cena, $"{Pasta}/{nome}.unity");
        }
    }
}
