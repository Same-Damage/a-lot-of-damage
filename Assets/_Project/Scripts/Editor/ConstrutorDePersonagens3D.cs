using System.Linq;
using ALotOfDamage.Visual;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ALotOfDamage.Ferramentas
{
    // Monta personagens 3D a partir dos FBX do Mixamo em Art/Characters/<nome>/:
    //   <nome>.fbx         modelo com pele e esqueleto (vira o avatar humanoide)
    //   <nome>@<Clipe>.fbx  uma animação por arquivo (Parada, Andar, Correr, Golpe, Dano, Morte)
    //   <nome>_Cor.png      textura de cor
    public static class ConstrutorDePersonagens3D
    {
        const string PastaDosPersonagens = "Assets/_Project/Art/Characters";
        const string Shader = "ALotOfDamage/PersonagemPintado";

        static readonly string[] ClipesEmLaco = { "Parada", "Andar", "Correr" };
        static readonly string[] Clipes = { "Parada", "Andar", "Correr", "Golpe", "Dano", "Morte" };

        // Personagem pronto para a cena, com os pés em "pes" e a altura dada (unidades do mundo)
        public static PersonagemAnimado3D Criar(string nome, Transform pai, Vector3 pes, float altura)
        {
            ConfigurarImportacao(nome);
            var modelo = AssetDatabase.LoadAssetAtPath<GameObject>($"{Pasta(nome)}/{nome}.fbx");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
            // solta do FBX: a cena guarda o que foi montado aqui, não uma variante do modelo
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = nome;
            if (pai) go.transform.SetParent(pai, false);
            go.transform.localPosition = pes;

            var material = Material(nome);
            float alturaDoModelo = 0;
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // a sombra é a SombraDeContato
                r.updateWhenOffscreen = false;
                alturaDoModelo = Mathf.Max(alturaDoModelo, r.sharedMesh.bounds.size.y * r.transform.lossyScale.y);
            }
            if (alturaDoModelo > 0) go.transform.localScale *= altura / alturaDoModelo;

            var animator = go.GetComponent<Animator>() ?? go.AddComponent<Animator>();
            animator.runtimeAnimatorController = Controlador(nome);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            return go.AddComponent<PersonagemAnimado3D>();
        }

        static string Pasta(string nome) => $"{PastaDosPersonagens}/{nome}";

        // Humanoide com avatar próprio no modelo; as animações usam o avatar dele e ficam no lugar (sem root motion)
        static void ConfigurarImportacao(string nome)
        {
            string caminhoDoModelo = $"{Pasta(nome)}/{nome}.fbx";
            var modelo = (ModelImporter)AssetImporter.GetAtPath(caminhoDoModelo);
            if (modelo.animationType != ModelImporterAnimationType.Human || modelo.importAnimation)
            {
                modelo.animationType = ModelImporterAnimationType.Human;
                modelo.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                modelo.importAnimation = false;
                modelo.materialImportMode = ModelImporterMaterialImportMode.None;
                modelo.SaveAndReimport();
            }
            var avatar = AssetDatabase.LoadAllAssetsAtPath(caminhoDoModelo).OfType<Avatar>().First();

            foreach (var clipe in Clipes)
            {
                var imp = (ModelImporter)AssetImporter.GetAtPath($"{Pasta(nome)}/{nome}@{clipe}.fbx");
                if (imp == null) continue;
                bool emLaco = ClipesEmLaco.Contains(clipe);
                imp.animationType = ModelImporterAnimationType.Human;
                imp.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                imp.sourceAvatar = avatar;
                imp.importAnimation = true;
                imp.materialImportMode = ModelImporterMaterialImportMode.None;
                var origem = imp.defaultClipAnimations[0];
                origem.name = clipe;
                origem.loopTime = emLaco;
                origem.loopPose = emLaco;
                origem.lockRootRotation = true;
                origem.lockRootHeightY = true;
                origem.lockRootPositionXZ = true;
                origem.keepOriginalOrientation = true;
                origem.keepOriginalPositionY = true;
                origem.keepOriginalPositionXZ = true;
                imp.clipAnimations = new[] { origem };
                imp.SaveAndReimport();
            }
        }

        static AnimationClip Clipe(string nome, string clipe) =>
            AssetDatabase.LoadAllAssetsAtPath($"{Pasta(nome)}/{nome}@{clipe}.fbx")
                .OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));

        // Locomoção numa árvore só (parada → andar → correr pelo parâmetro Locomocao); golpe e dano por gatilho, voltando sozinhos
        static AnimatorController Controlador(string nome)
        {
            string caminho = $"{Pasta(nome)}/{nome}.controller";
            AssetDatabase.DeleteAsset(caminho);
            var c = AnimatorController.CreateAnimatorControllerAtPath(caminho);
            c.AddParameter(PersonagemAnimado3D.ParametroDeLocomocao, AnimatorControllerParameterType.Float);
            c.AddParameter(PersonagemAnimado3D.ParametroDoGolpe, AnimatorControllerParameterType.Trigger);
            c.AddParameter(PersonagemAnimado3D.ParametroDoDano, AnimatorControllerParameterType.Trigger);
            c.AddParameter(new AnimatorControllerParameter
            {
                name = PersonagemAnimado3D.ParametroDaVelocidadeDoGolpe, type = AnimatorControllerParameterType.Float, defaultFloat = 1,
            });
            var sm = c.layers[0].stateMachine;

            var locomocao = c.CreateBlendTreeInController("Locomocao", out var arvore);
            arvore.blendParameter = PersonagemAnimado3D.ParametroDeLocomocao;
            arvore.useAutomaticThresholds = false;
            // sem Correr, o andar vai até o fim da árvore (só acelera, sem trocar de passo)
            var correr = Clipe(nome, "Correr");
            arvore.AddChild(Clipe(nome, "Parada"), 0f);
            arvore.AddChild(Clipe(nome, "Andar"), correr ? 0.5f : 1f);
            if (correr) arvore.AddChild(correr, 1f);
            sm.defaultState = locomocao;

            Acao(sm, locomocao, Clipe(nome, "Golpe"), PersonagemAnimado3D.ParametroDoGolpe, PersonagemAnimado3D.ParametroDaVelocidadeDoGolpe);
            Acao(sm, locomocao, Clipe(nome, "Dano"), PersonagemAnimado3D.ParametroDoDano, null);
            AssetDatabase.SaveAssets();
            return c;
        }

        static void Acao(AnimatorStateMachine sm, AnimatorState volta, AnimationClip clipe, string gatilho, string velocidade)
        {
            if (!clipe) return;
            var estado = sm.AddState(gatilho);
            estado.motion = clipe;
            if (velocidade != null) { estado.speedParameterActive = true; estado.speedParameter = velocidade; }
            var entrada = sm.AddAnyStateTransition(estado);
            entrada.AddCondition(AnimatorConditionMode.If, 0, gatilho);
            entrada.duration = 0.08f;
            entrada.hasExitTime = false;
            entrada.canTransitionToSelf = true;
            var saida = estado.AddTransition(volta);
            saida.hasExitTime = true;
            saida.exitTime = 0.85f;
            saida.duration = 0.15f;
        }

        static Material Material(string nome)
        {
            string caminho = $"{Pasta(nome)}/{nome}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (!m)
            {
                m = new Material(UnityEngine.Shader.Find(Shader));
                AssetDatabase.CreateAsset(m, caminho);
            }
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Pasta(nome)}/{nome}_Cor.png");
            if (tex) m.SetTexture("_BaseMap", tex);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
