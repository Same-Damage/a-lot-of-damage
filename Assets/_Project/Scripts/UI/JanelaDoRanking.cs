using System;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Ranking online por maior estágio ou Poder: os 100 primeiros, em páginas, e a posição do jogador embaixo
    public class JanelaDoRanking : JanelaDoJogo
    {
        const int PorPagina = 10, Quantos = 100;
        const float Validade = 60f;     // não consulta o servidor de novo antes disso

        protected override string Titulo => "Ranking";
        protected override Key Tecla => Key.P;
        protected override int Ordem => 11;
        protected override IconeDoHud Icone => IconeDoHud.Ranking;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1000, 860);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        IRanking ranking;
        IContaOnline conta;

        readonly Text[] posicoes = new Text[PorPagina + 1], nomes = new Text[PorPagina + 1], pontos = new Text[PorPagina + 1];
        readonly Image[] fundos = new Image[PorPagina + 1];
        Text aviso, numeroDaPagina;
        Text[] abas;
        readonly PaginaDoRanking[] paginas = new PaginaDoRanking[2];
        readonly float[] buscadoEm = { float.NegativeInfinity, float.NegativeInfinity };
        TipoDeRanking tipo;
        int pagina;
        bool buscando, mudou;

        [Inject]
        public void Construir(IRanking ranking, IContaOnline conta)
        {
            this.ranking = ranking;
            this.conta = conta;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            abas = new[] { Aba(painel, "Maior estágio", 52, TipoDeRanking.Estagio), Aba(painel, "Poder", 322, TipoDeRanking.Poder) };
            for (int i = 0; i <= PorPagina; i++) MontarPosicao(painel, i, i < PorPagina ? 156 + i * 52 : 156 + PorPagina * 52 + 24);
            aviso = KitDeInterface.Texto("Aviso", painel, 24, KitDeInterface.TextoClaro, TextAnchor.MiddleCenter);
            KitDeInterface.Ancorar((RectTransform)aviso.transform, new Vector2(0.5f, 1), new Vector2(0, -380), new Vector2(860, 40));
            var antes = KitDeInterface.Botao("Anterior", painel, "<", () => Folhear(-1), 26);
            KitDeInterface.Ancorar((RectTransform)antes.transform, new Vector2(0.5f, 0), new Vector2(-130, 34), new Vector2(80, 52));
            var depois = KitDeInterface.Botao("Próxima", painel, ">", () => Folhear(1), 26);
            KitDeInterface.Ancorar((RectTransform)depois.transform, new Vector2(0.5f, 0), new Vector2(130, 34), new Vector2(80, 52));
            numeroDaPagina = KitDeInterface.Texto("Página", painel, 22, KitDeInterface.TextoClaro, TextAnchor.MiddleCenter);
            KitDeInterface.Ancorar((RectTransform)numeroDaPagina.transform, new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(160, 52));
            mudou = true;
        }

        Text Aba(RectTransform painel, string rotulo, float x, TipoDeRanking t)
        {
            var b = KitDeInterface.Botao($"Aba {rotulo}", painel, rotulo, () => { tipo = t; pagina = 0; mudou = true; });
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(0, 1), new Vector2(x, -80), new Vector2(254, 56));
            return b.GetComponentInChildren<Text>();
        }

        void MontarPosicao(RectTransform painel, int i, float y)
        {
            fundos[i] = KitDeInterface.Painel($"Posição {i}", painel, Color.clear, false);
            fundos[i].raycastTarget = false;
            var linha = (RectTransform)fundos[i].transform;
            KitDeInterface.Ancorar(linha, new Vector2(0.5f, 1), new Vector2(0, -y), new Vector2(900, 46));
            posicoes[i] = Texto(linha, 14, 6, 24, KitDeInterface.Dourado, 90, true);
            nomes[i] = Texto(linha, 120, 6, 24, KitDeInterface.TextoClaro, 520);
            pontos[i] = KitDeInterface.Texto("Pontos", linha, 24, KitDeInterface.TextoClaro, TextAnchor.MiddleRight, true);
            KitDeInterface.Ancorar((RectTransform)pontos[i].transform, new Vector2(1, 1), new Vector2(-14, -6), new Vector2(240, 34));
        }

        int Paginas => paginas[(int)tipo] == null ? 1 : Mathf.Max(1, Mathf.CeilToInt(paginas[(int)tipo].Primeiros.Count / (float)PorPagina));

        void Folhear(int passo)
        {
            pagina = Mathf.Clamp(pagina + passo, 0, Paginas - 1);
            mudou = true;
        }

        protected override void AtualizarConteudo()
        {
            if (conta.Conectada && !buscando && Time.unscaledTime - buscadoEm[(int)tipo] > Validade) Buscar(tipo);
            if (!mudou) return;
            mudou = false;

            for (int i = 0; i < abas.Length; i++) abas[i].color = i == (int)tipo ? KitDeInterface.Dourado : KitDeInterface.TextoClaro;
            var p = paginas[(int)tipo];
            for (int i = 0; i < PorPagina; i++)
            {
                int k = pagina * PorPagina + i;
                Mostrar(i, p != null && k < p.Primeiros.Count ? p.Primeiros[k] : null);
            }
            Mostrar(PorPagina, p?.Minha);
            numeroDaPagina.text = $"{pagina + 1} / {Paginas}";
        }

        void Mostrar(int i, LinhaDoRanking l)
        {
            fundos[i].gameObject.SetActive(l != null);
            if (l == null) return;
            bool eu = l.JogadorId == conta.JogadorId;
            fundos[i].color = eu ? new Color(1f, 0.8f, 0.3f, 0.18f) : new Color(0, 0, 0, i % 2 == 0 ? 0.28f : 0.14f);
            posicoes[i].text = $"{l.Posicao}º";
            nomes[i].text = string.IsNullOrEmpty(l.Nome) ? (eu ? "Você" : "Sem nome") : l.Nome;
            pontos[i].text = tipo == TipoDeRanking.Estagio ? $"Estágio {l.Pontos:0}" : FormatadorDeNumeros.Formatar(l.Pontos);
        }

        async void Buscar(TipoDeRanking t)
        {
            buscando = true;
            buscadoEm[(int)t] = Time.unscaledTime;   // também depois de uma falha: não insiste a cada quadro
            if (paginas[(int)t] == null) aviso.text = "Buscando...";
            string erro = null;
            try { paginas[(int)t] = await ranking.Buscar(t, Quantos); }
            catch (FalhaOnline e) { erro = e.Message; }
            catch (Exception e) { Debug.LogException(e); erro = "Algo deu errado."; }
            if (!this) return;
            buscando = false;
            mudou = true;
            var p = paginas[(int)t];
            aviso.text = p == null ? erro : p.Primeiros.Count == 0 ? "Ninguém pontuou ainda." : "";
        }

        protected override void AoAbrir()
        {
            mudou = true;
            if (!conta.Conectada) aviso.text = "Sem conexão: o ranking aparece quando você estiver online.";
        }
    }
}
