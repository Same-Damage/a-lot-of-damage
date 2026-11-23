using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace ALotOfDamage.UI
{
    // Tela provisória de upgrades (OnGUI) até existir o kit de interface
    public class JanelaDeUpgrades : JanelaProvisoria
    {
        const float AlturaItem = 110;

        protected override string Titulo => "Upgrades";
        protected override Key Tecla => Key.U;
        protected override int Ordem => 0;
        protected override float AlturaDoConteudo => catalogo.Upgrades.Count * AlturaItem;

        ComprarUpgrade comprarUpgrade;

        [Inject]
        public void Construir(ComprarUpgrade comprarUpgrade) => this.comprarUpgrade = comprarUpgrade;

        void Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            double antes = Progresso.Valor(d);
            var r = comprarUpgrade.Executar(d.Id, quantidade);
            if (!r.Comprou) return;

            double ganho = Progresso.Valor(d) / antes;
            MostrarFaixa(r.AtingiuMarco
                ? $"MARCO! {d.Nome} x{FormatadorDeNumeros.Formatar(ganho)}"
                : $"{d.Nome} +{(ganho - 1) * 100:0}%", r.AtingiuMarco);
        }

        protected override void DesenharConteudo(Rect area)
        {
            var upgrades = catalogo.Upgrades;
            for (int i = 0; i < upgrades.Count; i++)
            {
                var d = upgrades[i];
                int nivel = Progresso.Nivel(d.Id);
                float y = area.y + i * AlturaItem;
                Separador(new Rect(area.x + 6, y - 6, area.width - 12, AlturaItem - 6));
                TemaVisual.Slot(new Rect(area.x + 16, y, 58, 58), TemaVisual.Inicial(d.Nome), TemaVisual.OuroClaro);
                bool linear = d.Tipo == TipoDeUpgrade.Linear;

                string valor = linear
                    ? $"{RegrasDeUpgrade.Valor(d, nivel) * 100:0}%"
                    : FormatadorDeNumeros.Formatar(RegrasDeUpgrade.Valor(d, nivel));
                GUI.Label(new Rect(area.x + 86, y, 420, 24), $"{d.Nome}  (nível {nivel})", EstiloTitulo);
                GUI.Label(new Rect(area.x + 86, y + 26, 420, 22), $"{d.Descricao}: {valor}", EstiloTexto);

                if (RegrasDeUpgrade.NoMaximo(d, nivel))
                {
                    GUI.Label(new Rect(area.x + 86, y + 48, 420, 22), "Nível máximo", EstiloTexto);
                    continue;
                }

                double custo = RegrasDeUpgrade.Custo(d, nivel);
                double ganho = RegrasDeUpgrade.GanhoRelativo(d, nivel) * 100;
                GUI.Label(new Rect(area.x + 86, y + 48, 420, 22),
                    $"Próximo: +{ganho:0.#}%   Custo: {FormatadorDeNumeros.Formatar(custo)}", EstiloTexto);

                if (!linear)
                {
                    int marco = RegrasDeUpgrade.ProximoMarco(nivel);
                    int anterior = RegrasDeUpgrade.MarcoAnterior(nivel);
                    Barra(new Rect(area.x + 86, y + 74, 300, 14), (float)(nivel - anterior) / (marco - anterior), TemaVisual.Ouro);
                    GUI.Label(new Rect(area.x + 394, y + 70, 200, 22), $"marco no nível {marco}", EstiloTexto);
                }

                float bx = area.x + area.width - 250;
                if (TemaVisual.Botao(new Rect(bx, y + 10, 70, 40), "x1")) Comprar(d, 1);
                if (TemaVisual.Botao(new Rect(bx + 78, y + 10, 70, 40), "x10")) Comprar(d, 10);
                if (TemaVisual.Botao(new Rect(bx + 156, y + 10, 80, 40), "Máx")) Comprar(d, RegrasDeUpgrade.Maximo);
            }
        }
    }
}
