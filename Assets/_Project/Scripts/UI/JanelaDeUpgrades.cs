using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    public class JanelaDeUpgrades : JanelaDoJogo
    {
        protected override string Titulo => "Upgrades";
        protected override Key Tecla => Key.U;
        protected override int Ordem => 0;
        protected override int Linhas => catalogo.Upgrades.Count;
        protected override float AlturaDaLinha => 130;

        ComprarUpgrade comprarUpgrade;
        Text[] nomes, valores, custos, marcos;
        Barra[] barras;
        GameObject[] botoes;

        [Inject]
        public void Construir(ComprarUpgrade comprarUpgrade) => this.comprarUpgrade = comprarUpgrade;

        protected override void Start()
        {
            int n = catalogo.Upgrades.Count;
            nomes = new Text[n]; valores = new Text[n]; custos = new Text[n]; marcos = new Text[n];
            barras = new Barra[n]; botoes = new GameObject[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            var d = catalogo.Upgrades[i];
            nomes[i] = Texto(linha, 24, 10, 28, KitDeInterface.Dourado, negrito: true);
            valores[i] = Texto(linha, 24, 46, 22, KitDeInterface.TextoClaro);
            custos[i] = Texto(linha, 24, 76, 22, KitDeInterface.TextoClaro);
            barras[i] = Barra(linha, 24, 110, 360, KitDeInterface.Dourado);
            marcos[i] = Texto(linha, 400, 104, 18, KitDeInterface.TextoClaro, 300);

            var grupo = KitDeInterface.Novo("Botoes", linha);
            KitDeInterface.Esticar(grupo);
            botoes[i] = grupo.gameObject;
            Botao(grupo, "x1", 330, 90, () => Comprar(d, 1));
            Botao(grupo, "x10", 230, 90, () => Comprar(d, 10));
            Botao(grupo, "Máx", 120, 100, () => Comprar(d, RegrasDeUpgrade.Maximo));
            if (d.Tipo == TipoDeUpgrade.Linear) { barras[i].Fundo.gameObject.SetActive(false); marcos[i].gameObject.SetActive(false); }
        }

        void Comprar(DefinicaoDeUpgrade d, int quantidade)
        {
            double antes = Progresso.Valor(d);
            var r = comprarUpgrade.Executar(d.Id, quantidade);
            if (!r.Comprou) return;
            double ganho = Progresso.Valor(d) / antes;
            faixa.Mostrar(r.AtingiuMarco
                ? $"MARCO! {d.Nome} ×{FormatadorDeNumeros.Formatar(ganho)}"
                : $"{d.Nome} {RegrasDePoder.TextoDoGanho(antes, Progresso.Valor(d))}", r.AtingiuMarco ? 2.5f : 1.2f);
        }

        protected override void AtualizarLinha(int i)
        {
            var d = catalogo.Upgrades[i];
            int nivel = Progresso.Nivel(d.Id);
            bool linear = d.Tipo == TipoDeUpgrade.Linear;
            bool noMaximo = RegrasDeUpgrade.NoMaximo(d, nivel);
            double valor = RegrasDeUpgrade.Valor(d, nivel);

            nomes[i].text = $"{d.Nome}  ·  nível {nivel}";
            valores[i].text = $"{d.Descricao}: {(linear ? $"{valor * 100:0}%" : FormatadorDeNumeros.Formatar(valor))}";
            botoes[i].SetActive(!noMaximo);
            if (noMaximo) { custos[i].text = "Nível máximo"; return; }

            double custo = RegrasDeUpgrade.Custo(d, nivel);
            bool podeComprar = custo <= Progresso.Ouro;
            custos[i].text = $"Próximo: +{RegrasDeUpgrade.GanhoRelativo(d, nivel) * 100:0.#}%   Custo: {FormatadorDeNumeros.Formatar(custo)}";
            custos[i].color = podeComprar ? KitDeInterface.Verde : KitDeInterface.TextoClaro;
            if (linear) return;

            int marco = RegrasDeUpgrade.ProximoMarco(nivel);
            int anterior = RegrasDeUpgrade.MarcoAnterior(nivel);
            barras[i].Definir((float)(nivel - anterior) / (marco - anterior));
            marcos[i].text = $"marco no nível {marco}";
        }
    }
}
