using UnityEngine;
using UnityEngine.InputSystem;
using ALotOfDamage.Core;
using VContainer;

namespace ALotOfDamage.UI
{
    // Base das telas provisórias em OnGUI (upgrades, forja, bestiário) até existir o kit de interface.
    // Só uma fica aberta por vez.
    public abstract class JanelaProvisoria : MonoBehaviour
    {
        static JanelaProvisoria aberta;

        string faixa;
        float faixaAte;

        protected abstract string Titulo { get; }
        protected abstract Key Tecla { get; }
        protected abstract int Ordem { get; }   // posição do botão na coluna da direita
        protected abstract float AlturaDoConteudo { get; }
        protected abstract void DesenharConteudo(Rect area);

        protected bool Aberta => aberta == this;

        protected IRepositorioDeProgresso repositorio;
        protected ICatalogo catalogo;
        protected Progresso Progresso => repositorio.Carregar();

        [Inject]
        public void ConstruirJanela(IRepositorioDeProgresso repositorio, ICatalogo catalogo)
        {
            this.repositorio = repositorio;
            this.catalogo = catalogo;
        }

        protected static GUIStyle EstiloTitulo => TemaVisual.EstiloTitulo;
        protected static GUIStyle EstiloTexto => TemaVisual.EstiloTexto;

        void Alternar() => aberta = Aberta ? null : this;

        void OnDisable()
        {
            if (Aberta) aberta = null;
        }

        protected virtual void Update()
        {
            var k = Keyboard.current;
            if (k != null && k[Tecla].wasPressedThisFrame) Alternar();
        }

        protected void MostrarFaixa(string texto, bool destaque)
        {
            faixa = texto;
            faixaAte = Time.time + (destaque ? 2.5f : 1.2f);
        }

        protected static void Barra(Rect r, float progresso, Color cor) => TemaVisual.Barra(r, progresso, cor);

        // Fundo escuro com linha dourada separando os itens da lista
        protected static void Separador(Rect r) => TemaVisual.Separador(r);

        void OnGUI()
        {
            if (TemaVisual.Botao(new Rect(Screen.width - 180, 20 + Ordem * 52, 160, 44),
                    Aberta ? $"Fechar [{Tecla}]" : $"{Titulo} [{Tecla}]"))
                Alternar();

            if (Time.time < faixaAte)
                TemaVisual.FaixaDeHabilidade(Screen.width * 0.5f, Screen.height * 0.15f, faixa);

            if (!Aberta) return;

            float largura = Mathf.Min(760, Screen.width - 240);
            var area = new Rect((Screen.width - 200 - largura) / 2, 90, largura, 90 + AlturaDoConteudo);
            var dentro = TemaVisual.Painel(area, Titulo);
            if (TemaVisual.BotaoFechar(area)) { Alternar(); return; }

            TemaVisual.TextoComContorno(new Rect(dentro.x + 4, dentro.y, dentro.width, 28),
                $"Ouro  {FormatadorDeNumeros.Formatar(Progresso.Ouro)}", EstiloTitulo, TemaVisual.OuroClaro, TextAnchor.MiddleRight, 18);
            DesenharConteudo(new Rect(dentro.x - 14, dentro.y + 36, dentro.width + 28, AlturaDoConteudo));
        }
    }
}
