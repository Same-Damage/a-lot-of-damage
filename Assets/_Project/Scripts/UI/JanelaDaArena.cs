using System;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Arena: três adversários (cópias dos saves de outros jogadores); o servidor decide a luta e a janela mostra as rodadas
    public class JanelaDaArena : JanelaDoJogo
    {
        const float Validade = 60f, SegundosPorRodada = 0.7f;

        protected override string Titulo => "Arena";
        protected override Key Tecla => Key.V;
        protected override int Ordem => 12;
        protected override IconeDoHud Icone => IconeDoHud.Arena;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1180, 800);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        ArenaDoJogador arena;
        IContaOnline conta;

        Text liga, meus, tempo, aviso, duelo, rodadas;
        Image[] cartoes;
        Text[] nomes, textos;
        Button[] lutar;
        Button outros;
        GameObject lista;
        RectTransform luta;
        bool fazendo;
        float buscadoEm = float.NegativeInfinity, inicioDaLuta = -1;
        VistaDaArena mostrada;
        ResultadoDaArena resultado;
        int rodadasMostradas;

        [Inject]
        public void Construir(ArenaDoJogador arena, IContaOnline conta)
        {
            this.arena = arena;
            this.conta = conta;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            liga = Texto(painel, 52, 82, 30, KitDeInterface.Dourado, 560, true);
            meus = Texto(painel, 52, 126, 22, KitDeInterface.TextoClaro, 620);
            tempo = Texto(painel, 720, 86, 22, KitDeInterface.TextoClaro, 420);

            var l = KitDeInterface.Novo("Adversários", painel);
            KitDeInterface.Esticar(l);
            lista = l.gameObject;
            int n = catalogo.Arena.Adversarios;
            cartoes = new Image[n]; nomes = new Text[n]; textos = new Text[n]; lutar = new Button[n];
            for (int i = 0; i < n; i++)
            {
                int k = i;
                cartoes[i] = KitDeInterface.Painel($"Adversário {i}", l, new Color(0, 0, 0, 0.32f), false);
                var rt = (RectTransform)cartoes[i].transform;
                KitDeInterface.Ancorar(rt, new Vector2(0, 1), new Vector2(52 + i * 366, -190), new Vector2(344, 380));
                nomes[i] = KitDeInterface.Texto("Nome", rt, 26, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
                nomes[i].supportRichText = false;
                KitDeInterface.Ancorar((RectTransform)nomes[i].transform, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(320, 36));
                textos[i] = KitDeInterface.Texto("Dados", rt, 23, KitDeInterface.TextoClaro, TextAnchor.UpperCenter);
                KitDeInterface.Ancorar((RectTransform)textos[i].transform, new Vector2(0.5f, 1), new Vector2(0, -84), new Vector2(320, 180));
                lutar[i] = KitDeInterface.Botao("Lutar", rt, "Lutar", () => Lutar(k), 26);
                KitDeInterface.Ancorar((RectTransform)lutar[i].transform, new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(260, 64));
            }
            outros = KitDeInterface.Botao("Outros", l, "Outros adversários", () => Fazer(() => arena.Pedir(AcaoDaArena.OutrosAdversarios)));
            KitDeInterface.Ancorar((RectTransform)outros.transform, new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(360, 56));

            luta = KitDeInterface.Novo("Luta", painel);
            KitDeInterface.Esticar(luta);
            duelo = KitDeInterface.Texto("Duelo", luta, 34, KitDeInterface.Dourado, TextAnchor.MiddleCenter, true);
            duelo.supportRichText = false;
            KitDeInterface.Ancorar((RectTransform)duelo.transform, new Vector2(0.5f, 1), new Vector2(0, -230), new Vector2(1000, 50));
            rodadas = KitDeInterface.Texto("Rodadas", luta, 30, KitDeInterface.TextoClaro, TextAnchor.UpperCenter);
            KitDeInterface.Ancorar((RectTransform)rodadas.transform, new Vector2(0.5f, 1), new Vector2(0, -310), new Vector2(1000, 330));
            luta.gameObject.SetActive(false);

            aviso = KitDeInterface.Texto("Aviso", painel, 22, KitDeInterface.Dourado, TextAnchor.MiddleCenter);
            KitDeInterface.Ancorar((RectTransform)aviso.transform, new Vector2(0.5f, 0), new Vector2(0, 48), new Vector2(1060, 40));
            lista.SetActive(false);
        }

        void Lutar(int i)
        {
            var v = arena.Vista;
            if (inicioDaLuta >= 0 || v == null || i >= v.Adversarios.Count) return;
            string id = v.Adversarios[i].JogadorId;
            Fazer(async () =>
            {
                await arena.Pedir(AcaoDaArena.Lutar, id);
                if (!this) return;
                resultado = arena.Vista.Resultado;
                inicioDaLuta = Time.unscaledTime;
                rodadasMostradas = -1;
                duelo.text = $"Você  ×  {NomeDe(resultado.Adversario)}";
                lista.SetActive(false);
                luta.gameObject.SetActive(true);
            });
        }

        static string NomeDe(string nome) => string.IsNullOrEmpty(nome) ? "Sem nome" : nome;

        async void Fazer(Func<Task> acao)
        {
            if (fazendo) return;
            fazendo = true;
            aviso.text = "Aguarde...";
            string erro = "";
            try { await acao(); }
            catch (FalhaOnline e) { erro = e.Message; }
            catch (Exception e) { Debug.LogException(e); erro = "Algo deu errado."; }
            if (!this) return;
            fazendo = false;
            buscadoEm = Time.unscaledTime;
            aviso.text = erro;
            if (erro == "" && arena.Vista.RecompensaDaTemporada > 0) aviso.text = $"Temporada encerrada: +{arena.Vista.RecompensaDaTemporada} cristais!";
        }

        protected override void AoAbrir()
        {
            mostrada = null;
            if (!conta.Conectada) aviso.text = "Sem conexão: a arena aparece quando você estiver online.";
        }

        protected override void AtualizarConteudo()
        {
            if (inicioDaLuta >= 0) { MostrarLuta(); return; }
            if (conta.Conectada && !fazendo && Time.unscaledTime - buscadoEm > Validade) Fazer(() => arena.Pedir(AcaoDaArena.Ver));
            var v = arena.Vista;
            if (v == null || v == mostrada) return;
            mostrada = v;
            lista.SetActive(true);
            liga.text = $"Liga {v.Liga}  ·  {v.Pontos} pontos";
            meus.text = $"Seu Poder: {FormatadorDeNumeros.Formatar(v.MeuPoder)}   Lutas hoje: {v.LutasRestantes}";
            var t = TimeSpan.FromSeconds(Math.Max(0, v.SegundosParaVirar));
            tempo.text = $"Fim da temporada em {(t.Days > 0 ? $"{t.Days} d {t.Hours} h" : $"{t.Hours} h {t.Minutes:00} min")}\nSua liga vale {v.CristaisDaLiga} cristais";
            for (int i = 0; i < cartoes.Length; i++)
            {
                bool ha = i < v.Adversarios.Count;
                cartoes[i].gameObject.SetActive(ha);
                if (!ha) continue;
                var a = v.Adversarios[i];
                double chance = RegrasDaArena.Chance(catalogo.Arena, v.MeuPoder, a.Poder);
                nomes[i].text = NomeDe(a.Nome);
                textos[i].text = $"{(string.IsNullOrEmpty(a.HeroiId) ? "Cavaleiro" : a.HeroiId)}\nEstágio {a.Estagio}\nPoder {FormatadorDeNumeros.Formatar(a.Poder)}\n{a.Pontos} pontos\n"
                    + $"Vitória vale {RegrasDaArena.PontosDaVitoria(catalogo.Arena, v.Pontos, a.Pontos)}";
                textos[i].color = chance >= 0.5 ? KitDeInterface.TextoClaro : KitDeInterface.DouradoClaro;
                lutar[i].interactable = v.LutasRestantes > 0;
            }
            if (v.Adversarios.Count == 0) aviso.text = "Ainda não há outros jogadores na arena.";
        }

        void MostrarLuta()
        {
            float passou = Time.unscaledTime - inicioDaLuta;
            int n = Mathf.Min(resultado.Rodadas.Count, (int)(passou / SegundosPorRodada));
            if (n != rodadasMostradas)
            {
                rodadasMostradas = n;
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < n; i++)
                    sb.Append(resultado.Rodadas[i] ? "<color=#73FF80>" : "<color=#E0545C>").Append("Rodada ").Append(i + 1).Append(resultado.Rodadas[i] ? ": você venceu" : ": perdeu").Append("</color>\n");
                if (n == resultado.Rodadas.Count)
                    sb.Append('\n').Append(resultado.Venceu ? $"<color=#FFCC52>VITÓRIA!  +{resultado.PontosGanhos} pontos  +{catalogo.Arena.CristaisPorVitoria} cristais</color>" : "Derrota. Nenhum ponto perdido.");
                rodadas.text = sb.ToString();
            }
            if (passou < (resultado.Rodadas.Count + 4) * SegundosPorRodada) return;
            inicioDaLuta = -1;
            luta.gameObject.SetActive(false);
            mostrada = null;
        }
    }
}
