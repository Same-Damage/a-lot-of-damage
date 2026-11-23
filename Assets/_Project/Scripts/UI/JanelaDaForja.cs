using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace ALotOfDamage.UI
{
    // Tela provisória da forja (OnGUI) até existir o kit de interface
    public class JanelaDaForja : JanelaProvisoria
    {
        const float AlturaItem = 90;

        protected override string Titulo => "Forja";
        protected override Key Tecla => Key.J;
        protected override int Ordem => 1;
        protected override float AlturaDoConteudo => catalogo.Itens.Count * AlturaItem;

        ForjarItem forjarItem;

        [Inject]
        public void Construir(ForjarItem forjarItem) => this.forjarItem = forjarItem;

        void Forjar(DefinicaoDeItem d)
        {
            var r = forjarItem.Executar(d.Id);
            if (!r.Forjou) return;
            MostrarFaixa(r.SucessoCritico
                ? $"SUCESSO CRÍTICO! {d.Nome} +{r.NovoNivel}"
                : $"{d.Nome} +{r.NovoNivel}", r.SucessoCritico);
        }

        protected override void DesenharConteudo(Rect area)
        {
            var itens = catalogo.Itens;
            for (int i = 0; i < itens.Count; i++)
            {
                var d = itens[i];
                int nivel = Progresso.NivelDoItem(d.Id);
                float y = area.y + i * AlturaItem;
                Separador(new Rect(area.x + 6, y - 6, area.width - 12, AlturaItem - 6));
                TemaVisual.Slot(new Rect(area.x + 16, y, 58, 58), $"+{nivel}", nivel > 0 ? TemaVisual.Laranja : TemaVisual.CorDoTexto);
                string material = catalogo.NomeDoMaterial(d.MaterialId);

                GUI.Label(new Rect(area.x + 86, y, 420, 24), $"{d.Nome} +{nivel}", EstiloTitulo);
                GUI.Label(new Rect(area.x + 86, y + 26, 460, 22),
                    $"{d.Descricao}: x{FormatadorDeNumeros.Formatar(RegrasDaForja.Multiplicador(d, nivel))}" +
                    $"   próximo: x{1 + d.BonusPorNivel:0.##}   crítico {d.ChanceDeSucessoCritico * 100:0}%", EstiloTexto);
                GUI.Label(new Rect(area.x + 86, y + 48, 460, 22),
                    $"Custo: {FormatadorDeNumeros.Formatar(RegrasDaForja.CustoEmOuro(d, nivel))} ouro + " +
                    $"{RegrasDaForja.CustoEmMaterial(d, nivel)} {material} (tem {Progresso.Material(d.MaterialId)})", EstiloTexto);

                if (TemaVisual.Botao(new Rect(area.x + area.width - 140, y + 10, 120, 40), "Forjar")) Forjar(d);
            }
        }
    }
}
