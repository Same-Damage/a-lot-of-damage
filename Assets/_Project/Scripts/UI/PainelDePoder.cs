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

        // Placa escura com moldura dourada e o número grande em itálico, como o "Poder" da ficha
        void OnGUI()
        {
            var numero = TemaVisual.EstiloNumero;
            string valor = FormatadorDeNumeros.Formatar(mostrado);
            float largura = TemaVisual.LarguraDoTexto(valor, numero, 30) + 130;
            var area = new Rect(16, 96, Mathf.Max(240, largura), 56);
            var dentro = TemaVisual.Painel(area, null);

            TemaVisual.TextoComContorno(new Rect(dentro.x, dentro.y - 2, 90, dentro.height + 4), "Poder", numero, TemaVisual.OuroClaro, TextAnchor.MiddleLeft, 20);
            TemaVisual.TextoComContorno(new Rect(dentro.x + 80, dentro.y - 4, area.width, dentro.height + 8), valor, numero, TemaVisual.OuroClaro, TextAnchor.MiddleLeft, 30);

            float t = (Time.unscaledTime - ganhoDesde) / duracaoDoGanho;
            if (t >= 1) return;
            var cor = TemaVisual.Verde;
            cor.a = 1 - t;
            TemaVisual.TextoComContorno(new Rect(area.xMax + 10, area.y + 8 - t * 30, 300, 40), ganho, numero, cor, TextAnchor.MiddleLeft, 26);
        }
    }
}
