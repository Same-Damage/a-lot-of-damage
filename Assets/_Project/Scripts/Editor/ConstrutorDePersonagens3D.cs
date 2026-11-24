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
    //   <nome>_Malha.asset  opcional: malha com cabeça própria (submalhas corpo e cabeça) no esqueleto do FBX,
    //                       com a textura da cabeça em <nome>_Cabeca_Cor.png
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
            var malhaComCabeca = AssetDatabase.LoadAssetAtPath<Mesh>($"{Pasta(nome)}/{nome}_Malha.asset");
            float alturaDoModelo = 0;
            foreach (var r in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
                if (malhaComCabeca)
                {
                    r.sharedMesh = malhaComCabeca;
                    r.sharedMaterials = new[] { material, MaterialEm(Pasta(nome), $"{nome}_Cabeca") };
                }
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // a sombra é a SombraDeContato
                r.updateWhenOffscreen = false;
                alturaDoModelo = Mathf.Max(alturaDoModelo, r.sharedMesh.bounds.size.y * r.transform.lossyScale.y);
            }

            var animator = go.GetComponent<Animator>() ?? go.AddComponent<Animator>();
            // as peças entram antes da escala, no tamanho real do modelo, e crescem junto com ele
            if (Equipamento.TryGetValue(nome, out var encaixes))
            {
                // o encaixe é medido na pose parada (a pose padrão de cada FBX varia): braços caídos, mãos ao lado do corpo
                // (clipe humanoide só posa os ossos pelo AnimationMode; ao sair, os ossos voltam e as peças ficam com o encaixe local)
                var parada = Clipe(nome, "Parada");
                bool amostrou = parada && !AnimationMode.InAnimationMode();
                if (amostrou)
                {
                    AnimationMode.StartAnimationMode();
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(go, parada, 0);
                    AnimationMode.EndSampling();
                }
                var pecas = new System.Collections.Generic.List<(Transform t, Vector3 pos, Quaternion rot)>();
                foreach (var e in encaixes)
                {
                    var t = Encaixar(animator, e);
                    if (t) pecas.Add((t, t.localPosition, t.localRotation));
                }
                if (amostrou)
                {
                    AnimationMode.StopAnimationMode();
                    foreach (var (t, pos, rot) in pecas) { t.localPosition = pos; t.localRotation = rot; }
                }
                // ajuste feito à mão no editor (menu "Salvar encaixes das peças") vale mais que a conta
                foreach (var (t, _, _) in pecas)
                {
                    var salvo = EncaixesSalvos.Buscar(nome, t.name);
                    if (salvo == null) continue;
                    t.localPosition = salvo.Posicao;
                    t.localRotation = salvo.Rotacao;
                    t.localScale = salvo.Escala;
                }
            }
            if (alturaDoModelo > 0) go.transform.localScale *= altura / alturaDoModelo;

            animator.runtimeAnimatorController = Controlador(nome);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            return go.AddComponent<PersonagemAnimado3D>();
        }

        static string Pasta(string nome) => $"{PastaDosPersonagens}/{nome}";

        const string PastaDoEquipamento = "Assets/_Project/Art/Equipment";

        // Onde cada peça vai, medido na pose parada no referencial do corpo (+z para a frente, +x para a direita, +y para cima),
        // em metros de um humano de 1,7 m: "pega" é o ponto da peça que fica na palma ou nas costas (em coordenadas da peça)
        // e "afastamento" leva o osso até esse ponto
        struct Encaixe
        {
            public string Peca;
            public HumanBodyBones Osso;
            public Vector3 Pega, Afastamento, Rotacao;
        }

        static readonly System.Collections.Generic.Dictionary<string, Encaixe[]> Equipamento = new()
        {
            ["Arqueira"] = new[]
            {
                // arco em pé na mão esquerda, ao lado da perna
                new Encaixe { Peca = "Arco", Osso = HumanBodyBones.LeftHand, Pega = Vector3.zero, Afastamento = new Vector3(0, -0.08f, 0.02f), Rotacao = new Vector3(0, 90, 0) },
                // aljava atravessada nas costas, com as flechas por cima do ombro direito
                new Encaixe { Peca = "Aljava", Osso = HumanBodyBones.UpperChest, Pega = Vector3.zero, Afastamento = new Vector3(0, -0.05f, -0.17f), Rotacao = new Vector3(0, 0, -30) },
            },
            ["Cavaleiro"] = new[]
            {
                // espada na mão direita, lâmina para a frente e um pouco para baixo
                new Encaixe { Peca = "Espada", Osso = HumanBodyBones.RightHand, Pega = new Vector3(0, 0.36f, 0), Afastamento = new Vector3(0, -0.08f, 0.03f), Rotacao = new Vector3(-60, 0, 0) },
            },
        };

        // Distância do quadril à cabeça num humano de 1,7 m: as peças são modeladas nessa escala
        const float DoQuadrilACabeca = 0.62f;

        static Transform Encaixar(Animator animator, Encaixe e)
        {
            var osso = animator.GetBoneTransform(e.Osso);
            if (!osso && e.Osso == HumanBodyBones.UpperChest) osso = animator.GetBoneTransform(HumanBodyBones.Chest);
            var modelo = Peca(e.Peca);
            if (!osso || !modelo) { Debug.LogWarning($"Sem osso {e.Osso} ou peça {e.Peca} para {animator.name}"); return null; }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(modelo);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = e.Peca;
            // referencial do corpo tirado dos ossos (o nó raiz de cada FBX pode vir girado)
            var cima = (animator.GetBoneTransform(HumanBodyBones.Head).position - animator.GetBoneTransform(HumanBodyBones.Hips).position).normalized;
            var direita = (animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position - animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position).normalized;
            var corpo = Quaternion.LookRotation(Vector3.Cross(direita, cima), cima);
            float escala = Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.Head).position, animator.GetBoneTransform(HumanBodyBones.Hips).position) / DoQuadrilACabeca;
            var rotacao = corpo * Quaternion.Euler(e.Rotacao);
            // o FBX de uma malha só vem com o giro de eixo (Blender z-para-cima) no próprio nó raiz: ele é mantido
            go.transform.localScale = modelo.transform.localScale * escala;
            go.transform.SetPositionAndRotation(
                osso.position + corpo * e.Afastamento * escala - rotacao * e.Pega * escala, rotacao * modelo.transform.localRotation);
            go.transform.SetParent(osso, true);
            var material = MaterialEm($"{PastaDoEquipamento}/{e.Peca}", e.Peca);
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials = Enumerable.Repeat(material, r.sharedMaterials.Length).ToArray();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            go.AddComponent<PecaDeEquipamento>();
            return go.transform;
        }

        // Peça rígida de Art/Equipment/<peça>/ (<peça>.fbx e <peça>_Cor.png), com o mesmo shader pintado dos heróis
        static GameObject Peca(string peca)
        {
            string pasta = $"{PastaDoEquipamento}/{peca}";
            var imp = (ModelImporter)AssetImporter.GetAtPath($"{pasta}/{peca}.fbx");
            if (imp == null) return null;
            if (imp.materialImportMode != ModelImporterMaterialImportMode.None || imp.importAnimation)
            {
                imp.materialImportMode = ModelImporterMaterialImportMode.None;
                imp.importAnimation = false;
                imp.animationType = ModelImporterAnimationType.None;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{pasta}/{peca}.fbx");
        }

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
                // Nos ciclos, o deslocamento e o giro do quadril saem como root motion, que é descartado
                // (applyRootMotion desligado). Se ficassem na pose, uma corrida que não é "In Place" ou que faz
                // curva levaria o corpo para o lado ao longo do ciclo e daria um tranco de volta a cada repetição.
                origem.lockRootRotation = !emLaco;
                origem.lockRootHeightY = true;
                origem.lockRootPositionXZ = !emLaco;
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

        static Material Material(string nome) => MaterialEm(Pasta(nome), nome);

        // <pasta>/<nome>.mat com o shader pintado e a textura <nome>_Cor.png
        static Material MaterialEm(string pasta, string nome)
        {
            string caminho = $"{pasta}/{nome}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(caminho);
            if (!m)
            {
                m = new Material(UnityEngine.Shader.Find(Shader));
                AssetDatabase.CreateAsset(m, caminho);
            }
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{pasta}/{nome}_Cor.png");
            if (tex) m.SetTexture("_BaseMap", tex);
            EditorUtility.SetDirty(m);
            return m;
        }
    }
}
