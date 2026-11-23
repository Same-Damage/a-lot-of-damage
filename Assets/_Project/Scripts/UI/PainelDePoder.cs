using ALotOfDamage.Core;
using UnityEngine;
using VContainer;

namespace ALotOfDamage.UI
{
    // Poder Total em destaque, com contador animado e o ganho (+38%, ×4) subindo ao lado.
    // Provisório em OnGUI até existir o kit de interface.
    public class PainelDePoder : MonoBehaviour
    {
        public float rapidezDoContador = 4f;
        public float intervaloDeConsulta = 0.2f;
        public float duracaoDoGanho = 1.6f;

        CalcularPoder calcularPoder;
        double alvo, mostrado;
        float proximaConsulta;
        string ganho;
        float ganhoDesde = -10;
        GUIStyle estiloPoder, estiloGanho;

        [Inject]
        public void Construir(CalcularPoder calcularPoder) => this.calcularPoder = calcularPoder;

        void Start()
        {
            alvo = mostrado = calcularPoder.Executar();
        }

        void Update()
        {
            if (Time.unscaledTime >= proximaConsulta)
            {
                proximaConsulta = Time.unscaledTime + intervaloDeConsulta;
                double novo = calcularPoder.Executar();
                string texto = RegrasDePoder.TextoDoGanho(alvo, novo);
                if (texto != null) { ganho = texto; ganhoDesde = Time.unscaledTime; }
                alvo = novo;
            }
            mostrado = RegrasDePoder.ContadorAnimado(mostrado, alvo, Time.unscaledDeltaTime, rapidezDoContador);
        }

        void OnGUI()
        {
            estiloPoder ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 30, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.82f, 0.3f) },
            };
            estiloGanho ??= new GUIStyle(estiloPoder) { fontSize = 26 };

            var area = new Rect(20, 100, 520, 44);
            GUI.Label(area, $"Poder {FormatadorDeNumeros.Formatar(mostrado)}", estiloPoder);

            float t = (Time.unscaledTime - ganhoDesde) / duracaoDoGanho;
            if (t >= 1) return;
            var cor = new Color(0.45f, 1f, 0.5f, 1 - t);
            estiloGanho.normal.textColor = cor;
            float largura = estiloPoder.CalcSize(new GUIContent($"Poder {FormatadorDeNumeros.Formatar(mostrado)}")).x;
            GUI.Label(new Rect(area.x + largura + 14, area.y - t * 30, 300, 40), ganho, estiloGanho);
        }
    }
}
