using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.UI
{
    // Tela provisória da forja (OnGUI) até existir o kit de interface
    public class JanelaDaForja : JanelaProvisoria
    {
        const float AlturaItem = 90;

        protected override string Titulo => "Forja";
        protected override Key Tecla => Key.J;
        protected override int Ordem => 1;
        protected override float AlturaDoConteudo => BalanceamentoDaForja.Atual.itens.Count * AlturaItem;

        void Forjar(DefinicaoDeItem d)
        {
            var r = ProgressoDoJogador.Forjar(d, Random.value);
            if (!r.Forjou) return;
            MostrarFaixa(r.SucessoCritico
                ? $"SUCESSO CRÍTICO! {d.Nome} +{r.NovoNivel}"
                : $"{d.Nome} +{r.NovoNivel}", r.SucessoCritico);
        }

        protected override void DesenharConteudo(Rect area)
        {
            var itens = BalanceamentoDaForja.Atual.itens;
            var bestiario = BalanceamentoDoBestiario.Atual;
            for (int i = 0; i < itens.Count; i++)
            {
                var d = itens[i];
                int nivel = ProgressoDoJogador.NivelDoItem(d.Id);
                float y = area.y + i * AlturaItem;
                string material = bestiario.NomeDoMaterial(d.MaterialId);

                GUI.Label(new Rect(area.x + 16, y, 420, 24), $"{d.Nome} +{nivel}", EstiloTitulo);
                GUI.Label(new Rect(area.x + 16, y + 26, 460, 22),
                    $"{d.Descricao}: x{FormatadorDeNumeros.Formatar(RegrasDaForja.Multiplicador(d, nivel))}" +
                    $"   próximo: x{1 + d.BonusPorNivel:0.##}   crítico {d.ChanceDeSucessoCritico * 100:0}%", EstiloTexto);
                GUI.Label(new Rect(area.x + 16, y + 48, 460, 22),
                    $"Custo: {FormatadorDeNumeros.Formatar(RegrasDaForja.CustoEmOuro(d, nivel))} ouro + " +
                    $"{RegrasDaForja.CustoEmMaterial(d, nivel)} {material} (tem {ProgressoDoJogador.Material(d.MaterialId)})", EstiloTexto);

                if (GUI.Button(new Rect(area.x + area.width - 140, y + 10, 120, 40), "Forjar", EstiloBotao)) Forjar(d);
            }
        }
    }
}
