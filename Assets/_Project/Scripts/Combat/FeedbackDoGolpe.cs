using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Combat
{
    // O "suco" de cada golpe: hit stop, tremor do inimigo, partículas e som.
    // A intensidade vem de RegrasDeFeedback; os sons são gerados por código até chegar o áudio de verdade.
    public class FeedbackDoGolpe : MonoBehaviour
    {
        public Transform alvo;

        ParticleSystem particulas;
        AudioSource fonte;
        AudioClip somDoImpacto, somDoCritico, somDaUltimate;

        float pausaAte = -1, tremorAte, forcaDoTremor;
        TipoDeGolpe golpeDaPausa;

        // Awake, e não Start: o combate pode pedir um golpe no mesmo frame em que cria este componente
        void Awake()
        {
            particulas = CriarParticulas();
            fonte = gameObject.AddComponent<AudioSource>();
            fonte.playOnAwake = false;
            somDoImpacto = GerarSom("impacto", 0.08f, 180f, 0.6f);
            somDoCritico = GerarSom("critico", 0.12f, 420f, 0.8f);
            somDaUltimate = GerarSom("ultimate", 0.45f, 90f, 1f);
        }

        public void Golpe(TipoDeGolpe tipo, Color cor)
        {
            int quantidade = RegrasDeFeedback.Particulas(tipo);
            if (particulas && alvo)
            {
                var emitir = new ParticleSystem.EmitParams { position = alvo.position + Vector3.up * 0.3f, startColor = cor, applyShapeToPosition = true };
                particulas.Emit(emitir, quantidade);
            }

            float tremor = RegrasDeFeedback.Tremor(tipo);
            if (tremor > 0 && (Time.unscaledTime >= tremorAte || tremor >= forcaDoTremor))
            {
                forcaDoTremor = tremor;
                tremorAte = Time.unscaledTime + (tipo == TipoDeGolpe.Ultimate ? 0.4f : 0.1f);
            }

            var som = tipo == TipoDeGolpe.Ultimate ? somDaUltimate
                : tipo == TipoDeGolpe.Critico || tipo == TipoDeGolpe.PontoFraco ? somDoCritico
                : tipo == TipoDeGolpe.Click ? somDoImpacto : null;
            if (som) fonte.PlayOneShot(som, 0.6f);

            float pausa = RegrasDeFeedback.HitStop(tipo);
            if (pausa > 0)
            {
                golpeDaPausa = Time.timeScale == 0 ? RegrasDeFeedback.MaisForte(golpeDaPausa, tipo) : tipo;
                pausaAte = Mathf.Max(pausaAte, Time.unscaledTime + RegrasDeFeedback.HitStop(golpeDaPausa));
                Time.timeScale = 0;
            }
        }

        void Update()
        {
            if (pausaAte > 0 && Time.unscaledTime >= pausaAte)
            {
                pausaAte = -1;
                Time.timeScale = 1;
            }

            if (!alvo) return;
            alvo.localPosition = Time.unscaledTime < tremorAte ? (Vector3)Random.insideUnitCircle * forcaDoTremor : Vector3.zero;
        }

        void OnDisable() => Time.timeScale = 1;

        ParticleSystem CriarParticulas()
        {
            var go = new GameObject("Particulas do Golpe");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 300;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.gravityModifier = 1.2f;
            main.useUnscaledTime = true;   // continua no hit stop
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emissao = ps.emission;
            emissao.enabled = false;
            var forma = ps.shape;
            forma.shapeType = ParticleSystemShapeType.Sphere;
            forma.radius = 0.25f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader) renderer.sharedMaterial = new Material(shader);
            return ps;
        }

        // Som curto: ruído com uma nota grave por baixo, caindo rápido
        static AudioClip GerarSom(string nome, float duracao, float frequencia, float ruido)
        {
            const int taxa = 22050;
            int n = Mathf.CeilToInt(duracao * taxa);
            var amostras = new float[n];
            var aleatorio = new System.Random(nome.GetHashCode());
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / taxa;
                float envelope = Mathf.Exp(-t * 6f / duracao);
                float tom = Mathf.Sin(2 * Mathf.PI * frequencia * t * (1 - t / duracao * 0.5f));
                float chiado = (float)(aleatorio.NextDouble() * 2 - 1) * ruido;
                amostras[i] = Mathf.Clamp((tom * 0.6f + chiado * 0.5f) * envelope, -1, 1);
            }
            var clip = AudioClip.Create(nome, n, 1, taxa, false);
            clip.SetData(amostras, 0);
            return clip;
        }
    }
}
