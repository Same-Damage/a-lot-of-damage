using System.Collections.Generic;
using ALotOfDamage.Core;
using ALotOfDamage.Visual;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace ALotOfDamage.World
{
    // Monta o visual do mapa conforme a região da campanha: relevo da ilha, árvores, pedras, cristais e ruínas,
    // luz do sol, névoa, céu, pós-processamento (bloom, vinheta, cor e desfoque no fundo), mar de nuvens e partículas.
    // As regras de onde cada coisa nasce ficam em RegrasDoCenario; aqui só se desenha.
    public class CenarioDaRegiao : MonoBehaviour
    {
        public Light sol;
        public Camera camera_;
        public Transform jogador;
        public Material materialBase;          // URP Lit
        public Material materialDasNuvens;     // URP Unlit
        public Material materialDasParticulas; // URP Particles Unlit, transparente aditivo
        public ParametrosDaIlha ilha = new ParametrosDaIlha();

        ConsultarCampanha consultarCampanha;
        readonly List<Object> criados = new List<Object>();
        Material nuvens;
        Vector2 deslocamentoDasNuvens;
        Transform particulas;

        [Inject]
        public void Construir(ConsultarCampanha consultarCampanha) => this.consultarCampanha = consultarCampanha;

        void Start()
        {
            var regiao = consultarCampanha.Executar().Regiao;
            var visual = regiao?.Visual ?? new VisualDaRegiao();
            int semente = Semente(regiao?.Nome);

            MontarIlha(visual, semente);
            MontarObjetos(visual, semente);
            Iluminar(visual);
            PosProcessamento(visual);
            MontarNuvens(visual);
            MontarParticulas(visual);
        }

        void Update()
        {
            if (nuvens)
            {
                deslocamentoDasNuvens.x += Time.deltaTime * 0.0035f;
                deslocamentoDasNuvens.y += Time.deltaTime * 0.0015f;
                nuvens.mainTextureOffset = deslocamentoDasNuvens;
            }
            if (particulas && jogador) particulas.position = jogador.position;
        }

        void OnDestroy()
        {
            foreach (var o in criados) if (o) Destroy(o);
        }

        // Mesmo nome de região, mesma semente (string.GetHashCode muda entre plataformas)
        public static int Semente(string nome)
        {
            unchecked
            {
                int h = 17;
                if (nome != null) foreach (char c in nome) h = h * 31 + c;
                return h & 0x7FFFFFFF;
            }
        }

        static Color Cor(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        Material NovoMaterial(Color cor, float brilhoProprio = 0)
        {
            var m = new Material(materialBase) { color = cor };
            m.SetFloat("_Smoothness", 0.08f);
            if (brilhoProprio > 0)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", cor * brilhoProprio);
            }
            criados.Add(m);
            return m;
        }

        // ---------- ilha e objetos ----------

        void MontarIlha(VisualDaRegiao v, int semente)
        {
            // O colisor (e o NavMesh) vêm do editor; o relevo da borda muda por região só no visual
            var malha = MalhaDaIlha.Criar(ilha, semente);
            criados.Add(malha);
            GetComponent<MeshFilter>().sharedMesh = malha;
            var chao = NovoMaterial(Color.white);
            chao.mainTexture = TexturaDoChao(v, semente);
            criados.Add(chao.mainTexture);
            GetComponent<MeshRenderer>().sharedMaterials = new[] { chao, NovoMaterial(Cor(v.CorDaRocha) * 0.85f) };
        }

        void MontarObjetos(VisualDaRegiao v, int semente)
        {
            var livres = new List<AreaLivre>();
            foreach (var inimigo in FindObjectsByType<InimigoDoMapa>(FindObjectsSortMode.None))
                livres.Add(new AreaLivre(inimigo.transform.position.x, inimigo.transform.position.z, 3.5f));

            var itens = RegrasDoCenario.Espalhar(ilha, v, semente, livres);
            var malhas = new Malhas(ModelosDoCenario.Partes);
            var obstaculos = new GameObject("Obstaculos").transform;
            obstaculos.SetParent(transform, false);
            foreach (var item in itens)
            {
                ModelosDoCenario.Adicionar(malhas, item, semente);
                if (RegrasDoCenario.Bloqueia(item.Tipo) && item.X * item.X + item.Z * item.Z < ilha.RaioAndavel * ilha.RaioAndavel)
                    CriarObstaculo(obstaculos, item);
            }

            var malha = malhas.Criar("Objetos do Cenario");
            criados.Add(malha);
            var go = new GameObject("Objetos do Cenario", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = malha;
            go.GetComponent<MeshRenderer>().sharedMaterials = new[]
            {
                NovoMaterial(Cor(v.CorDaFolhagem)),
                NovoMaterial(Cor(v.CorDoTronco)),
                NovoMaterial(Cor(v.CorDaRocha)),
                NovoMaterial(Cor(v.CorDoCristal), 1.1f),
                NovoMaterial(Cor(v.CorDaGrama) * 1.15f),
            };
        }

        static void CriarObstaculo(Transform pai, ItemDoCenario item)
        {
            float raio = item.Escala * (item.Tipo switch
            {
                TipoDeItemDoCenario.Ruina => 0.95f,
                TipoDeItemDoCenario.Pedra => 0.9f,
                TipoDeItemDoCenario.Cristal => 0.7f,
                TipoDeItemDoCenario.Pinheiro => 0.45f,
                _ => 0.55f,
            });
            var go = new GameObject(item.Tipo.ToString());
            go.transform.SetParent(pai, false);
            go.transform.position = new Vector3(item.X, item.Y + 1, item.Z);
            var colisor = go.AddComponent<CapsuleCollider>();
            colisor.radius = raio;
            colisor.height = 3;
            var obstaculo = go.AddComponent<NavMeshObstacle>();
            obstaculo.shape = NavMeshObstacleShape.Capsule;
            obstaculo.radius = raio;
            obstaculo.height = 3;
            obstaculo.carving = true;
            obstaculo.carveOnlyStationary = true;
        }

        // ---------- luz, névoa e pós ----------

        void Iluminar(VisualDaRegiao v)
        {
            Color ceu = Cor(v.CorDoCeu), nevoa = Cor(v.CorDaNevoa), grama = Cor(v.CorDaGrama);
            if (sol)
            {
                sol.color = Cor(v.CorDoSol);
                sol.intensity = v.IntensidadeDoSol;
                sol.shadows = LightShadows.Soft;
                sol.shadowStrength = 0.75f;
                sol.transform.rotation = Quaternion.Euler(48, -35, 0);
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ceu * 0.95f;
            RenderSettings.ambientEquatorColor = nevoa * 0.75f;
            RenderSettings.ambientGroundColor = grama * 0.35f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = v.DensidadeDaNevoa;
            RenderSettings.fogColor = nevoa;
            if (camera_)
            {
                camera_.clearFlags = CameraClearFlags.SolidColor;
                camera_.backgroundColor = nevoa;
            }
        }

        void PosProcessamento(VisualDaRegiao v)
        {
            if (!camera_) return;
            camera_.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var perfil = ScriptableObject.CreateInstance<VolumeProfile>();
            criados.Add(perfil);
            var volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = perfil;

            var bloom = perfil.Add<Bloom>(true);
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(0.6f);
            bloom.scatter.Override(0.7f);

            var vinheta = perfil.Add<Vignette>(true);
            vinheta.intensity.Override(0.3f);
            vinheta.smoothness.Override(0.45f);

            var cor = perfil.Add<ColorAdjustments>(true);
            cor.postExposure.Override(0.1f);
            cor.contrast.Override(14f);
            cor.saturation.Override(14f);
            cor.colorFilter.Override(Color.Lerp(Color.white, Cor(v.CorDoSol), 0.25f));

            perfil.Add<Tonemapping>(true).mode.Override(TonemappingMode.Neutral);

            // Desfoque no fundo da tela, o efeito "miniatura" do HD-2D; caro demais para celular
            if (Application.isMobilePlatform) return;
            // a câmera ainda pode não ter chegado na posição no Start: usa a distância configurada nela
            var iso = camera_.GetComponent<CameraIsometrica>();
            float foco = iso ? iso.distancia : 30;
            var desfoque = perfil.Add<DepthOfField>(true);
            desfoque.mode.Override(DepthOfFieldMode.Gaussian);
            desfoque.gaussianStart.Override(foco + 14);
            desfoque.gaussianEnd.Override(foco + 45);
            desfoque.gaussianMaxRadius.Override(0.8f);
        }

        // ---------- nuvens e partículas ----------

        void MontarNuvens(VisualDaRegiao v)
        {
            if (!materialDasNuvens) return;
            var m = new Malhas(1);
            const float lado = 500;
            m.Quad(0, new Vector3(-lado, 0, -lado), new Vector3(-lado, 0, lado), new Vector3(lado, 0, lado), new Vector3(lado, 0, -lado));
            var malha = m.Criar("Mar de Nuvens");
            var uv = new Vector2[malha.vertexCount];
            var vertices = malha.vertices;
            for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2(vertices[i].x, vertices[i].z) / (lado * 2) + Vector2.one * 0.5f;
            malha.uv = uv;
            criados.Add(malha);

            nuvens = new Material(materialDasNuvens) { color = Cor(v.CorDasNuvens), mainTexture = TexturaDasNuvens() };
            nuvens.mainTextureScale = new Vector2(7, 7);
            criados.Add(nuvens);
            criados.Add(nuvens.mainTexture);

            var go = new GameObject("Mar de Nuvens", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, -32, 0);
            go.GetComponent<MeshFilter>().sharedMesh = malha;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = nuvens;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        // Chão em pixel art: grama em três tons com manchas grandes e clareiras de terra; ladrilha sem emenda
        static Texture2D TexturaDoChao(VisualDaRegiao v, int semente)
        {
            const int n = 256;
            Color grama = Cor(v.CorDaGrama), terra = Cor(v.CorDaTerra);
            var t = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
            var px = new Color[n * n];
            int camada = semente % 1000;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float manchas = RuidoSuave(x * 3f / n, y * 3f / n, 3, camada);
                float clareira = RuidoSuave(x * 3f / n, y * 3f / n, 3, camada + 5);
                // pixels de 2x2 para o grão ficar visível de longe
                float grao = Ruido.Valor(x / 2 + camada, y / 2);
                float tom = manchas * 0.6f + grao * 0.4f;
                var c = tom < 0.38f ? grama * 0.86f : tom > 0.62f ? grama * 1.1f : grama;
                if (clareira > 0.72f)
                {
                    float borda = Mathf.InverseLerp(0.72f, 0.78f, clareira);
                    if (grao < borda) c = terra * (0.92f + grao * 0.16f);
                }
                c.a = 1;
                px[y * n + x] = c;
            }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        // Ruído suave que se repete nas bordas, para a textura ladrilhar sem emenda
        static Texture2D TexturaDasNuvens()
        {
            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float v = 0, peso = 0.55f, total = 0;
                for (int o = 0, celulas = 4; o < 4; o++, celulas *= 2, peso *= 0.5f)
                {
                    v += RuidoSuave(x * celulas / (float)n, y * celulas / (float)n, celulas, o) * peso;
                    total += peso;
                }
                v /= total;
                float claro = Mathf.SmoothStep(0.25f, 0.75f, v);
                px[y * n + x] = Color.Lerp(new Color(0.72f, 0.76f, 0.86f), Color.white, claro);
            }
            t.SetPixels(px);
            t.Apply(true);
            return t;
        }

        static float RuidoSuave(float x, float y, int periodo, int camada)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);
            float V(int i, int j) => Ruido.Valor(((i % periodo) + periodo) % periodo + camada * 977, ((j % periodo) + periodo) % periodo);
            return Mathf.Lerp(Mathf.Lerp(V(x0, y0), V(x0 + 1, y0), fx), Mathf.Lerp(V(x0, y0 + 1), V(x0 + 1, y0 + 1), fx), fy);
        }

        void MontarParticulas(VisualDaRegiao v)
        {
            if (v.Particulas == TipoDeParticula.Nenhuma || !materialDasParticulas) return;

            var go = new GameObject("Particulas do Ambiente");
            go.transform.SetParent(transform, false);
            particulas = go.transform;
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var principal = ps.main;
            principal.loop = true;
            principal.simulationSpace = ParticleSystemSimulationSpace.World;
            principal.maxParticles = 150;
            principal.startLifetime = new ParticleSystem.MinMaxCurve(5, 9);
            principal.startSpeed = 0.2f;

            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Box;
            forma.scale = new Vector3(36, 6, 36);
            forma.position = new Vector3(0, 3, 0);

            var ruido = ps.noise;
            ruido.enabled = true;
            ruido.frequency = 0.3f;

            var emissao = ps.emission;
            var vida = ps.colorOverLifetime;
            vida.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.2f), new GradientAlphaKey(1, 0.8f), new GradientAlphaKey(0, 1) });
            vida.color = fade;

            switch (v.Particulas)
            {
                case TipoDeParticula.Folhas:
                    principal.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
                    principal.startColor = new ParticleSystem.MinMaxGradient(Cor(v.CorDaFolhagem) * 1.3f, new Color(0.95f, 0.75f, 0.3f));
                    principal.gravityModifier = 0.02f;
                    principal.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
                    ruido.strength = 0.6f;
                    emissao.rateOverTime = 10;
                    break;
                case TipoDeParticula.Vagalumes:
                    principal.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
                    principal.startColor = new Color(1f, 0.92f, 0.45f);
                    ruido.strength = 0.9f;
                    emissao.rateOverTime = 14;
                    break;
                case TipoDeParticula.Poeira:
                    principal.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.11f);
                    principal.startColor = Color.Lerp(Cor(v.CorDaNevoa), Color.white, 0.5f);
                    ruido.strength = 0.4f;
                    emissao.rateOverTime = 18;
                    break;
            }

            var material = new Material(materialDasParticulas) { mainTexture = TexturaDePonto() };
            criados.Add(material);
            criados.Add(material.mainTexture);
            var renderizador = go.GetComponent<ParticleSystemRenderer>();
            renderizador.sharedMaterial = material;
            renderizador.shadowCastingMode = ShadowCastingMode.Off;
            ps.Play();
        }

        static Texture2D TexturaDePonto()
        {
            const int n = 32;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                float a = Mathf.Clamp01(1 - d);
                px[y * n + x] = new Color(1, 1, 1, a * a);
            }
            t.SetPixels(px);
            t.Apply();
            return t;
        }
    }
}
