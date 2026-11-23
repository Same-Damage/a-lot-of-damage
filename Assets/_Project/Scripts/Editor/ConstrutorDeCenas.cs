using ALotOfDamage.Combat;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.UI;
using ALotOfDamage.World;
using ALotOfDamage.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
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
            CriarAssetSeFaltar<BalanceamentoDePersonagens>();
            CriarAssetSeFaltar<BalanceamentoDaCampanha>();
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
            new GameObject("Escopo da Cena").AddComponent<EscopoDaCena>();
            Random.InitState(42);

            var chao = GameObject.CreatePrimitive(PrimitiveType.Plane);
            chao.name = "Chao";
            chao.transform.localScale = new Vector3(10, 1, 10);
            Pintar(chao, Color.white);
            chao.AddComponent<AparenciaDaRegiao>();

            // Obstáculos longe do ponto inicial para perceber o movimento da câmera
            for (int i = 0; i < 25; i++)
            {
                Vector3 p;
                do p = new Vector3(Random.Range(-40, 40), 0.5f, Random.Range(-40, 40)); while (p.magnitude < 6);
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.name = "Obstaculo";
                c.transform.position = p;
                Pintar(c, new Color(0.5f, 0.45f, 0.4f));
                var modificador = c.AddComponent<NavMeshModifier>();
                modificador.overrideArea = true;
                modificador.area = NavMesh.GetAreaFromName("Not Walkable");
            }

            var jogador = new GameObject("Jogador") { tag = "Player" };
            jogador.transform.position = new Vector3(0, 1, 0);
            jogador.AddComponent<CharacterController>();
            var movimento = jogador.AddComponent<MovimentoDoJogador>();
            jogador.AddComponent<AtaqueNoMapa>();

            // Guerreiro pintado, animado por recorte, com os pés na base do CharacterController
            var boneco = CriarBoneco("Guerreiro", jogador.transform, new Vector3(0, -1f, 0));
            boneco.gameObject.AddComponent<Billboard>();
            movimento.boneco = boneco;
            jogador.GetComponent<AtaqueNoMapa>().boneco = boneco;

            // Follower (Pip, o falcão) acompanhando o jogador
            var follower = CriarFollower(new Vector3(-1.6f, 2.8f, 0));
            follower.AddComponent<SeguidorNoMapa>().alvo = jogador.transform;

            // Pontos de monstro: o tipo de cada um vem da região atual da campanha
            for (int i = 0; i < 9; i++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2);
                float distancia = 12f + i * 2.2f;
                CriarPontoDeMonstro($"ponto_{i + 1}", i, false, new Vector3(Mathf.Cos(ang) * distancia, 1, Mathf.Sin(ang) * distancia));
            }
            CriarPontoDeMonstro("chefe", 0, true, new Vector3(0, 1, 32));

            var cam = Camera.main;
            var iso = cam.gameObject.AddComponent<CameraIsometrica>();
            iso.alvo = jogador.transform;
            movimento.camera_ = cam.transform;

            AssarNavMesh(chao);
            CriarControlesDeToque();
            var janelas = new GameObject("Janelas");
            janelas.AddComponent<JanelaDeUpgrades>();
            janelas.AddComponent<JanelaDaForja>();
            janelas.AddComponent<JanelaDoBestiario>();
            new GameObject("HUD do Mapa").AddComponent<HudDoMapa>();
            Salvar(cena, "Mapa");
        }

        [System.Serializable]
        class Montagem
        {
            public float PixelsPorUnidade = 280;
            public PecaDaMontagem[] Pecas;
        }

        [System.Serializable]
        class PecaDaMontagem
        {
            public string Nome, Pai;
            public int Ordem;
            public float PivoX, PivoY, Largura, Altura, EncaixeX, EncaixeY;
        }

        // Monta o boneco de recorte a partir de Art/Sprites/<personagem>/montagem.json:
        // cada peça vira um osso no ponto de encaixe do pai, com o sprite deslocado para girar no pivô.
        // pes: posição dos pés, no espaço do pai (ou do mundo, sem pai).
        static BonecoDeRecorte CriarBoneco(string personagem, Transform pai, Vector3 pes)
        {
            string pasta = $"Assets/_Project/Art/Sprites/{personagem}";
            var montagem = JsonUtility.FromJson<Montagem>(AssetDatabase.LoadAssetAtPath<TextAsset>($"{pasta}/montagem.json").text);
            float ppu = montagem.PixelsPorUnidade;

            var raiz = new GameObject(personagem);
            raiz.transform.SetParent(pai, false);
            raiz.transform.localPosition = pes;
            raiz.AddComponent<UnityEngine.Rendering.SortingGroup>();
            var corpo = new GameObject("Corpo").transform;
            corpo.SetParent(raiz.transform, false);

            string caminhoDoMaterial = $"{pasta}/Material.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminhoDoMaterial);
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (!shader) shader = Shader.Find("Sprites/Default");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, caminhoDoMaterial);
            }

            var porNome = new System.Collections.Generic.Dictionary<string, PecaDaMontagem>();
            foreach (var p in montagem.Pecas) porNome[p.Nome] = p;
            var ossos = new System.Collections.Generic.Dictionary<string, Transform>();

            Transform Osso(PecaDaMontagem p)
            {
                if (ossos.TryGetValue(p.Nome, out var existente)) return existente;
                var osso = new GameObject(p.Nome).transform;
                if (string.IsNullOrEmpty(p.Pai)) osso.SetParent(corpo, false);
                else
                {
                    var doPai = porNome[p.Pai];
                    osso.SetParent(Osso(doPai), false);
                    // Na imagem o Y cresce para baixo; na Unity, para cima
                    osso.localPosition = new Vector3(p.EncaixeX - doPai.PivoX, -(p.EncaixeY - doPai.PivoY), 0) / ppu;
                }

                var go = new GameObject("Sprite", typeof(SpriteRenderer));
                go.transform.SetParent(osso, false);
                go.transform.localPosition = new Vector3(p.Largura / 2 - p.PivoX, -(p.Altura / 2 - p.PivoY), 0) / ppu;
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = ImportarSprite($"{pasta}/{p.Nome}.png", ppu);
                sr.sortingOrder = p.Ordem;
                sr.sharedMaterial = material;
                ossos[p.Nome] = osso;
                return osso;
            }
            foreach (var p in montagem.Pecas) Osso(p);

            // Sobe o corpo até a parte mais baixa (os pés) encostar na raiz
            float maisBaixo = float.MaxValue;
            foreach (var sr in raiz.GetComponentsInChildren<SpriteRenderer>()) maisBaixo = Mathf.Min(maisBaixo, sr.bounds.min.y);
            corpo.localPosition = new Vector3(0, raiz.transform.position.y - maisBaixo, 0);

            var b = raiz.AddComponent<BonecoDeRecorte>();
            b.corpo = corpo;
            Transform O(string n) => ossos.TryGetValue(n, out var t) ? t : null;
            b.tronco = O("tronco"); b.cabeca = O("cabeca"); b.capa = O("capa");
            b.bracoFrente = O("braco_frente"); b.antebracoFrente = O("antebraco_frente"); b.espada = O("espada");
            b.bracoTras = O("braco_tras"); b.antebracoTras = O("antebraco_tras");
            b.pernaFrente = O("perna_frente"); b.pernaTras = O("perna_tras");
            return b;
        }

        static Sprite ImportarSprite(string caminho, float ppu)
        {
            var importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.spritePixelsPerUnit = ppu;
            importador.filterMode = FilterMode.Bilinear;
            importador.mipmapEnabled = false;
            importador.alphaIsTransparency = true;
            importador.textureCompression = TextureImporterCompression.CompressedHQ;
            var android = importador.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.format = TextureImporterFormat.ASTC_6x6;
            importador.SetPlatformTextureSettings(android);
            importador.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        }

        // Placeholder do follower: um quad pequeno virado para a câmera
        static GameObject CriarFollower(Vector3 posicao)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Follower (placeholder)";
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.position = posicao;
            go.transform.localScale = Vector3.one * 0.6f;
            go.AddComponent<Billboard>();
            Pintar(go, new Color(0.85f, 0.55f, 0.25f));
            return go;
        }

        static void CriarPontoDeMonstro(string id, int indice, bool chefe, Vector3 posicao)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = chefe ? "Ponto do Chefe" : $"Ponto de Monstro {indice + 1}";
            go.transform.position = posicao;
            Pintar(go, Color.white);

            var agente = go.AddComponent<NavMeshAgent>();
            agente.radius = 0.5f;
            agente.acceleration = 20f;
            var e = go.AddComponent<InimigoDoMapa>();
            e.idDoInimigo = id;
            e.indiceDoPonto = indice;
            e.pontoDoChefe = chefe;
        }

        // Gera o NavMesh do chão (obstáculos ficam como não andáveis) e salva ao lado da cena
        static void AssarNavMesh(GameObject chao)
        {
            var superficie = chao.AddComponent<NavMeshSurface>();
            superficie.collectObjects = CollectObjects.All;
            // Só colisores contam; monstros não têm colisor e o jogador fica de fora
            superficie.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            GameObject.FindWithTag("Player").AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            superficie.BuildNavMesh();

            string pasta = $"{Pasta}/Mapa";
            if (!AssetDatabase.IsValidFolder(pasta)) AssetDatabase.CreateFolder(Pasta, "Mapa");
            string caminho = $"{pasta}/NavMesh.asset";
            AssetDatabase.DeleteAsset(caminho);
            AssetDatabase.CreateAsset(superficie.navMeshData, caminho);
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
            new GameObject("Escopo da Cena").AddComponent<EscopoDaCena>();

            var cam = Camera.main;
            cam.transform.position = new Vector3(0, 2, -6);
            cam.transform.rotation = Quaternion.Euler(10, 0, 0);

            var raiz = new GameObject("Inimigo");
            raiz.transform.position = new Vector3(0, 1, 0);
            var corpo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            corpo.transform.SetParent(raiz.transform, false);

            CriarFollower(new Vector3(-1.6f, 2.4f, -1.2f));

            var heroi = CriarBoneco("Guerreiro", null, new Vector3(-2.4f, 0, -0.6f));
            heroi.olhandoParaDireita = true;

            var combate = new GameObject("Combate").AddComponent<CombateClicker>();
            combate.visualDoInimigo = corpo.transform;
            combate.heroi = heroi;

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
