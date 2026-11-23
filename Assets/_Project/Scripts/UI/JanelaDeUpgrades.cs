using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.UI
{
    // Tela provisória de upgrades (OnGUI) até existir o kit de interface
    public class JanelaDeUpgrades : JanelaProvisoria
    {
        const float AlturaItem = 110;

        protected override string Titulo => "Upgrades";
        protected override Key Tecla => Key.U;
        protected override int Ordem => 0;
        protected override float AlturaDoConteudo => BalanceamentoDeUpgrades.Atual.upgrades.Count * AlturaItem;

        void Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            double antes = ProgressoDoJogador.Valor(d);
            var r = ProgressoDoJogador.Comprar(d, quantidade);
            if (!r.Comprou) return;

            double ganho = ProgressoDoJogador.Valor(d) / antes;
            MostrarFaixa(r.AtingiuMarco
                ? $"MARCO! {d.Nome} x{FormatadorDeNumeros.Formatar(ganho)}"
                : $"{d.Nome} +{(ganho - 1) * 100:0}%", r.AtingiuMarco);
        }

        protected override void DesenharConteudo(Rect area)
        {
            var upgrades = BalanceamentoDeUpgrades.Atual.upgrades;
            for (int i = 0; i < upgrades.Count; i++)
            {
                var d = upgrades[i];
                int nivel = ProgressoDoJogador.Nivel(d.Id);
                float y = area.y + i * AlturaItem;
                bool linear = d.Tipo == TipoDeUpgrade.Linear;

                string valor = linear
                    ? $"{RegrasDeUpgrade.Valor(d, nivel) * 100:0}%"
                    : FormatadorDeNumeros.Formatar(RegrasDeUpgrade.Valor(d, nivel));
                GUI.Label(new Rect(area.x + 16, y, 420, 24), $"{d.Nome}  (nível {nivel})", EstiloTitulo);
                GUI.Label(new Rect(area.x + 16, y + 26, 420, 22), $"{d.Descricao}: {valor}", EstiloTexto);

                if (RegrasDeUpgrade.NoMaximo(d, nivel))
                {
                    GUI.Label(new Rect(area.x + 16, y + 48, 420, 22), "Nível máximo", EstiloTexto);
                    continue;
                }

                double custo = RegrasDeUpgrade.Custo(d, nivel);
                double ganho = RegrasDeUpgrade.GanhoRelativo(d, nivel) * 100;
                GUI.Label(new Rect(area.x + 16, y + 48, 420, 22),
                    $"Próximo: +{ganho:0.#}%   Custo: {FormatadorDeNumeros.Formatar(custo)}", EstiloTexto);

                if (!linear)
                {
                    int marco = RegrasDeUpgrade.ProximoMarco(nivel);
                    int anterior = RegrasDeUpgrade.MarcoAnterior(nivel);
                    Barra(new Rect(area.x + 16, y + 74, 300, 14), (float)(nivel - anterior) / (marco - anterior), new Color(1f, 0.8f, 0.2f));
                    GUI.Label(new Rect(area.x + 324, y + 70, 200, 22), $"marco no nível {marco}", EstiloTexto);
                }

                float bx = area.x + area.width - 250;
                if (GUI.Button(new Rect(bx, y + 10, 70, 40), "x1", EstiloBotao)) Comprar(d, 1);
                if (GUI.Button(new Rect(bx + 78, y + 10, 70, 40), "x10", EstiloBotao)) Comprar(d, 10);
                if (GUI.Button(new Rect(bx + 156, y + 10, 80, 40), "Máx", EstiloBotao)) Comprar(d, RegrasDeUpgrade.Maximo);
            }
        }
    }
}
