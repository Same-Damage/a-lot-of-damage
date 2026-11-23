using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // HUD do mapa: Poder Total em destaque (contador animado + ganho relativo), ouro, estágio e região
    // da campanha, e a lista de pendências ("coisas prontas") na lateral.
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

        Text poder, ganho, ouro, estagio, regiao, progressoDoEstagio;
        Barra barraDoEstagio;
        readonly Text[] pendencias = new Text[MaximoDePendencias];
        RectTransform painelDePendencias;
        readonly List<Pendencia> listaDePendencias = new List<Pendencia>(16);

        double alvo, mostrado = -1;
        float proximaConsulta, ganhoDesde = -10;
        string ultimoPoder;

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

            var painel = (RectTransform)KitDeInterface.Painel("HUD", canvas).transform;
            KitDeInterface.Ancorar(painel, new Vector2(0, 1), new Vector2(20, -20), new Vector2(560, 250));
            poder = Linha(painel, 14, 46, KitDeInterface.Dourado, true);
            ganho = Linha(painel, 14, 34, KitDeInterface.Verde, true);
            ganho.alignment = TextAnchor.MiddleRight;
            ouro = Linha(painel, 74, 26, KitDeInterface.TextoClaro, false);
            estagio = Linha(painel, 112, 26, KitDeInterface.TextoClaro, true);
            regiao = Linha(painel, 146, 22, KitDeInterface.Azul, false);
            barraDoEstagio = KitDeInterface.NovaBarra("Estagio", painel, KitDeInterface.Dourado);
            KitDeInterface.Ancorar(barraDoEstagio.Rect, new Vector2(0, 1), new Vector2(24, -186), new Vector2(380, 16));
            progressoDoEstagio = KitDeInterface.Texto("Vitorias", painel, 20, KitDeInterface.TextoClaro);
            KitDeInterface.Ancorar((RectTransform)progressoDoEstagio.transform, new Vector2(0, 1), new Vector2(414, -178), new Vector2(140, 30));
            var dica = Linha(painel, 210, 18, new Color(1, 1, 1, 0.6f), false);
            dica.text = "F / Espaço: atacar (antes de ser visto = ataque preventivo)";

            painelDePendencias = (RectTransform)KitDeInterface.Painel("Pendencias", canvas).transform;
            KitDeInterface.Ancorar(painelDePendencias, new Vector2(1, 1), new Vector2(-20, -240), new Vector2(420, 60 + MaximoDePendencias * 40));
            var titulo = KitDeInterface.Texto("Titulo", painelDePendencias, 24, KitDeInterface.Dourado, TextAnchor.MiddleLeft, true);
            KitDeInterface.Ancorar((RectTransform)titulo.transform, new Vector2(0, 1), new Vector2(18, -10), new Vector2(380, 36));
            titulo.text = "Pronto para fazer";
            for (int i = 0; i < MaximoDePendencias; i++)
            {
                pendencias[i] = KitDeInterface.Texto("Pendencia", painelDePendencias, 20, KitDeInterface.TextoClaro);
                KitDeInterface.Ancorar((RectTransform)pendencias[i].transform, new Vector2(0, 1), new Vector2(18, -52 - i * 40), new Vector2(390, 34));
            }

            alvo = calcularPoder.Executar();
            Consultar();
        }

        static Text Linha(RectTransform painel, float y, int tamanho, Color cor, bool negrito)
        {
            var t = KitDeInterface.Texto("Linha", painel, tamanho, cor, TextAnchor.MiddleLeft, negrito);
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0, 1), new Vector2(24, -y), new Vector2(512, tamanho + 12));
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
            ouro.text = $"Ouro {FormatadorDeNumeros.Formatar(p.Ouro)}   ·   Vitórias {p.Vitorias}";

            var c = consultarCampanha.Executar();
            estagio.text = $"Estágio {c.Estagio}";
            regiao.text = c.Regiao?.Nome ?? "";
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
            painelDePendencias.gameObject.SetActive(listaDePendencias.Count > 0);
            for (int i = 0; i < MaximoDePendencias; i++)
            {
                bool tem = i < listaDePendencias.Count;
                pendencias[i].gameObject.SetActive(tem);
                if (!tem) continue;
                pendencias[i].text = "• " + listaDePendencias[i].Texto;
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
                poder.text = "Poder " + textoDoPoder;
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
                ((RectTransform)ganho.transform).anchoredPosition = new Vector2(24, -14 + t * 30);
            }
        }
    }
}
