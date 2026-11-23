using ALotOfDamage.Core;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.World
{
    // Ataque no mapa: acertar o monstro antes de ser visto dá ataque preventivo
    public class AtaqueNoMapa : MonoBehaviour
    {
        public float alcance = 2f;
        public float recarga = 0.4f;

        float proximoAtaque;
        float golpeVisivel;

        void Update()
        {
            if (EstadoDoEncontro.Ativo) return;
            if (golpeVisivel > 0) golpeVisivel -= Time.deltaTime;

            if (AtaquePedido() && Time.time >= proximoAtaque)
            {
                proximoAtaque = Time.time + recarga;
                golpeVisivel = 0.15f;
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
                Vector3 d = e.transform.position - transform.position; d.y = 0f;
                if (d.magnitude < menorDistancia) { menorDistancia = d.magnitude; maisPerto = e; }
            }
            if (maisPerto) maisPerto.Enfrentar(RegrasDoEncontro.AoAtacar(maisPerto.Perseguindo));
        }

        void OnGUI()
        {
            var estilo = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            var pequeno = new GUIStyle(estilo) { fontSize = 14, fontStyle = FontStyle.Normal };
            GUI.Label(new Rect(20, 20, 700, 30),
                $"Ouro {FormatadorDeNumeros.Formatar(ProgressoDoJogador.Ouro)}   Dano x{FormatadorDeNumeros.Formatar(ProgressoDoJogador.MultiplicadorDeDano)}   Vitórias {ProgressoDoJogador.Vitorias}", estilo);
            GUI.Label(new Rect(20, 50, 700, 30), "F / Espaço: atacar (acerte antes de ser visto = ataque preventivo)", pequeno);
            if (EstadoDoEncontro.EmProtecao)
                GUI.Label(new Rect(20, 75, 400, 30), "Protegido...", pequeno);
            if (golpeVisivel > 0)
                GUI.Label(new Rect(Screen.width * 0.5f - 30, Screen.height * 0.5f - 60, 100, 40), "GOLPE!", estilo);
        }
    }
}
