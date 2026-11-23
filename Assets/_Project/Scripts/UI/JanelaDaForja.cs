using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    public class JanelaDaForja : JanelaDoJogo
    {
        protected override string Titulo => "Forja";
        protected override Key Tecla => Key.J;
        protected override int Ordem => 1;
        protected override int Linhas => catalogo.Itens.Count;
        protected override float AlturaDaLinha => 120;

        ForjarItem forjarItem;
        Text[] nomes, efeitos, custos;
        Button[] botoes;

        [Inject]
        public void Construir(ForjarItem forjarItem) => this.forjarItem = forjarItem;

        protected override void Start()
        {
            int n = catalogo.Itens.Count;
            nomes = new Text[n]; efeitos = new Text[n]; custos = new Text[n]; botoes = new Button[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            var d = catalogo.Itens[i];
            nomes[i] = Texto(linha, 24, 10, 28, KitDeInterface.Dourado, negrito: true);
            efeitos[i] = Texto(linha, 24, 46, 22, KitDeInterface.TextoClaro, 700);
            custos[i] = Texto(linha, 24, 78, 22, KitDeInterface.TextoClaro, 700);
            botoes[i] = Botao(linha, "Forjar", 30, 160, () => Forjar(d));
        }

        void Forjar(DefinicaoDeItem d)
        {
            var r = forjarItem.Executar(d.Id);
            if (!r.Forjou) return;
            faixa.Mostrar(r.SucessoCritico ? $"SUCESSO CRÍTICO! {d.Nome} +{r.NovoNivel}" : $"{d.Nome} +{r.NovoNivel}",
                r.SucessoCritico ? 2.5f : 1.2f);
        }

        protected override void AtualizarLinha(int i)
        {
            var d = catalogo.Itens[i];
            int nivel = Progresso.NivelDoItem(d.Id);
            double ouro = RegrasDaForja.CustoEmOuro(d, nivel);
            int material = RegrasDaForja.CustoEmMaterial(d, nivel);
            int tem = Progresso.Material(d.MaterialId);
            bool pode = ouro <= Progresso.Ouro && material <= tem;

            nomes[i].text = $"{d.Nome} +{nivel}";
            efeitos[i].text = $"{d.Descricao}: ×{FormatadorDeNumeros.Formatar(RegrasDaForja.Multiplicador(d, nivel))}" +
                              $"   próximo ×{1 + d.BonusPorNivel:0.##}   crítico {d.ChanceDeSucessoCritico * 100:0}%";
            custos[i].text = $"Custo: {FormatadorDeNumeros.Formatar(ouro)} ouro + {material} {catalogo.NomeDoMaterial(d.MaterialId)} (tem {tem})";
            custos[i].color = pode ? KitDeInterface.Verde : KitDeInterface.TextoClaro;
            botoes[i].interactable = pode;
        }
    }
}
