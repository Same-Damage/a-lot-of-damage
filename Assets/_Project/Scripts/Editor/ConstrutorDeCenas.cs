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
            CriarComparacao();
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
            CriarAssetSeFaltar<BalanceamentoDasConquistas>();
            CriarAssetSeFaltar<BalanceamentoDasReliquias>();
            CriarAssetSeFaltar<BalanceamentoDaTaverna>();
            CriarAssetSeFaltar<BalanceamentoDaHistoria>();
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

            // Herói 3D com shader pintado, com os pés na base do CharacterController
            var herois = CriarHerois(jogador.transform, new Vector3(0, -1f, 0), AlturaDoPersonagem3D, Quaternion.identity);
            var boneco = herois.herois[0];
            CriarSombra(jogador.transform, new Vector3(0, -0.98f, 0), 0.9f, 0.5f, 0.6f);
            movimento.boneco = boneco;
            jogador.GetComponent<AtaqueNoMapa>().boneco = boneco;
            var troca = jogador.AddComponent<TrocaDeHeroi>();
            troca.herois = herois;
            troca.movimento = movimento;
            troca.ataque = jogador.GetComponent<AtaqueNoMapa>();

            // Follower (Pip, o falcão) acompanhando o jogador
            var follower = CriarFollower(new Vector3(-1.6f, 2.8f, 0), Vector3.back);
            follower.seguidor = follower.gameObject.AddComponent<SeguidorNoMapa>();
            follower.seguidor.alvo = jogador.transform;

            // Pontos de monstro: o tipo de cada um vem da região atual da campanha
            for (int i = 0; i < 9; i++)
            {
                float ang = Random.Range(0f, Mathf.PI * 2);
                float distancia = 12f + i * 2.2f;
                CriarPontoDeMonstro($"ponto_{i + 1}", i, false, new Vector3(Mathf.Cos(ang) * distancia, 1, Mathf.Sin(ang) * distancia));
            }
            CriarPontoDeMonstro("chefe", 0, true, new Vector3(0, 1, 32));

            // Pontos de coleta (ingredientes da alquimia), revezando os recursos da campanha
            var recursos = BalanceamentoDaCampanha.Carregar().recursos;
            for (int i = 0; i < 12 && recursos.Count > 0; i++)
            {
                float ang = i * 2.4f + 0.7f;
                float distancia = 7f + (i % 4) * 5f;
                var ponto = new GameObject($"Coleta {i + 1}").AddComponent<PontoDeColeta>();
                ponto.transform.position = new Vector3(Mathf.Cos(ang) * distancia, 1, Mathf.Sin(ang) * distancia);
                ponto.recursoId = recursos[i % recursos.Count].Id;
                ponto.materialBase = MaterialDoMapa("Cenario", "Universal Render Pipeline/Lit");
            }

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
            janelas.AddComponent<JanelaDaAscensao>();
            janelas.AddComponent<JanelaDasConquistas>();
            janelas.AddComponent<JanelaDosFollowers>();
            janelas.AddComponent<JanelaDasReliquias>();
            janelas.AddComponent<JanelaDaTaverna>();
            janelas.AddComponent<JanelaDasExpedicoes>();
            janelas.AddComponent<JanelaDaAlquimia>();
            var vitrine = new GameObject("Vitrine do herói").AddComponent<VitrineDoHeroi>();
            vitrine.herois = herois;
            vitrine.brilho = herois.GetComponent<BrilhoDasArmas>();
            vitrine.janela = janelas.AddComponent<JanelaDeInventario>();
            new GameObject("HUD do Mapa").AddComponent<HudDoMapa>();
            new GameObject("Recompensa AFK").AddComponent<JanelaDaRecompensaAfk>();
            new GameObject("História").AddComponent<CaixaDeDialogo>();
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

        // Follower ativo: raiz que se move (segue o herói no mapa) e, dentro, o modelo low-poly da família dele,
        // montado em jogo pelo FollowerDaCena conforme o follower escolhido; a sombra é ajustada à altura de cada um
        static FollowerDaCena CriarFollower(Vector3 posicao, Vector3 olharPara)
        {
            var go = new GameObject("Follower");
            go.transform.position = posicao;
            var modelo = new GameObject("Modelo").AddComponent<FollowerLowPoly>();
            modelo.transform.SetParent(go.transform, false);
            modelo.materialBase = MaterialDoMapa("Cenario", "Universal Render Pipeline/Lit");
            CriarSombra(go.transform, Vector3.zero, 0.6f, 0.35f, 0.35f);
            var f = go.AddComponent<FollowerDaCena>();
            f.modelo = modelo;
            f.sombra = go.GetComponentInChildren<SombraDeContato>();
            f.olharPara = olharPara;
            return f;
        }

        static void CriarPontoDeMonstro(string id, int indice, bool chefe, Vector3 posicao)
        {
            var go = new GameObject(chefe ? "Ponto do Chefe" : $"Ponto de Monstro {indice + 1}");
            go.transform.position = posicao;
            // o ponto fica 1 acima do chão (meio do agente); o modelo pisa no chão
            var modelo = new GameObject("Modelo").AddComponent<MonstroLowPoly>();
            modelo.transform.SetParent(go.transform, false);
            modelo.transform.localPosition = new Vector3(0, -1, 0);
            modelo.materialBase = MaterialDoMapa("Cenario", "Universal Render Pipeline/Lit");

            var agente = go.AddComponent<NavMeshAgent>();
            agente.radius = 0.5f;
            agente.acceleration = 20f;
            var e = go.AddComponent<InimigoDoMapa>();
            e.idDoInimigo = id;
            e.indiceDoPonto = indice;
            e.pontoDoChefe = chefe;
            e.modelo = modelo;
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
            var fundo = NovaImagem("Joystick", canvasGo.transform, circulo, new Color(1, 1, 1, 0.16f), 230);
            fundo.anchorMin = fundo.anchorMax = new Vector2(0, 0);
            fundo.anchoredPosition = new Vector2(220, 220);
            var pino = NovaImagem("Pino", fundo, circulo, new Color(1, 1, 1, 0.5f), 96);
            pino.GetComponent<Image>().raycastTarget = false;
            var joy = fundo.gameObject.AddComponent<JoystickVirtual>();
            joy.fundo = fundo;
            joy.pino = pino;

            // Botão de ataque no canto inferior direito
            var ataque = NovaImagem("Botao de Ataque", canvasGo.transform, circulo, new Color(0.8f, 0.25f, 0.2f, 0.55f), 150);
            ataque.anchorMin = ataque.anchorMax = new Vector2(1, 0);
            ataque.anchoredPosition = new Vector2(-150, 150);
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
            raiz.transform.position = new Vector3(0, 0, 0);
            // virado para o herói, à esquerda e um pouco à frente
            var corpo = new GameObject("Modelo").AddComponent<MonstroLowPoly>();
            corpo.transform.SetParent(raiz.transform, false);
            corpo.materialBase = MaterialDoMapa("Cenario", "Universal Render Pipeline/Lit");
            corpo.transform.localRotation = Quaternion.LookRotation(new Vector3(-1, 0, -0.5f));

            // ao lado do herói, no chão da arena, virado para o inimigo
            CriarFollower(new Vector3(-1.6f, 0, -1.6f), new Vector3(1, 0, 0.4f));

            // de três quartos, virados para o inimigo e um pouco para a câmera
            var herois = CriarHerois(null, new Vector3(-2.4f, 0, -0.6f), 1.8f, Quaternion.LookRotation(new Vector3(1, 0, -0.6f)));

            // na batalha só marca a presença (a janela de recompensa aparece no mapa)
            new GameObject("Presença").AddComponent<JanelaDaRecompensaAfk>().mostrarJanela = false;
            var combate = new GameObject("Combate").AddComponent<CombateClicker>();
            combate.visualDoInimigo = corpo.transform;
            combate.modeloDoInimigo = corpo;
            combate.herois = herois;

            Salvar(cena, "Batalha");
        }


        // Altura no mapa dos personagens 3D, parecida com a do boneco de recorte para comparar
        const float AlturaDoPersonagem3D = 2.3f;

        // Heróis que o jogador alterna no mapa (pastas de Art/Characters); o primeiro é o padrão
        static readonly string[] Herois = { "Cavaleiro", "Arqueira", "Ladino", "Mago" };

        // Todos os heróis no mesmo lugar, um ativo por vez (TrocaDeHeroi no mapa, CombateClicker na batalha)
        static HeroisDaCena CriarHerois(Transform pai, Vector3 pes, float altura, Quaternion rotacao)
        {
            var grupo = new GameObject("Herois").AddComponent<HeroisDaCena>();
            grupo.gameObject.AddComponent<BrilhoDasArmas>();
            grupo.transform.SetParent(pai, false);
            grupo.herois = new PersonagemVisual[Herois.Length];
            for (int i = 0; i < Herois.Length; i++)
            {
                var h = ConstrutorDePersonagens3D.Criar(Herois[i], grupo.transform, pes, altura);
                h.transform.rotation = rotacao;
                h.gameObject.SetActive(i == 0);
                grupo.herois[i] = h;
            }
            return grupo;
        }

        // Cena só para avaliar a direção de arte: os personagens 3D lado a lado, com a câmera do mapa.
        // Não entra no build.
        [MenuItem("A Lot of Damage/Criar cena: Comparação de personagens")]
        public static void CriarComparacao()
        {
            var cena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            new GameObject("Escopo da Cena").AddComponent<EscopoDaCena>();

            var chao = GameObject.CreatePrimitive(PrimitiveType.Plane);
            chao.name = "Chao";
            chao.GetComponent<MeshRenderer>().sharedMaterial = MaterialDoMapa("Cenario", "Universal Render Pipeline/Lit");

            var personagens = Herois;
            for (int i = 0; i < personagens.Length; i++)
            {
                var lugar = new GameObject("Lugar de " + personagens[i]).transform;
                lugar.position = new Vector3((i - (personagens.Length - 1) * 0.5f) * 3.2f, 0, 0);
                var p = ConstrutorDePersonagens3D.Criar(personagens[i], lugar, Vector3.zero, AlturaDoPersonagem3D);
                p.transform.rotation = Quaternion.LookRotation(new Vector3(-0.4f, 0, -1));
                CriarSombra(lugar, new Vector3(0, 0.02f, 0), 0.9f, 0.5f, 0.6f);
            }

            var cam = Camera.main;
            var foco = new GameObject("Foco").transform;
            foco.position = new Vector3(0, 1, 0);
            var iso = cam.gameObject.AddComponent<CameraIsometrica>();
            iso.inclinacao = 32f;
            iso.distancia = 30f;
            cam.fieldOfView = 24f;
            iso.alvo = foco;
            Salvar(cena, "Comparacao");
        }

        static void Salvar(UnityEngine.SceneManagement.Scene cena, string nome)
        {
            if (!AssetDatabase.IsValidFolder(Pasta)) AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
            EditorSceneManager.SaveScene(cena, $"{Pasta}/{nome}.unity");
        }
    }
}
