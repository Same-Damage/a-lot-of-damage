using System;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Conta online: situação da nuvem, apelido, usuário e senha (para entrar em outro aparelho) e apagar a conta
    public class JanelaDaConta : JanelaDoJogo
    {
        protected override string Titulo => "Conta";
        protected override Key Tecla => Key.O;
        protected override int Ordem => 12;
        protected override IconeDoHud Icone => IconeDoHud.Conta;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1000, 720);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        SincronizarProgresso sincronia;
        IContaOnline conta;

        Text situacao, aviso, apelidoAtual, vinculo, rotuloDeApagar;
        InputField apelido, usuario, senha;
        GameObject grupo;
        Button vincular;
        bool fazendo, confirmarEntrar;
        float fimDaConfirmacao;

        [Inject]
        public void Construir(SincronizarProgresso sincronia, IContaOnline conta)
        {
            this.sincronia = sincronia;
            this.conta = conta;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            situacao = Texto(painel, 52, 80, 24, KitDeInterface.TextoClaro, 640);
            Botao(painel, "Sincronizar", 52, 200, () => Fazer(Sincronizar, null), 76);

            var g = KitDeInterface.Novo("Com conta", painel);
            KitDeInterface.Esticar(g);
            grupo = g.gameObject;

            Texto(g, 52, 160, 26, KitDeInterface.Dourado, negrito: true).text = "Apelido no ranking";
            apelidoAtual = Texto(g, 420, 162, 22, KitDeInterface.TextoClaro, 500);
            apelido = KitDeInterface.Campo("Apelido", g, "novo apelido", false, 20);
            KitDeInterface.Ancorar((RectTransform)apelido.transform, new Vector2(0, 1), new Vector2(52, -206), new Vector2(560, 56));
            Botao(g, "Trocar", 52, 200, () => Fazer(() => conta.TrocarApelido(apelido.text.Trim()), "Apelido trocado."), 206);

            Texto(g, 52, 296, 26, KitDeInterface.Dourado, negrito: true).text = "Jogar em outro aparelho";
            vinculo = Texto(g, 52, 336, 20, KitDeInterface.TextoClaro, 900);
            vinculo.horizontalOverflow = HorizontalWrapMode.Wrap;
            ((RectTransform)vinculo.transform).sizeDelta = new Vector2(890, 56);
            usuario = KitDeInterface.Campo("Usuario", g, "usuário", false, 20);
            KitDeInterface.Ancorar((RectTransform)usuario.transform, new Vector2(0, 1), new Vector2(52, -404), new Vector2(430, 56));
            senha = KitDeInterface.Campo("Senha", g, "senha", true, 30);
            KitDeInterface.Ancorar((RectTransform)senha.transform, new Vector2(0, 1), new Vector2(502, -404), new Vector2(446, 56));
            vincular = KitDeInterface.Botao("Vincular", g, "Criar usuário e senha", Vincular);
            KitDeInterface.Ancorar((RectTransform)vincular.transform, new Vector2(0, 1), new Vector2(52, -480), new Vector2(430, 60));
            var entrar = KitDeInterface.Botao("Entrar", g, "Já tenho conta: entrar", Entrar);
            KitDeInterface.Ancorar((RectTransform)entrar.transform, new Vector2(0, 1), new Vector2(502, -480), new Vector2(446, 60));

            var apagar = KitDeInterface.Botao("Apagar", g, "Apagar conta online", Apagar, 20);
            KitDeInterface.Ancorar((RectTransform)apagar.transform, new Vector2(0, 0), new Vector2(52, 40), new Vector2(430, 52));
            rotuloDeApagar = apagar.GetComponentInChildren<Text>();

            aviso = KitDeInterface.Texto("Aviso", painel, 22, KitDeInterface.Dourado, TextAnchor.MiddleLeft);
            KitDeInterface.Ancorar((RectTransform)aviso.transform, new Vector2(0, 0), new Vector2(52, 110), new Vector2(896, 34));
        }

        Task Sincronizar() => conta.Conectada && sincronia.Situacao != SituacaoDaNuvem.SemConexao ? sincronia.EnviarSeMudou() : sincronia.Abrir();

        void Vincular() => Fazer(async () =>
        {
            Exigir(RegrasDaConta.ProblemaDoUsuario(usuario.text) ?? RegrasDaConta.ProblemaDaSenha(senha.text));
            await conta.Vincular(usuario.text, senha.text);
            if (senha) senha.text = "";
        }, "Pronto: entre com este usuário e senha em outro aparelho.");

        void Entrar()
        {
            string problema = RegrasDaConta.ProblemaDoUsuario(usuario.text) ?? RegrasDaConta.ProblemaDaSenha(senha.text);
            if (problema != null) { aviso.text = problema; return; }
            // a conta de convidado sem senha não tem volta
            if (!conta.Vinculada && !confirmarEntrar)
            {
                confirmarEntrar = true;
                aviso.text = "A conta de convidado atual será deixada para trás. Clique em entrar de novo.";
                return;
            }
            confirmarEntrar = false;
            Fazer(async () =>
            {
                await sincronia.EntrarComConta(usuario.text, senha.text);
                if (senha) senha.text = "";
            }, "Você entrou na conta.");
        }

        void Apagar()
        {
            if (Time.unscaledTime > fimDaConfirmacao)
            {
                fimDaConfirmacao = Time.unscaledTime + 6f;
                aviso.text = "Apaga a conta e o progresso da nuvem, sem volta. O progresso deste aparelho fica.";
                return;
            }
            fimDaConfirmacao = 0;
            Fazer(sincronia.ApagarConta, "Conta apagada. O jogo segue só neste aparelho.");
        }

        static void Exigir(string problema) { if (problema != null) throw new FalhaOnline(problema); }

        async void Fazer(Func<Task> acao, string sucesso)
        {
            if (fazendo) return;
            fazendo = true;
            aviso.text = "Aguarde...";
            string resultado;
            try { await acao(); resultado = sucesso ?? ""; }
            catch (FalhaOnline e) { resultado = e.Message; }
            catch (Exception e) { Debug.LogException(e); resultado = "Algo deu errado."; }
            if (!this) return;   // a cena pode ter recomeçado com o progresso da nuvem
            fazendo = false;
            aviso.text = resultado;
        }

        protected override void AtualizarConteudo()
        {
            if (grupo.activeSelf != conta.Conectada) grupo.SetActive(conta.Conectada);
            Escrever(situacao, Situacao());
            if (!conta.Conectada) return;
            Escrever(apelidoAtual, string.IsNullOrEmpty(conta.Apelido) ? "sem apelido" : conta.Apelido);
            if (vincular.gameObject.activeSelf == conta.Vinculada) vincular.gameObject.SetActive(!conta.Vinculada);
            Escrever(vinculo, conta.Vinculada ? "Esta conta já tem usuário e senha."
                : "Você está como convidado: se desinstalar o jogo, a conta se perde. Crie um usuário e senha só deste jogo, sem usar e-mail nem senha de outro lugar.");
            Escrever(rotuloDeApagar, Time.unscaledTime < fimDaConfirmacao ? "Clique de novo para apagar" : "Apagar conta online");
        }

        static void Escrever(Text t, string s) { if (t.text != s) t.text = s; }

        string Situacao()
        {
            if (sincronia.Encerrada) return "Conta apagada. Reabra o jogo para criar outra.";
            switch (sincronia.Situacao)
            {
                case SituacaoDaNuvem.Desligada: return "O online não está configurado nesta versão.";
                case SituacaoDaNuvem.SemConexao: return "Sem conexão: o progresso está salvo neste aparelho.";
                case SituacaoDaNuvem.EmDia: return "Conectado. O progresso está na nuvem.";
                case SituacaoDaNuvem.Conflito: return "Escolha qual progresso vale.";
                case SituacaoDaNuvem.Recusado: return $"A nuvem recusou o progresso: {sincronia.Motivo}.";
                case SituacaoDaNuvem.JogoAntigo: return "Atualize o jogo para sincronizar.";
                default: return "Sincronizando...";
            }
        }
    }
}
