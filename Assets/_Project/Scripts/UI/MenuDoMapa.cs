using System;
using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.UI
{
    // Ícones das janelas na borda direita: os principais sempre à vista e os outros atrás do botão Menu.
    // As janelas se registram no Start; o HUD informa as pendências, que viram selos.
    public class MenuDoMapa : MonoBehaviour
    {
        public const int Principais = 3;
        const int PorColuna = 9;
        const float TamanhoDoBotao = 72, Passo = 104, Margem = 24;

        class Entrada { public BotaoDeIcone Botao; public int Ordem; public TipoDePendencia? Tipo; }

        readonly List<Entrada> entradas = new List<Entrada>();
        BotaoDeIcone menu;
        RectTransform extras;

        public bool Aberto => extras.gameObject.activeSelf;

        public static MenuDoMapa DoCanvas(RectTransform canvas)
        {
            var existente = canvas.Find("Menu do Mapa");
            if (existente) return existente.GetComponent<MenuDoMapa>();
            var raiz = KitDeInterface.Novo("Menu do Mapa", canvas);
            KitDeInterface.Esticar(raiz);
            var m = raiz.gameObject.AddComponent<MenuDoMapa>();
            m.extras = KitDeInterface.Novo("Extras", raiz);
            KitDeInterface.Esticar(m.extras);
            m.menu = new BotaoDeIcone("Menu", raiz, IconeDoHud.Menu, "Menu", TamanhoDoBotao, m.Alternar);
            KitDeInterface.Ancorar(m.menu.Rect, new Vector2(1, 1), Posicao(0, Principais), Vector2.one * TamanhoDoBotao);
            m.extras.gameObject.SetActive(false);
            return m;
        }

        static Vector2 Posicao(int coluna, int linha) => new Vector2(-Margem - coluna * (TamanhoDoBotao + 40), -Margem - linha * Passo);

        public BotaoDeIcone Adicionar(int ordem, IconeDoHud icone, string titulo, TipoDePendencia? tipo, Action alternar)
        {
            bool principal = ordem < Principais;
            var b = new BotaoDeIcone($"Botao {titulo}", principal ? transform : extras, icone, titulo, TamanhoDoBotao, () => { if (!principal) Alternar(); alternar(); });
            KitDeInterface.Ancorar(b.Rect, new Vector2(1, 1), principal ? Posicao(0, ordem) : Posicao(1 + (ordem - Principais) / PorColuna, (ordem - Principais) % PorColuna), Vector2.one * TamanhoDoBotao);
            entradas.Add(new Entrada { Botao = b, Ordem = ordem, Tipo = tipo });
            return b;
        }

        public void Alternar()
        {
            extras.gameObject.SetActive(!Aberto);
            menu.Destacar(Aberto);
        }

        // O botão Menu soma o que está escondido nele
        public void Pendencias(List<Pendencia> lista)
        {
            int escondidas = 0;
            foreach (var e in entradas)
            {
                int n = e.Tipo.HasValue ? RegrasDasPendencias.Quantas(lista, e.Tipo.Value) : 0;
                e.Botao.Selo(n);
                if (e.Ordem >= Principais) escondidas += n;
            }
            menu.Selo(Aberto ? 0 : escondidas);
        }
    }
}
