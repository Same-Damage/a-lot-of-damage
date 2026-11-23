using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace ALotOfDamage.World
{
    // Ataque no mapa: acertar o monstro antes de ser visto dá ataque preventivo
    public class AtaqueNoMapa : MonoBehaviour
    {
        FluxoDeEncontro encontro;
        IRepositorioDeProgresso repositorio;

        [Inject]
        public void Construir(FluxoDeEncontro encontro, IRepositorioDeProgresso repositorio)
        {
            this.encontro = encontro;
            this.repositorio = repositorio;
        }

        public float alcance = 2f;
        public float recarga = 0.4f;

        float proximoAtaque;
        float golpeVisivel;

        void Update()
        {
            if (encontro.Ativo) return;
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
                if (!e.Disponivel) continue;
                Vector3 d = e.transform.position - transform.position; d.y = 0f;
                if (d.magnitude < menorDistancia) { menorDistancia = d.magnitude; maisPerto = e; }
            }
            if (maisPerto) maisPerto.Enfrentar(RegrasDoEncontro.AoAtacar(maisPerto.Perseguindo));
        }

        // Placa do topo esquerdo com ouro e vitórias, e a lista de pendências em verde embaixo
        void OnGUI()
        {
            var titulo = TemaVisual.EstiloTitulo;
            var progresso = repositorio.Carregar();
            var dentro = TemaVisual.Painel(new Rect(16, 16, 360, 70), null);
            TemaVisual.TextoComContorno(new Rect(dentro.x, dentro.y - 4, dentro.width, 24),
                $"Ouro  {FormatadorDeNumeros.Formatar(progresso.Ouro)}", titulo, TemaVisual.OuroClaro, TextAnchor.UpperLeft, 20);
            TemaVisual.TextoComContorno(new Rect(dentro.x, dentro.y + 22, dentro.width, 22),
                $"Vitórias  {progresso.Vitorias}", titulo, TemaVisual.CorDoTexto, TextAnchor.UpperLeft, 15);

            float y = 166;
            TemaVisual.TextoComContorno(new Rect(20, y, 640, 22), "F / Espaço: atacar (acerte antes de ser visto = ataque preventivo)",
                titulo, TemaVisual.Verde, TextAnchor.UpperLeft, 14);
            if (encontro.EmProtecao)
                TemaVisual.TextoComContorno(new Rect(20, y + 22, 400, 22), "Protegido...", titulo, TemaVisual.Ciano, TextAnchor.UpperLeft, 14);
            if (golpeVisivel > 0)
                TemaVisual.TextoComContorno(new Rect(0, Screen.height * 0.5f - 70, Screen.width, 50), "GOLPE!",
                    TemaVisual.EstiloNumero, TemaVisual.Laranja, TextAnchor.MiddleCenter, 34);
        }
    }
}
