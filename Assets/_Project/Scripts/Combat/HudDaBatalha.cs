using System;
using ALotOfDamage.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ALotOfDamage.Combat
{
    // Interface da arena no kit de interface: inimigo e vida no alto, combo e ultimate embaixo,
    // faixa de destaque e números de dano. Só desenha; quem decide é o CombateClicker.
    public class HudDaBatalha
    {
        readonly Text nome, vida, ouro, combo, follower, ultimate, pontoFraco;
        readonly Barra barraDeVida, barraDaUltimate;
        readonly GameObject botaoDeFugir;
        public readonly Faixa Faixa;
        public readonly NumerosFlutuantes Numeros;

        int comboMostrado = -1, furiaMostrada = -1;
        float ultimateMostrada = -1;

        public HudDaBatalha(Action fugir)
        {
            var canvas = KitDeInterface.Canvas();

            var topo = (RectTransform)KitDeInterface.Painel("Inimigo", canvas).transform;
            KitDeInterface.Ancorar(topo, new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(900, 110));
            nome = KitDeInterface.Texto("Nome", topo, 32, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)nome.transform, new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(860, 44));
            barraDeVida = KitDeInterface.NovaBarra("Vida", topo, KitDeInterface.Vermelho);
            KitDeInterface.Ancorar(barraDeVida.Rect, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(840, 34));
            vida = KitDeInterface.Texto("Vida", barraDeVida.Rect, 22, Color.white, TextAnchor.MiddleCenter, true);
            KitDeInterface.Esticar((RectTransform)vida.transform);

            var esquerda = (RectTransform)KitDeInterface.Painel("Ouro", canvas).transform;
            KitDeInterface.Ancorar(esquerda, new Vector2(0, 1), new Vector2(20, -20), new Vector2(360, 60));
            ouro = KitDeInterface.Texto("Ouro", esquerda, 26, KitDeInterface.Dourado, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)ouro.transform, new Vector2(0, 0.5f), new Vector2(18, 0), new Vector2(330, 40));

            var fugirBotao = KitDeInterface.Botao("Fugir", canvas, "Fugir [Esc]", fugir);
            KitDeInterface.Ancorar((RectTransform)fugirBotao.transform, new Vector2(1, 1), new Vector2(-20, -20), new Vector2(220, 60));
            botaoDeFugir = fugirBotao.gameObject;

            pontoFraco = KitDeInterface.Texto("Ponto Fraco", canvas, 40, KitDeInterface.Azul, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)pontoFraco.transform, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(700, 60));
            pontoFraco.text = "PONTO FRACO! CLIQUE!";

            var baixo = (RectTransform)KitDeInterface.Painel("Combate", canvas).transform;
            KitDeInterface.Ancorar(baixo, new Vector2(0, 0), new Vector2(20, 20), new Vector2(620, 170));
            follower = Linha(baixo, 12, 22, KitDeInterface.Azul);
            combo = Linha(baixo, 46, 30, KitDeInterface.Dourado);
            ultimate = Linha(baixo, 90, 24, KitDeInterface.TextoClaro);
            barraDaUltimate = KitDeInterface.NovaBarra("Ultimate", baixo, new Color(0.75f, 0.4f, 1f));
            KitDeInterface.Ancorar(barraDaUltimate.Rect, new Vector2(0, 1), new Vector2(20, -136), new Vector2(580, 18));

            Faixa = Faixa.DoCanvas(canvas, 0.72f);
            Numeros = NumerosFlutuantes.Criar(canvas);
        }

        static Text Linha(RectTransform painel, float y, int tamanho, Color cor)
        {
            var t = KitDeInterface.Texto("Linha", painel, tamanho, cor, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0, 1), new Vector2(20, -y), new Vector2(580, tamanho + 10));
            return t;
        }

        public void Inimigo(string nomeDoInimigo, string nomeDoFollower)
        {
            nome.text = nomeDoInimigo;
            follower.text = nomeDoFollower != null ? $"{nomeDoFollower} luta com você" : "";
        }

        public void Ouro(string texto) => ouro.text = texto;

        public void Vida(double atual, double maxima, string texto)
        {
            barraDeVida.Definir((float)(Math.Max(0, atual) / maxima));
            vida.text = texto;
        }

        public void Combo(int valor, int furia, string nomeDaPassiva)
        {
            if (valor == comboMostrado && furia == furiaMostrada) return;
            comboMostrado = valor;
            furiaMostrada = furia;
            combo.text = valor > 1 ? $"Combo ×{valor}" + (furia > 0 ? $"   {nomeDaPassiva} ×{furia}" : "") : "";
        }

        public void Ultimate(float carga)
        {
            if (Mathf.Approximately(carga, ultimateMostrada)) return;
            ultimateMostrada = carga;
            barraDaUltimate.Definir(carga / 100f);
            ultimate.text = carga >= 100 ? "ULTIMATE PRONTA! [Espaço]" : $"Ultimate {carga:0}%";
            ultimate.color = carga >= 100 ? KitDeInterface.Dourado : KitDeInterface.TextoClaro;
        }

        public void PontoFraco(bool ativo)
        {
            if (pontoFraco.gameObject.activeSelf != ativo) pontoFraco.gameObject.SetActive(ativo);
        }

        public void PodeFugir(bool pode)
        {
            if (botaoDeFugir.activeSelf != pode) botaoDeFugir.SetActive(pode);
        }
    }
}
