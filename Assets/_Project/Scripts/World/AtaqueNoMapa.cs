using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using ALotOfDamage.Visual;
using VContainer;

namespace ALotOfDamage.World
{
    // Ataque no mapa: acertar o monstro antes de ser visto dá ataque preventivo
    public class AtaqueNoMapa : MonoBehaviour
    {
        FluxoDeEncontro encontro;

        [Inject]
        public void Construir(FluxoDeEncontro encontro) => this.encontro = encontro;

        public float alcance = 2f;
        public float recarga = 0.4f;
        public PersonagemVisual boneco;

        float proximoAtaque;

        void Update()
        {
            if (encontro.Ativo) return;

            if (AtaquePedido() && Time.time >= proximoAtaque)
            {
                proximoAtaque = Time.time + recarga;
                if (boneco) boneco.Golpear();
                TentarAcertar();
            }
        }

        static bool AtaquePedido()
        {
            var k = Keyboard.current;
            if (k != null && (k.fKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)) return true;
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) return true;
            return ControlesDeToque.ConsumirAtaque();
        }

        void TentarAcertar()
        {
            InimigoDoMapa maisPerto = null;
            float menorDistancia = alcance;
            foreach (var e in FindObjectsByType<InimigoDoMapa>(FindObjectsSortMode.None))
            {
                if (!e.Disponivel) continue;
                Vector3 d = e.transform.position - transform.position; d.y = 0f;
                if (d.magnitude < menorDistancia) { menorDistancia = d.magnitude; maisPerto = e; }
            }
            if (maisPerto) maisPerto.Enfrentar(RegrasDoEncontro.AoAtacar(maisPerto.Perseguindo));
        }

    }
}
