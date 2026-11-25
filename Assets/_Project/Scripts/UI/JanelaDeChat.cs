using System;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Conversa por texto da cidade e da guilda. Tocar numa mensagem permite bloquear ou denunciar quem escreveu
    public class JanelaDeChat : JanelaDoJogo
    {
        const int Visiveis = 13;

        protected override string Titulo => "Conversa";
        protected override Key Tecla => Key.Y;
        protected override int Ordem => 14;
        protected override IconeDoHud Icone => IconeDoHud.Conversa;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1060, 820);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        ConversaDoJogador conversa;
        GuildaDoJogador guilda;
        IContaOnline conta;

        readonly Text[] textos = new Text[Visiveis];
        readonly Button[] botoes = new Button[Visiveis];
        readonly MensagemDeChat[] mostradas = new MensagemDeChat[Visiveis];
        Text[] abas;
        Text aviso, deQuem;
        InputField campo;
        RectTransform opcoes;
        MensagemDeChat escolhida;
        CanalDeChat canal = CanalDeChat.Guilda;
        int versao = -1;
        bool fazendo, tentouGuilda;

        [Inject]
        public void Construir(ConversaDoJogador conversa, GuildaDoJogador guilda, IContaOnline conta)
        {
            this.conversa = conversa;
            this.guilda = guilda;
            this.conta = conta;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            abas = new[] { Aba(painel, "Cidade", 52, CanalDeChat.Cidade), Aba(painel, "Guilda", 292, CanalDeChat.Guilda) };
            for (int i = 0; i < Visiveis; i++)
            {
                int k = i;
                var fundo = KitDeInterface.Painel($"Mensagem {i}", painel, new Color(0, 0, 0, 0.01f), false);
                KitDeInterface.Ancorar((RectTransform)fundo.transform, new Vector2(0, 1), new Vector2(52, -150 - i * 40), new Vector2(956, 38));
                botoes[i] = fundo.gameObject.AddComponent<Button>();
                botoes[i].onClick.AddListener(() => Escolher(mostradas[k]));
                textos[i] = KitDeInterface.Texto("Texto", fundo.transform, 21, KitDeInterface.TextoClaro);
                textos[i].supportRichText = false;
                KitDeInterface.Esticar((RectTransform)textos[i].transform);
            }

            opcoes = KitDeInterface.Novo("Opções", painel);
            KitDeInterface.Ancorar(opcoes, new Vector2(0, 0), new Vector2(52, 132), new Vector2(956, 48));
            deQuem = KitDeInterface.Texto("De quem", opcoes, 21, KitDeInterface.Dourado);
            deQuem.supportRichText = false;
            KitDeInterface.Ancorar((RectTransform)deQuem.transform, new Vector2(0, 0.5f), Vector2.zero, new Vector2(430, 40));
            BotaoNas(opcoes, "Bloquear", 0, () => { conversa.Bloquear(escolhida.JogadorId); Escolher(null); aviso.text = "Jogador bloqueado."; });
            BotaoNas(opcoes, "Denunciar", 170, Denunciar);
            BotaoNas(opcoes, "Cancelar", 340, () => Escolher(null));
            opcoes.gameObject.SetActive(false);

            campo = KitDeInterface.Campo("Mensagem", painel, "escreva e aperte Enter", false, RegrasDoChat.TamanhoMaximo);
            KitDeInterface.Ancorar((RectTransform)campo.transform, new Vector2(0, 0), new Vector2(52, 66), new Vector2(770, 56));
            campo.onSubmit.AddListener(_ => Enviar());
            var enviar = KitDeInterface.Botao("Enviar", painel, "Enviar", Enviar);
            KitDeInterface.Ancorar((RectTransform)enviar.transform, new Vector2(1, 0), new Vector2(-52, 66), new Vector2(170, 56));
            aviso = KitDeInterface.Texto("Aviso", painel, 19, KitDeInterface.Dourado);
            KitDeInterface.Ancorar((RectTransform)aviso.transform, new Vector2(0, 0), new Vector2(52, 30), new Vector2(956, 30));
        }

        Text Aba(RectTransform painel, string rotulo, float x, CanalDeChat c)
        {
            var b = KitDeInterface.Botao($"Aba {rotulo}", painel, rotulo, () => { canal = c; versao = -1; Escolher(null); });
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(0, 1), new Vector2(x, -80), new Vector2(226, 54));
            return b.GetComponentInChildren<Text>();
        }

        static void BotaoNas(RectTransform pai, string rotulo, float xDaDireita, Action acao)
        {
            var b = KitDeInterface.Botao(rotulo, pai, rotulo, acao, 20);
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(1, 0.5f), new Vector2(-xDaDireita, 0), new Vector2(160, 46));
        }

        void Escolher(MensagemDeChat m)
        {
            escolhida = m != null && !m.Minha ? m : null;
            opcoes.gameObject.SetActive(escolhida != null);
            if (escolhida != null) deQuem.text = escolhida.Nome;
        }

        void Denunciar()
        {
            var m = escolhida;
            Escolher(null);
            Fazer(() => conversa.Denunciar(m), "Denúncia enviada e jogador bloqueado.");
        }

        void Enviar()
        {
            string texto = campo.text;
            if (string.IsNullOrWhiteSpace(texto)) return;
            Fazer(async () =>
            {
                await conversa.Enviar(canal, texto);
                if (!campo) return;
                campo.text = "";
                campo.ActivateInputField();
            }, "");
        }

        async void Fazer(Func<System.Threading.Tasks.Task> acao, string sucesso)
        {
            if (fazendo) return;
            fazendo = true;
            string resultado = sucesso;
            try { await acao(); }
            catch (FalhaOnline e) { resultado = e.Message; }
            catch (Exception e) { Debug.LogException(e); resultado = "Algo deu errado."; }
            if (!this) return;
            fazendo = false;
            aviso.text = resultado;
        }

        protected override void AoAbrir()
        {
            versao = -1;
            tentouGuilda = false;
            if (conversa.Conectado(CanalDeChat.Cidade)) canal = CanalDeChat.Cidade;
        }

        protected override void AtualizarConteudo()
        {
            // fora da cidade, a conversa da guilda entra quando a janela abre
            if (!tentouGuilda && !fazendo && conta.Conectada)
            {
                tentouGuilda = true;
                Fazer(async () =>
                {
                    if (guilda.Vista == null) await guilda.Pedir(AcaoDaGuilda.Ver);
                    await conversa.Entrar(CanalDeChat.Guilda, guilda.Vista.Canal);
                }, "");
            }
            if (versao == conversa.Versao) return;
            versao = conversa.Versao;
            for (int i = 0; i < abas.Length; i++) abas[i].color = i == (int)canal ? KitDeInterface.Dourado : KitDeInterface.TextoClaro;

            int n = 0;
            for (int k = conversa.Historico.Count - 1; k >= 0 && n < Visiveis; k--)
            {
                var m = conversa.Historico[k];
                if (m.Canal == canal) mostradas[n++] = m;
            }
            Array.Reverse(mostradas, 0, n);
            for (int i = 0; i < Visiveis; i++)
            {
                if (i >= n) mostradas[i] = null;
                botoes[i].gameObject.SetActive(i < n);
                if (i >= n) continue;
                textos[i].text = $"{(string.IsNullOrEmpty(mostradas[i].Nome) ? "Sem nome" : mostradas[i].Nome)}: {mostradas[i].Texto}";
                textos[i].color = mostradas[i].Minha ? KitDeInterface.DouradoClaro : KitDeInterface.TextoClaro;
            }
            if (n == 0 && !conversa.Conectado(canal)) aviso.text = canal == CanalDeChat.Cidade ? "Entre na cidade para conversar." : "Entre em uma guilda para conversar.";
        }
    }
}
