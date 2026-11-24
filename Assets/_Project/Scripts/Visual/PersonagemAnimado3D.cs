using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Personagem 3D com esqueleto humanoide (rig do Mixamo), animado pelo Animator montado no ConstrutorDeCenas.
    // Fica no objeto do modelo: gira o modelo para onde anda e passa a locomoção para a árvore de animação.
    [RequireComponent(typeof(Animator))]
    public class PersonagemAnimado3D : PersonagemVisual
    {
        public const string ParametroDeLocomocao = "Locomocao";
        public const string ParametroDoGolpe = "Golpe";
        public const string ParametroDoDano = "Dano";
        public const string ParametroDaVelocidadeDoGolpe = "VelocidadeDoGolpe";

        [Tooltip("Graus por segundo ao virar para a direção do movimento")]
        public float velocidadeDeGiro = 720f;
        [Tooltip("Segundos para a locomoção chegar ao valor novo (suaviza parar e arrancar)")]
        public float suavizacao = 0.1f;

        static readonly int Locomocao = Animator.StringToHash(ParametroDeLocomocao);
        static readonly int Golpe = Animator.StringToHash(ParametroDoGolpe);
        static readonly int Dano = Animator.StringToHash(ParametroDoDano);
        static readonly int VelocidadeDoGolpe = Animator.StringToHash(ParametroDaVelocidadeDoGolpe);

        Animator animator;
        Quaternion rumoAlvo;

        void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
            rumoAlvo = transform.rotation;
        }

        public override void Mover(Vector3 velocidadeNoMundo, float velocidadeMaxima, Vector2 entrada)
        {
            velocidadeNoMundo.y = 0;
            float velocidade = velocidadeNoMundo.magnitude;
            animator.SetFloat(Locomocao, RegrasDeAnimacao.Locomocao(velocidade, velocidadeMaxima), suavizacao, Time.deltaTime);
            if (velocidade >= RegrasDeAnimacao.LimiarDeMovimento)
                rumoAlvo = Quaternion.Euler(0, RegrasDeAnimacao.Rumo(velocidadeNoMundo.x, velocidadeNoMundo.z), 0);
        }

        public override void Golpear(float velocidade = 1f)
        {
            animator.SetFloat(VelocidadeDoGolpe, Mathf.Max(0.1f, velocidade));
            animator.SetTrigger(Golpe);
        }

        public override void LevarDano() => animator.SetTrigger(Dano);

        void Update()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rumoAlvo, velocidadeDeGiro * Time.deltaTime);
        }
    }
}
