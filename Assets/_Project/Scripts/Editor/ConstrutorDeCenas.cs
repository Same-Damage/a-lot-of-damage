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

            // Ilha flutuante: o relevo da borda e os objetos mudam por região em tempo de jogo (CenarioDaRegiao);
            // a forma base fica salva aqui para o colisor e o NavMesh
            var parametrosDaIlha = new ParametrosDaIlha();
            var ilha = new GameObject("Ilha", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            var malhaDaIlha = SalvarAsset(MalhaDaIlha.Criar(parametrosDaIlha, 0), $"{Pasta}/Mapa/Ilha.asset");
            ilha.GetComponent<MeshFilter>().sharedMesh = malhaDaIlha;
            ilha.GetComponent<MeshCollider>().sharedMesh = malhaDaIlha;
            var materialDoCenario = MaterialDoMapa("Cenario", "Universal Render Pipeline/Lit");
            ilha.GetComponent<MeshRenderer>().sharedMaterials = new[] { materialDoCenario, materialDoCenario };
            var cenario = ilha.AddComponent<CenarioDaRegiao>();
            cenario.ilha = parametrosDaIlha;
            cenario.materialBase = materialDoCenario;
            cenario.materialDasNuvens = MaterialDoMapa("Nuvens", "Universal Render Pipeline/Unlit");
            cenario.materialDasParticulas = MaterialDoMapa("Particulas", "Universal Render Pipeline/2D/Sprite-Unlit-Default");
            CriarMuralha(parametrosDaIlha);

            var jogador = new GameObject("Jogador") { tag = "Player" };
            jogador.transform.position = new Vector3(0, 1, 0);
            jogador.AddComponent<CharacterController>();
            var movimento = jogador.AddComponent<MovimentoDoJogador>();
            jogador.AddComponent<AtaqueNoMapa>();

            // Guerreiro pintado, animado por recorte, com os pés na base do CharacterController
            var boneco = CriarBoneco("Guerreiro", jogador.transform, new Vector3(0, -1f, 0));
            // Virado de frente para a câmera, com os pés como eixo: fica em pé e em tamanho cheio
            boneco.gameObject.AddComponent<Billboard>().lockVertical = false;
            boneco.transform.localScale = Vector3.one * EscalaNoMapa;
            // Um pouco para trás (na direção em que a câmera olha) para ficar embaixo dos pés do desenho inclinado
            CriarSombra(jogador.transform, new Vector3(0.2f, -0.98f, 0.2f), 1.1f, 0.45f, 0.65f);
            movimento.boneco = boneco;
            jogador.GetComponent<AtaqueNoMapa>().boneco = boneco;

            // Follower (Pip, o falcão) acompanhando o jogador
            var follower = CriarFollower(new Vector3(-1.6f, 2.8f, 0));
            follower.AddComponent<SeguidorNoMapa>().alvo = jogador.transform;
            CriarSombra(follower.transform, new Vector3(0, -2.78f, 0), 0.5f, 0.25f, 0.25f);

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
            // Como no lobby do Legend Online: câmera mais baixa e quase sem perspectiva,
            // para os personagens pintados ficarem retos e grandes na tela
            iso.inclinacao = 32f;
            iso.distancia = 30f;
            cam.fieldOfView = 24f;
            iso.alvo = jogador.transform;
            movimento.camera_ = cam.transform;
            cenario.camera_ = cam;
            cenario.jogador = jogador.transform;
            cenario.sol = Object.FindAnyObjectByType<Light>();

            AssarNavMesh(ilha);
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
            public bool OlhaParaDireita = true;
            public PecaDaMontagem[] Pecas;
        }

        [System.Serializable]
        class PecaDaMontagem
        {
            public string Nome, Pai;
            public int Ordem;
            public float PivoX, PivoY, Largura, Altura, EncaixeX, EncaixeY;
            public float RotacaoDoSprite;   // graus, anti-horário: alinha a peça desenhada torta (ex.: espada na diagonal)
        }

        class Recorte
        {
            public GameObject Raiz;
            public Transform Corpo;
            public bool OlhaParaDireita;
            public System.Collections.Generic.Dictionary<string, Transform> Ossos = new System.Collections.Generic.Dictionary<string, Transform>();
            public Transform Osso(string nome) => Ossos.TryGetValue(nome, out var t) ? t : null;
        }

        // Monta um personagem de recorte a partir de Art/Sprites/<personagem>/montagem.json:
        // cada peça vira um osso no ponto de encaixe do pai, com o sprite deslocado para girar no pivô.
        // Com noCentro, o meio do desenho fica na raiz (aves voando); sem, os pés (personagens de pé).
        static Recorte MontarRecorte(string personagem, Transform pai, Vector3 posicao, bool noCentro)
        {
            string pasta = $"Assets/_Project/Art/Sprites/{personagem}";
            var montagem = JsonUtility.FromJson<Montagem>(AssetDatabase.LoadAssetAtPath<TextAsset>($"{pasta}/montagem.json").text);
            float ppu = montagem.PixelsPorUnidade;

            var raiz = new GameObject(personagem);
            raiz.transform.SetParent(pai, false);
            raiz.transform.localPosition = posicao;
            raiz.AddComponent<UnityEngine.Rendering.SortingGroup>();
            var corpo = new GameObject("Corpo").transform;
            corpo.SetParent(raiz.transform, false);
            var recorte = new Recorte { Raiz = raiz, Corpo = corpo, OlhaParaDireita = montagem.OlhaParaDireita };

            var material = MaterialDosPersonagens();
            var porNome = new System.Collections.Generic.Dictionary<string, PecaDaMontagem>();
            foreach (var p in montagem.Pecas) porNome[p.Nome] = p;

            Transform Osso(PecaDaMontagem p)
            {
                if (recorte.Ossos.TryGetValue(p.Nome, out var existente)) return existente;
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
                var giro = Quaternion.Euler(0, 0, p.RotacaoDoSprite);
                go.transform.localRotation = giro;
                go.transform.localPosition = giro * (new Vector3(p.Largura / 2 - p.PivoX, -(p.Altura / 2 - p.PivoY), 0) / ppu);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = ImportarSprite($"{pasta}/{p.Nome}.png", ppu);
                sr.sortingOrder = p.Ordem;
                sr.sharedMaterial = material;
                recorte.Ossos[p.Nome] = osso;
                return osso;
            }
            foreach (var p in montagem.Pecas) Osso(p);

            var limites = new Bounds();
            bool primeiro = true;
            foreach (var sr in raiz.GetComponentsInChildren<SpriteRenderer>())
            {
                if (primeiro) { limites = sr.bounds; primeiro = false; }
                else limites.Encapsulate(sr.bounds);
            }
            float alvo = noCentro ? limites.center.y : limites.min.y;
            corpo.localPosition = new Vector3(0, raiz.transform.position.y - alvo, 0);
            return recorte;
        }

        static Material MaterialDosPersonagens()
        {
            const string caminho = "Assets/_Project/Art/Sprites/Personagens.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (material) return material;
            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (!shader) shader = Shader.Find("Sprites/Default");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, caminho);
            return material;
        }

        // Personagem de pé (pés em "pes"), animado pelo BonecoDeRecorte
        static BonecoDeRecorte CriarBoneco(string personagem, Transform pai, Vector3 pes)
        {
            var r = MontarRecorte(personagem, pai, pes, false);
            var b = r.Raiz.AddComponent<BonecoDeRecorte>();
            b.corpo = r.Corpo;
            b.desenhoOlhaParaDireita = r.OlhaParaDireita;
            b.tronco = r.Osso("tronco"); b.cabeca = r.Osso("cabeca"); b.capa = r.Osso("capa");
            b.bracoFrente = r.Osso("braco_frente"); b.antebracoFrente = r.Osso("antebraco_frente"); b.espada = r.Osso("espada");
            b.bracoTras = r.Osso("braco_tras"); b.antebracoTras = r.Osso("antebraco_tras");
            b.pernaFrente = r.Osso("perna_frente"); b.pernaTras = r.Osso("perna_tras");
            return b;
        }

        // Ave voando (centro em "centro"), animada pela AveDeRecorte
        static AveDeRecorte CriarAve(string personagem, Transform pai, Vector3 centro)
        {
            var r = MontarRecorte(personagem, pai, centro, true);
            var a = r.Raiz.AddComponent<AveDeRecorte>();
            a.corpo = r.Corpo;
            a.desenhoOlhaParaDireita = r.OlhaParaDireita;
            a.asaFrente = r.Osso("asa_frente"); a.asaTras = r.Osso("asa_tras");
            a.cabeca = r.Osso("cabeca"); a.cauda = r.Osso("cauda");
            return a;
        }

        static Sprite ImportarSprite(string caminho, float ppu)
        {
            var importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.spritePixelsPerUnit = ppu;
            importador.filterMode = FilterMode.Bilinear;
            // Mipmaps ligados: a arte pintada aparece bem menor que o original e serrilharia sem eles
            importador.mipmapEnabled = true;
            importador.mipMapsPreserveCoverage = true;
            importador.alphaIsTransparency = true;
            importador.textureCompression = TextureImporterCompression.CompressedHQ;
            var android = importador.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.format = TextureImporterFormat.ASTC_6x6;
            importador.SetPlatformTextureSettings(android);
            importador.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        }

        // Personagens um pouco maiores que no desenho original, para ler bem com a câmera alta do mapa
        const float EscalaNoMapa = 1.3f;

        static void CriarSombra(Transform dono, Vector3 posicao, float largura, float profundidade, float opacidade = 0.45f)
        {
            var go = new GameObject("Sombra", typeof(SpriteRenderer));
            go.transform.SetParent(dono, false);
            go.transform.localPosition = posicao;
            go.GetComponent<SpriteRenderer>().sharedMaterial = MaterialDosPersonagens();
            var sombra = go.AddComponent<SombraDeContato>();
            sombra.largura = largura;
            sombra.profundidade = profundidade;
            sombra.opacidade = opacidade;
        }

        const float EscalaDoPip = 0.95f;

        // Pip, o falcão: um suporte que segue a posição e, dentro dele, a ave virada para a câmera
        static GameObject CriarFollower(Vector3 posicao)
        {
            var go = new GameObject("Pip");
            go.transform.position = posicao;
            var ave = CriarAve("Pip", go.transform, Vector3.zero);
            ave.transform.localScale = Vector3.one * EscalaDoPip;
            ave.gameObject.AddComponent<Billboard>().lockVertical = false;
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

        // Parede invisível na beira da área andável: os morros e o penhasco ficam só de cenário
        static void CriarMuralha(ParametrosDaIlha p)
        {
            var muralha = new GameObject("Muralha");
            const int pedacos = 48;
            float raio = p.RaioAndavel + 0.5f;
            float largura = 2 * Mathf.PI * raio / pedacos + 0.5f;
            for (int i = 0; i < pedacos; i++)
            {
                float ang = i * 360f / pedacos;
                var parte = new GameObject("Parede", typeof(BoxCollider));
                parte.transform.SetParent(muralha.transform, false);
                parte.transform.SetPositionAndRotation(Quaternion.Euler(0, ang, 0) * new Vector3(0, 2, raio), Quaternion.Euler(0, ang, 0));
                parte.GetComponent<BoxCollider>().size = new Vector3(largura, 6, 1);
                var modificador = parte.AddComponent<NavMeshModifier>();
                modificador.overrideArea = true;
                modificador.area = NavMesh.GetAreaFromName("Not Walkable");
            }
        }

        static Material MaterialDoMapa(string nome, string shader)
        {
            string caminho = $"Assets/_Project/Art/Mapa/{nome}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (material) return material;
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Mapa")) AssetDatabase.CreateFolder("Assets/_Project/Art", "Mapa");
            material = new Material(Shader.Find(shader)) { enableInstancing = true };
            AssetDatabase.CreateAsset(material, caminho);
            return material;
        }

        static T SalvarAsset<T>(T asset, string caminho) where T : Object
        {
            if (!AssetDatabase.IsValidFolder($"{Pasta}/Mapa")) AssetDatabase.CreateFolder(Pasta, "Mapa");
            AssetDatabase.DeleteAsset(caminho);
            AssetDatabase.CreateAsset(asset, caminho);
            return asset;
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
