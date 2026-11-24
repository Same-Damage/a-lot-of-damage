using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // HUD do mapa, só com o essencial: Poder Total (contador animado + ganho relativo) e ouro no canto, estágio e região
    // no alto, e um resumo das pendências ("coisas prontas") que abre a lista ao tocar. Some com uma janela aberta.
    public class HudDoMapa : MonoBehaviour
    {
        const int MaximoDePendencias = 6;

        public float rapidezDoContador = 4f;
        public float intervaloDeConsulta = 0.2f;
        public float duracaoDoGanho = 1.6f;

        CalcularPoder calcularPoder;
        ConsultarCampanha consultarCampanha;
        ListarPendencias listarPendencias;
        IRepositorioDeProgresso repositorio;

        Text poder, ganho, ouro, estagio, progressoDoEstagio, resumo;
        Barra barraDoEstagio;
        readonly Text[] pendencias = new Text[MaximoDePendencias];
        RectTransform chip, painelDePendencias;
        CanvasGroup grupo;
        MenuDoMapa menu;
        bool listaAberta;
        readonly List<Pendencia> listaDePendencias = new List<Pendencia>(16);

        double alvo, mostrado = -1;
        float proximaConsulta, ganhoDesde = -10;
        string ultimoPoder, regiaoMostrada;
        int estagioMostrado = -1;

        [Inject]
        public void Construir(CalcularPoder calcularPoder, ConsultarCampanha consultarCampanha,
            ListarPendencias listarPendencias, IRepositorioDeProgresso repositorio)
        {
            this.calcularPoder = calcularPoder;
            this.consultarCampanha = consultarCampanha;
            this.listarPendencias = listarPendencias;
            this.repositorio = repositorio;
        }

        void Start()
        {
            var canvas = KitDeInterface.Canvas();

            menu = MenuDoMapa.DoCanvas(canvas);
            var raiz = KitDeInterface.Novo("HUD", canvas);
            KitDeInterface.Esticar(raiz);
            raiz.SetAsFirstSibling();
            grupo = raiz.gameObject.AddComponent<CanvasGroup>();

            // canto: Poder e ouro, ao lado do botão do herói
            var canto = KitDeInterface.Placa("Poder", raiz);
            KitDeInterface.Ancorar(canto, new Vector2(0, 1), new Vector2(116, -24), new Vector2(250, 84));
            Linha(canto, 7, 15, KitDeInterface.DouradoClaro, "PODER");
            poder = Linha(canto, 17, 32, KitDeInterface.Dourado);
            ganho = KitDeInterface.Texto("Ganho", raiz, 26, KitDeInterface.Verde, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)ganho.transform, new Vector2(0, 1), new Vector2(380, -30), new Vector2(220, 36));
            var moeda = KitDeInterface.Icone("Moeda", canto, IconeDoHud.Ouro);
            KitDeInterface.Ancorar((RectTransform)moeda.transform, new Vector2(0, 1), new Vector2(18, -60), new Vector2(18, 18));
            ouro = Linha(canto, 56, 19, KitDeInterface.TextoClaro);
            ((RectTransform)ouro.transform).anchoredPosition += new Vector2(26, 0);

            // alto: região, estágio e o quanto falta
            var topo = KitDeInterface.Placa("Estagio", raiz);
            KitDeInterface.Ancorar(topo, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(400, 62));
            estagio = KitDeInterface.Texto("Estagio", topo, 21, KitDeInterface.TextoClaro, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)estagio.transform, new Vector2(0, 1), new Vector2(22, -8), new Vector2(290, 28));
            progressoDoEstagio = KitDeInterface.Texto("Vitorias", topo, 19, KitDeInterface.TextoClaro, TextAnchor.MiddleRight, true);
            KitDeInterface.Ancorar((RectTransform)progressoDoEstagio.transform, new Vector2(1, 1), new Vector2(-22, -8), new Vector2(110, 28));
            barraDoEstagio = KitDeInterface.NovaBarra("Estagio", topo, KitDeInterface.Dourado);
            KitDeInterface.Ancorar(barraDoEstagio.Rect, new Vector2(0.5f, 0), new Vector2(0, 9), new Vector2(356, 14));

            // pendências: uma linha que abre a lista
            chip = KitDeInterface.Placa("Pendencias", raiz);
            KitDeInterface.Ancorar(chip, new Vector2(0, 1), new Vector2(116, -116), new Vector2(330, 40));
            chip.GetComponent<Image>().raycastTarget = true;
            var botao = chip.gameObject.AddComponent<Button>();
            botao.navigation = new Navigation { mode = Navigation.Mode.None };
            botao.onClick.AddListener(() => { listaAberta = !listaAberta; MostrarPendencias(); });
            var ponto = KitDeInterface.Painel("Ponto", chip, KitDeInterface.Vermelho, false);
            ponto.sprite = TemaVisual.Circulo;
            ponto.raycastTarget = false;
            KitDeInterface.Ancorar((RectTransform)ponto.transform, new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(12, 12));
            resumo = KitDeInterface.Texto("Resumo", chip, 18, KitDeInterface.TextoClaro);
            KitDeInterface.Ancorar((RectTransform)resumo.transform, new Vector2(0, 0.5f), new Vector2(38, 0), new Vector2(280, 30));

            painelDePendencias = KitDeInterface.Placa("Lista", raiz);
            for (int i = 0; i < MaximoDePendencias; i++)
            {
                pendencias[i] = KitDeInterface.Texto("Pendencia", painelDePendencias, 18, KitDeInterface.TextoClaro);
                KitDeInterface.Ancorar((RectTransform)pendencias[i].transform, new Vector2(0, 1), new Vector2(18, -10 - i * 32), new Vector2(300, 30));
            }

            alvo = calcularPoder.Executar();
            Consultar();
        }

        static Text Linha(RectTransform painel, float y, int tamanho, Color cor, string texto = "")
        {
            var t = KitDeInterface.Texto("Linha", painel, tamanho, cor, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0, 1), new Vector2(18, -y), new Vector2(214, tamanho + 8));
            t.text = texto;
            return t;
        }

        void Consultar()
        {
            double novo = calcularPoder.Executar();
            string texto = RegrasDePoder.TextoDoGanho(alvo, novo);
            if (texto != null) { ganho.text = texto; ganhoDesde = Time.unscaledTime; }
            alvo = novo;
            if (mostrado < 0) mostrado = alvo;

            var p = repositorio.Carregar();
            string textoDoOuro = FormatadorDeNumeros.Formatar(p.Ouro);
            if (ouro.text != textoDoOuro) ouro.text = textoDoOuro;

            var c = consultarCampanha.Executar();
            if (c.Estagio != estagioMostrado || c.Regiao?.Nome != regiaoMostrada)
            {
                estagioMostrado = c.Estagio;
                regiaoMostrada = c.Regiao?.Nome;
                estagio.text = regiaoMostrada == null ? $"Estágio {c.Estagio}" : $"{regiaoMostrada}  ·  Estágio {c.Estagio}";
            }
            if (c.EstagioDeChefe)
            {
                barraDoEstagio.Definir(1);
                progressoDoEstagio.text = "CHEFE!";
                progressoDoEstagio.color = KitDeInterface.Vermelho;
            }
            else
            {
                barraDoEstagio.Definir((float)c.VitoriasNoEstagio / c.VitoriasParaAvancar);
                progressoDoEstagio.text = $"{c.VitoriasNoEstagio}/{c.VitoriasParaAvancar}";
                progressoDoEstagio.color = KitDeInterface.TextoClaro;
            }

            listarPendencias.Executar(listaDePendencias);
            menu.Pendencias(listaDePendencias);
            MostrarPendencias();
        }

        void MostrarPendencias()
        {
            int quantas = Mathf.Min(listaDePendencias.Count, MaximoDePendencias);
            chip.gameObject.SetActive(quantas > 0);
            painelDePendencias.gameObject.SetActive(listaAberta && quantas > 0);
            if (quantas == 0) return;
            string texto = listaAberta ? "Pronto para fazer" : RegrasDasPendencias.Resumo(listaDePendencias);
            if (resumo.text != texto)
            {
                resumo.text = texto;
                chip.sizeDelta = new Vector2(Mathf.Max(200, resumo.preferredWidth + 60), 40);
            }
            if (!listaAberta) return;
            KitDeInterface.Ancorar(painelDePendencias, new Vector2(0, 1), new Vector2(116, -162), new Vector2(360, 18 + quantas * 32));
            for (int i = 0; i < MaximoDePendencias; i++)
            {
                bool tem = i < quantas;
                pendencias[i].gameObject.SetActive(tem);
                if (!tem) continue;
                if (pendencias[i].text != listaDePendencias[i].Texto) pendencias[i].text = listaDePendencias[i].Texto;
                pendencias[i].color = listaDePendencias[i].Tipo == TipoDePendencia.Chefe ? KitDeInterface.Vermelho : KitDeInterface.TextoClaro;
            }
        }

        void Update()
        {
            if (Time.unscaledTime >= proximaConsulta)
            {
                proximaConsulta = Time.unscaledTime + intervaloDeConsulta;
                Consultar();
            }

            bool visivel = !JanelaDoJogo.AlgumaAberta;
            grupo.alpha = Mathf.MoveTowards(grupo.alpha, visivel ? 1 : 0, Time.unscaledDeltaTime * 8);
            grupo.blocksRaycasts = visivel;

            if (mostrado != alvo || ultimoPoder == null) AtualizarPoder();
            AnimarGanho();
        }

        void AtualizarPoder()
        {
            mostrado = RegrasDePoder.ContadorAnimado(mostrado, alvo, Time.unscaledDeltaTime, rapidezDoContador);
            string textoDoPoder = FormatadorDeNumeros.Formatar(mostrado);
            if (textoDoPoder != ultimoPoder)
            {
                ultimoPoder = textoDoPoder;
                poder.text = textoDoPoder;
            }
        }

        void AnimarGanho()
        {
            float t = (Time.unscaledTime - ganhoDesde) / duracaoDoGanho;
            ganho.gameObject.SetActive(t < 1);
            if (t < 1)
            {
                var cor = KitDeInterface.Verde;
                cor.a = 1 - t;
                ganho.color = cor;
                ((RectTransform)ganho.transform).anchoredPosition = new Vector2(380, -30 + t * 24);
            }
        }
    }
}
