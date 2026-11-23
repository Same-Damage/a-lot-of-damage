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

        protected static GUIStyle EstiloTitulo, EstiloTexto, EstiloBotao;

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

        protected static void Barra(Rect r, float progresso, Color cor)
        {
            GUI.Box(r, "");
            GUI.color = cor;
            GUI.Box(new Rect(r.x, r.y, r.width * Mathf.Clamp01(progresso), r.height), "");
            GUI.color = Color.white;
        }

        void OnGUI()
        {
            EstiloTitulo ??= new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            EstiloTexto ??= new GUIStyle(GUI.skin.label) { fontSize = 15 };
            EstiloBotao ??= new GUIStyle(GUI.skin.button) { fontSize = 15 };

            if (GUI.Button(new Rect(Screen.width - 180, 20 + Ordem * 52, 160, 44),
                    Aberta ? $"Fechar [{Tecla}]" : $"{Titulo} [{Tecla}]", EstiloBotao))
                Alternar();

            if (Time.time < faixaAte)
                GUI.Label(new Rect(Screen.width * 0.5f - 300, Screen.height * 0.15f, 600, 60), faixa,
                    new GUIStyle(EstiloTitulo) { fontSize = 34, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } });

            if (!Aberta) return;

            float largura = Mathf.Min(760, Screen.width - 240);
            var area = new Rect((Screen.width - 200 - largura) / 2, 80, largura, 60 + AlturaDoConteudo);
            GUI.Box(area, "");
            GUI.Label(new Rect(area.x + 16, area.y + 10, largura, 30),
                $"{Titulo}     Ouro: {FormatadorDeNumeros.Formatar(Progresso.Ouro)}", EstiloTitulo);
            DesenharConteudo(new Rect(area.x, area.y + 50, area.width, AlturaDoConteudo));
        }
    }
}
