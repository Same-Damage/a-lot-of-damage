using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.UI
{
    // Tela provisória de upgrades (OnGUI) até existir o kit de interface
    public class JanelaDeUpgrades : MonoBehaviour
    {
        bool aberta;
        string faixa;
        float faixaAte;

        void Update()
        {
            var k = Keyboard.current;
            if (k != null && k.uKey.wasPressedThisFrame) aberta = !aberta;
        }

        void Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            double antes = ProgressoDoJogador.Valor(d);
            var r = ProgressoDoJogador.Comprar(d, quantidade);
            if (!r.Comprou) return;

            double ganho = ProgressoDoJogador.Valor(d) / antes;
            faixa = r.AtingiuMarco
                ? $"MARCO! {d.Nome} x{FormatadorDeNumeros.Formatar(ganho)}"
                : $"{d.Nome} +{(ganho - 1) * 100:0}%";
            faixaAte = Time.time + (r.AtingiuMarco ? 2.5f : 1.2f);
        }

        void OnGUI()
        {
            var titulo = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            var texto = new GUIStyle(GUI.skin.label) { fontSize = 15 };
            var botao = new GUIStyle(GUI.skin.button) { fontSize = 15 };

            if (GUI.Button(new Rect(Screen.width - 160, 20, 140, 44), aberta ? "Fechar [U]" : "Upgrades [U]", botao))
                aberta = !aberta;

            if (Time.time < faixaAte)
                GUI.Label(new Rect(Screen.width * 0.5f - 300, Screen.height * 0.15f, 600, 60), faixa,
                    new GUIStyle(titulo) { fontSize = 34, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } });

            if (!aberta) return;

            var upgrades = BalanceamentoDeUpgrades.Atual.upgrades;
            float largura = Mathf.Min(760, Screen.width - 40);
            float alturaItem = 110;
            var area = new Rect((Screen.width - largura) / 2, 80, largura, 60 + upgrades.Count * alturaItem);
            GUI.Box(area, "");
            GUI.Label(new Rect(area.x + 16, area.y + 10, largura, 30),
                $"Upgrades     Ouro: {FormatadorDeNumeros.Formatar(ProgressoDoJogador.Ouro)}", titulo);

            for (int i = 0; i < upgrades.Count; i++)
            {
                var d = upgrades[i];
                int nivel = ProgressoDoJogador.Nivel(d.Id);
                float y = area.y + 50 + i * alturaItem;
                bool linear = d.Tipo == TipoDeUpgrade.Linear;
                bool noMaximo = RegrasDeUpgrade.NoMaximo(d, nivel);

                string valor = linear
                    ? $"{RegrasDeUpgrade.Valor(d, nivel) * 100:0}%"
                    : FormatadorDeNumeros.Formatar(RegrasDeUpgrade.Valor(d, nivel));
                GUI.Label(new Rect(area.x + 16, y, 420, 24), $"{d.Nome}  (nível {nivel})", titulo);
                GUI.Label(new Rect(area.x + 16, y + 26, 420, 22), $"{d.Descricao}: {valor}", texto);

                if (noMaximo)
                {
                    GUI.Label(new Rect(area.x + 16, y + 48, 420, 22), "Nível máximo", texto);
                }
                else
                {
                    double custo = RegrasDeUpgrade.Custo(d, nivel);
                    double ganho = RegrasDeUpgrade.GanhoRelativo(d, nivel) * 100;
                    GUI.Label(new Rect(area.x + 16, y + 48, 420, 22),
                        $"Próximo: +{ganho:0.#}%   Custo: {FormatadorDeNumeros.Formatar(custo)}", texto);

                    if (!linear)
                    {
                        // Barra até o próximo marco
                        int marco = RegrasDeUpgrade.ProximoMarco(nivel);
                        int anterior = RegrasDeUpgrade.MarcoAnterior(nivel);
                        float p = Mathf.Clamp01((float)(nivel - anterior) / (marco - anterior));
                        var barra = new Rect(area.x + 16, y + 74, 400, 14);
                        GUI.Box(barra, "");
                        GUI.color = new Color(1f, 0.8f, 0.2f);
                        GUI.Box(new Rect(barra.x, barra.y, barra.width * p, barra.height), "");
                        GUI.color = Color.white;
                        GUI.Label(new Rect(barra.x + 410, y + 70, 200, 22), $"marco no nível {marco}", texto);
                    }

                    float bx = area.x + largura - 250;
                    if (GUI.Button(new Rect(bx, y + 10, 70, 40), "x1", botao)) Comprar(d, 1);
                    if (GUI.Button(new Rect(bx + 78, y + 10, 70, 40), "x10", botao)) Comprar(d, 10);
                    if (GUI.Button(new Rect(bx + 156, y + 10, 80, 40), "Máx", botao)) Comprar(d, RegrasDeUpgrade.Maximo);
                }
            }
        }
    }
}
