using System;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ALotOfDamage.UI
{
    // Botão redondo do HUD: disco escuro com aro dourado, ícone, rótulo pequeno embaixo e selo vermelho de pendência
    public class BotaoDeIcone
    {
        public readonly RectTransform Rect;
        public readonly Text Rotulo;
        readonly Image disco;
        readonly GameObject selo;
        readonly Text textoDoSelo;
        int seloMostrado = -1;

        public BotaoDeIcone(string nome, Transform pai, IconeDoHud icone, string rotulo, float tamanho, Action aoClicar)
        {
            disco = KitDeInterface.Painel(nome, pai, Color.white, false);
            disco.sprite = TemaVisual.Disco;
            Rect = (RectTransform)disco.transform;
            Rect.sizeDelta = new Vector2(tamanho, tamanho);

            var botao = disco.gameObject.AddComponent<Button>();
            botao.transition = Selectable.Transition.SpriteSwap;
            botao.spriteState = new SpriteState { highlightedSprite = TemaVisual.DiscoClaro, selectedSprite = TemaVisual.Disco, pressedSprite = TemaVisual.DiscoClaro };
            // teclado e controle não navegam pelo HUD: espaço e Enter são do jogo
            botao.navigation = new Navigation { mode = Navigation.Mode.None };
            botao.onClick.AddListener(() => aoClicar());

            var img = KitDeInterface.Icone("Icone", Rect, icone);
            KitDeInterface.Ancorar((RectTransform)img.transform, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one * tamanho * 0.56f);

            Rotulo = KitDeInterface.Texto("Rotulo", Rect, 17, KitDeInterface.TextoClaro, TextAnchor.MiddleCenter, true);
            KitDeInterface.Ancorar((RectTransform)Rotulo.transform, new Vector2(0.5f, 0), new Vector2(0, -20), new Vector2(tamanho + 20, 20));
            Rotulo.text = rotulo;

            var fundoDoSelo = KitDeInterface.Painel("Selo", Rect, KitDeInterface.Vermelho, false);
            fundoDoSelo.sprite = TemaVisual.Circulo;
            fundoDoSelo.raycastTarget = false;
            KitDeInterface.Ancorar((RectTransform)fundoDoSelo.transform, new Vector2(1, 1), new Vector2(4, 4), new Vector2(26, 26));
            textoDoSelo = KitDeInterface.Texto("Quantas", fundoDoSelo.transform, 16, Color.white, TextAnchor.MiddleCenter, true);
            KitDeInterface.Esticar((RectTransform)textoDoSelo.transform);
            selo = fundoDoSelo.gameObject;
            Selo(0);
        }

        public bool ComSelo => selo.activeSelf;

        public void Selo(int quantas)
        {
            if (quantas == seloMostrado) return;
            seloMostrado = quantas;
            textoDoSelo.text = RegrasDasPendencias.Selo(quantas);
            selo.SetActive(quantas > 0);
        }

        public void Destacar(bool sim) => disco.sprite = sim ? TemaVisual.DiscoClaro : TemaVisual.Disco;
    }
}
