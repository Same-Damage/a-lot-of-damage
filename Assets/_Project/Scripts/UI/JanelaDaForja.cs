using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Uma linha por slot: refina o item equipado nele ou fabrica um item novo do tipo
    public class JanelaDaForja : JanelaDoJogo
    {
        protected override string Titulo => "Forja";
        protected override Key Tecla => Key.J;
        protected override int Ordem => 2;
        protected override IconeDoHud Icone => IconeDoHud.Forja;
        protected override SistemaDoJogo? Sistema => SistemaDoJogo.Forja;
        protected override TipoDePendencia? Pendencia => TipoDePendencia.Forja;
        protected override int Linhas => catalogo.Itens.Count;
        protected override float AlturaDaLinha => 92;

        RefinarItem refinarItem;
        FabricarItem fabricarItem;
        Text[] nomes, efeitos, custos;
        Button[] refinar, fabricar;

        [Inject]
        public void Construir(RefinarItem refinarItem, FabricarItem fabricarItem)
        {
            this.refinarItem = refinarItem;
            this.fabricarItem = fabricarItem;
        }

        protected override void Start()
        {
            int n = catalogo.Itens.Count;
            nomes = new Text[n]; efeitos = new Text[n]; custos = new Text[n]; refinar = new Button[n]; fabricar = new Button[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            var d = catalogo.Itens[i];
            nomes[i] = Texto(linha, 24, 6, 26, KitDeInterface.Dourado, negrito: true);
            efeitos[i] = Texto(linha, 24, 38, 20, KitDeInterface.TextoClaro, 600);
            custos[i] = Texto(linha, 24, 64, 20, KitDeInterface.TextoClaro, 600);
            refinar[i] = Botao(linha, "Refinar", 200, 150, () => Refinar(d));
            fabricar[i] = Botao(linha, "Fabricar", 30, 160, () => Fabricar(d));
        }

        void Refinar(DefinicaoDeItem d)
        {
            var item = Progresso.Equipado(d.Slot);
            if (item == null) return;
            var r = refinarItem.Executar(item.Id);
            if (!r.Forjou) return;
            faixa.Mostrar(r.SucessoCritico ? $"SUCESSO CRÍTICO! {d.Nome} +{r.NovoNivel}" : $"{d.Nome} +{r.NovoNivel}",
                r.SucessoCritico ? 2.5f : 1.2f);
        }

        void Fabricar(DefinicaoDeItem d)
        {
            var r = fabricarItem.Executar(d.Id);
            if (r.Fabricou) faixa.Mostrar($"{d.Nome} {RegrasDosItens.Nome(r.Item.Raridade)} fabricado!", 1.5f);
        }

        protected override void AtualizarLinha(int i)
        {
            var d = catalogo.Itens[i];
            var p = Progresso;
            var regras = catalogo.ParametrosDosItens;
            var item = p.Equipado(d.Slot);
            int tem = p.Material(d.MaterialId);
            string material = catalogo.NomeDoMaterial(d.MaterialId);

            double custoFabricar = RegrasDosItens.CustoDeFabricar(regras, p.Estagio);
            bool podeFabricar = custoFabricar <= p.Ouro && regras.MaterialParaFabricar <= tem && p.Inventario.Count < regras.LimiteDoInventario;
            fabricar[i].interactable = podeFabricar;

            if (item == null)
            {
                nomes[i].text = $"{d.Nome}: nada equipado";
                efeitos[i].text = $"Fabricar: {FormatadorDeNumeros.Formatar(custoFabricar)} ouro + {regras.MaterialParaFabricar} {material}";
                custos[i].text = $"(tem {tem} {material})";
                custos[i].color = KitDeInterface.TextoClaro;
                refinar[i].interactable = false;
                return;
            }

            double ouro = RegrasDaForja.CustoEmOuro(d, item.Nivel);
            int custoMaterial = RegrasDaForja.CustoEmMaterial(d, item.Nivel);
            bool podeRefinar = ouro <= p.Ouro && custoMaterial <= tem;
            nomes[i].text = $"{d.Nome} {RegrasDosItens.Nome(item.Raridade)} +{item.Nivel}";
            efeitos[i].text = $"{d.Descricao}: ×{FormatadorDeNumeros.Formatar(RegrasDosItens.Multiplicador(regras, d, item))}" +
                              $"   refino ×{1 + d.BonusPorNivel:0.##}   crítico {d.ChanceDeSucessoCritico * 100:0}%";
            custos[i].text = $"Refinar: {FormatadorDeNumeros.Formatar(ouro)} ouro + {custoMaterial} {material} (tem {tem})";
            custos[i].color = podeRefinar ? KitDeInterface.Verde : KitDeInterface.TextoClaro;
            refinar[i].interactable = podeRefinar;
        }
    }
}
