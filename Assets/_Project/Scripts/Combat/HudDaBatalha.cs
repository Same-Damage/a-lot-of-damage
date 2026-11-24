using System;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ALotOfDamage.Combat
{
    // Interface da arena, só com o essencial: inimigo e vida no alto, combo e ultimate na direita, ouro no canto,
    // faixa de destaque e números de dano. Só desenha; quem decide é o CombateClicker.
    public class HudDaBatalha
    {
        readonly Text nome, vida, ouro, combo, furia, pontoFraco;
        readonly Barra barraDeVida;
        readonly BotaoDeIcone botaoDaUltimate, botaoDeFugir;
        readonly Image cargaDaUltimate;
        public readonly Faixa Faixa;
        public readonly NumerosFlutuantes Numeros;

        int comboMostrado = -1, furiaMostrada = -1;
        float ultimateMostrada = -1;

        public HudDaBatalha(Action fugir, Action dispararUltimate)
        {
            var canvas = KitDeInterface.Canvas();

            var topo = KitDeInterface.Placa("Inimigo", canvas);
            KitDeInterface.Ancorar(topo, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(640, 78));
            nome = KitDeInterface.Texto("Nome", topo, 25, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)nome.transform, new Vector2(0.5f, 1), new Vector2(0, -6), new Vector2(600, 34));
            barraDeVida = KitDeInterface.NovaBarra("Vida", topo, KitDeInterface.Vermelho);
            KitDeInterface.Ancorar(barraDeVida.Rect, new Vector2(0.5f, 0), new Vector2(0, 11), new Vector2(596, 24));
            vida = KitDeInterface.Texto("Vida", barraDeVida.Rect, 17, Color.white, TextAnchor.MiddleCenter, true);
            KitDeInterface.Esticar((RectTransform)vida.transform);

            var canto = KitDeInterface.Placa("Ouro", canvas);
            KitDeInterface.Ancorar(canto, new Vector2(0, 1), new Vector2(24, -24), new Vector2(190, 44));
            var moeda = KitDeInterface.Icone("Moeda", canto, IconeDoHud.Ouro);
            KitDeInterface.Ancorar((RectTransform)moeda.transform, new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(22, 22));
            ouro = KitDeInterface.Texto("Ouro", canto, 22, KitDeInterface.TextoClaro, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)ouro.transform, new Vector2(0, 0.5f), new Vector2(48, 0), new Vector2(136, 32));

            botaoDeFugir = new BotaoDeIcone("Fugir", canvas, IconeDoHud.Fugir, "Fugir", 64, fugir);
            KitDeInterface.Ancorar(botaoDeFugir.Rect, new Vector2(1, 1), new Vector2(-24, -24), Vector2.one * 64);

            pontoFraco = KitDeInterface.Texto("Ponto Fraco", canvas, 30, KitDeInterface.Azul, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)pontoFraco.transform, new Vector2(0.5f, 1), new Vector2(0, -112), new Vector2(600, 44));
            pontoFraco.text = "PONTO FRACO!";

            // ultimate: o disco enche e acende quando está pronta (toque, clique ou Espaço)
            botaoDaUltimate = new BotaoDeIcone("Ultimate", canvas, IconeDoHud.Ultimate, "", 124, dispararUltimate);
            KitDeInterface.Ancorar(botaoDaUltimate.Rect, new Vector2(1, 0), new Vector2(-48, 64), Vector2.one * 124);
            cargaDaUltimate = KitDeInterface.Painel("Carga", botaoDaUltimate.Rect, CorDaCarga, false);
            cargaDaUltimate.sprite = TemaVisual.Circulo;
            cargaDaUltimate.raycastTarget = false;
            cargaDaUltimate.type = Image.Type.Filled;
            cargaDaUltimate.fillMethod = Image.FillMethod.Vertical;
            KitDeInterface.Ancorar((RectTransform)cargaDaUltimate.transform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * 112);
            cargaDaUltimate.transform.SetAsFirstSibling();

            combo = KitDeInterface.Texto("Combo", canvas, 44, KitDeInterface.Dourado, TextAnchor.MiddleRight, true);
            KitDeInterface.Ancorar((RectTransform)combo.transform, new Vector2(1, 0.5f), new Vector2(-48, 40), new Vector2(400, 60));
            furia = KitDeInterface.Texto("Furia", canvas, 24, KitDeInterface.Vermelho, TextAnchor.MiddleRight, true);
            KitDeInterface.Ancorar((RectTransform)furia.transform, new Vector2(1, 0.5f), new Vector2(-48, -6), new Vector2(400, 36));

            Faixa = Faixa.DoCanvas(canvas, 0.72f);
            Numeros = NumerosFlutuantes.Criar(canvas);
        }

        static readonly Color CorDaCarga = new Color(0.55f, 0.28f, 0.85f, 0.55f);
        static readonly Color CorDaCargaPronta = new Color(0.62f, 0.34f, 0.95f, 0.85f);

        public void Inimigo(string nomeDoInimigo) => nome.text = nomeDoInimigo;

        public void Ouro(string texto) => ouro.text = texto;

        public void Vida(double atual, double maxima, string texto)
        {
            barraDeVida.Definir((float)(Math.Max(0, atual) / maxima));
            vida.text = texto;
        }

        public void Combo(int valor, int acumulosDeFuria, string nomeDaPassiva)
        {
            if (valor == comboMostrado && acumulosDeFuria == furiaMostrada) return;
            comboMostrado = valor;
            furiaMostrada = acumulosDeFuria;
            combo.text = valor > 1 ? $"×{valor}" : "";
            furia.text = valor > 1 && acumulosDeFuria > 0 ? $"{nomeDaPassiva} ×{acumulosDeFuria}" : "";
        }

        public void Ultimate(float carga)
        {
            if (Mathf.Approximately(carga, ultimateMostrada)) return;
            ultimateMostrada = carga;
            bool pronta = carga >= 100;
            cargaDaUltimate.fillAmount = carga / 100f;
            cargaDaUltimate.color = pronta ? CorDaCargaPronta : CorDaCarga;
            botaoDaUltimate.Destacar(pronta);
            botaoDaUltimate.Rotulo.text = pronta ? "ULTIMATE!" : "";
            botaoDaUltimate.Rotulo.color = KitDeInterface.Dourado;
        }

        public void PontoFraco(bool ativo)
        {
            if (pontoFraco.gameObject.activeSelf != ativo) pontoFraco.gameObject.SetActive(ativo);
        }

        public void PodeFugir(bool pode)
        {
            var botao = botaoDeFugir.Rect.gameObject;
            if (botao.activeSelf != pode) botao.SetActive(pode);
        }
    }
}
