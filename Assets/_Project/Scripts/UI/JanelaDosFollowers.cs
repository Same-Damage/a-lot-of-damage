using System.Collections.Generic;
using System.Text;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Followers e gacha: à esquerda os 40 followers (os que ainda não tem aparecem apagados), à direita o selecionado,
    // os botões de invocar com as taxas públicas e o pity, e o resultado da última invocação
    public class JanelaDosFollowers : JanelaDoJogo
    {
        const int Colunas = 5;
        const float LarguraDaCasa = 168, AlturaDaCasa = 86, Espaco = 6;

        protected override string Titulo => "Followers";
        protected override Key Tecla => Key.G;
        protected override int Ordem => 3;
        protected override IconeDoHud Icone => IconeDoHud.Followers;
        protected override SistemaDoJogo? Sistema => SistemaDoJogo.Followers;
        protected override int Linhas => 0;
        protected override float AlturaDaLinha => 0;
        protected override Vector2 Tamanho => new Vector2(1520, 880);
        protected override void MontarLinha(int i, RectTransform linha) { }
        protected override void AtualizarLinha(int i) { }

        static readonly string[] NomesDosEfeitos = { "Dano extra", "Ponto fraco", "Ouro", "AFK", "Crítico", "Drop" };

        ListarFollowers listar;
        Invocar invocar;
        EscolherFollower escolher;

        Image[] molduras;
        Text[] nomesDasCasas;
        Text detalhes, cristais, taxas, resultado;
        Button escolherBotao, um, dez;
        string selecionado;

        [Inject]
        public void Construir(ListarFollowers listar, Invocar invocar, EscolherFollower escolher)
        {
            this.listar = listar;
            this.invocar = invocar;
            this.escolher = escolher;
        }

        static Color CorDa(Raridade r)
        {
            ColorUtility.TryParseHtmlString(RegrasDosItens.CorDaRaridade(r), out var c);
            return c;
        }

        protected override void MontarConteudo(RectTransform painel)
        {
            int n = catalogo.Followers.Count;
            molduras = new Image[n];
            nomesDasCasas = new Text[n];
            for (int i = 0; i < n; i++)
            {
                string id = catalogo.Followers[i].Id;
                var casa = KitDeInterface.Painel("Follower", painel, Color.white, false);
                KitDeInterface.Ancorar((RectTransform)casa.transform, new Vector2(0, 1),
                    new Vector2(30 + (i % Colunas) * (LarguraDaCasa + Espaco), -84 - (i / Colunas) * (AlturaDaCasa + Espaco)), new Vector2(LarguraDaCasa, AlturaDaCasa));
                casa.gameObject.AddComponent<Button>().onClick.AddListener(() => { selecionado = id; AtualizarConteudo(); });
                molduras[i] = casa;
                var nome = KitDeInterface.Texto("Nome", casa.transform, 17, KitDeInterface.TextoClaro, TextAnchor.MiddleCenter, true);
                KitDeInterface.Esticar((RectTransform)nome.transform);
                nome.horizontalOverflow = HorizontalWrapMode.Wrap;
                nomesDasCasas[i] = nome;
            }

            float x0 = 30 + Colunas * (LarguraDaCasa + Espaco) + 30, largura = 1520 - x0 - 40;
            detalhes = TextoEm(painel, x0, -84, largura, 220, 22);
            escolherBotao = BotaoEm(painel, "Usar este follower", x0, -310, 300, Escolher);

            cristais = TextoEm(painel, x0, -400, largura, 40, 28);
            cristais.color = KitDeInterface.Azul;
            um = BotaoEm(painel, "Invocar ×1", x0, -450, 230, () => Invocar(1));
            dez = BotaoEm(painel, "Invocar ×10", x0 + 250, -450, 230, () => Invocar(10));
            taxas = TextoEm(painel, x0, -530, largura, 90, 18);
            resultado = TextoEm(painel, x0, -630, largura, 230, 20);
        }

        static Text TextoEm(RectTransform painel, float x, float y, float largura, float altura, int tamanho)
        {
            var t = KitDeInterface.Texto("Texto", painel, tamanho, KitDeInterface.TextoClaro, TextAnchor.UpperLeft);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            KitDeInterface.Ancorar((RectTransform)t.transform, new Vector2(0, 1), new Vector2(x, y), new Vector2(largura, altura));
            return t;
        }

        static Button BotaoEm(RectTransform painel, string rotulo, float x, float y, float largura, System.Action aoClicar)
        {
            var b = KitDeInterface.Botao(rotulo, painel, rotulo, aoClicar);
            KitDeInterface.Ancorar((RectTransform)b.transform, new Vector2(0, 1), new Vector2(x, y), new Vector2(largura, 60));
            return b;
        }

        void Escolher()
        {
            if (selecionado != null && escolher.Executar(selecionado)) faixa.Mostrar($"{catalogo.Follower(selecionado).Nome} luta com você!", 1.5f);
            AtualizarConteudo();
        }

        void Invocar(int quantidade)
        {
            var r = invocar.Executar(quantidade);
            if (r.Count == 0) return;
            var sb = new StringBuilder("Invocados:\n");
            var melhor = r[0];
            foreach (var x in r)
            {
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(CorDa(x.Raridade))}>{x.Nome}</color>");
                sb.Append(x.Novo ? " NOVO!" : x.CristaisDevolvidos > 0 ? $" (+{x.CristaisDevolvidos} cristais)" : $" ★{x.Estrelas}");
                sb.Append("   ");
                if (x.Raridade > melhor.Raridade) melhor = x;
            }
            resultado.text = sb.ToString();
            selecionado = melhor.FollowerId;
            if (melhor.Raridade >= Raridade.Epico) faixa.Mostrar($"{RegrasDosItens.Nome(melhor.Raridade).ToUpperInvariant()}! {melhor.Nome}", 3f);
            AtualizarConteudo();
        }

        protected override void AtualizarConteudo()
        {
            var lista = listar.Executar();
            var p = Progresso;
            SituacaoDoFollower atual = default;
            for (int i = 0; i < lista.Count && i < molduras.Length; i++)
            {
                var s = lista[i];
                var cor = CorDa(s.Definicao.Raridade);
                bool tem = s.Estrelas > 0;
                molduras[i].color = tem ? Color.Lerp(cor, Color.black, s.Definicao.Id == selecionado ? 0.1f : 0.55f) : new Color(0.15f, 0.13f, 0.12f);
                nomesDasCasas[i].text = (s.Ativo ? "► " : "") + s.Definicao.Nome + (tem ? $"\n{new string('★', s.Estrelas)}" : "\n?");
                nomesDasCasas[i].color = tem ? Color.white : new Color(0.5f, 0.45f, 0.4f);
                if (s.Definicao.Id == selecionado || (selecionado == null && s.Ativo)) atual = s;
            }

            if (atual.Definicao != null)
            {
                var d = atual.Definicao;
                string efeito = d.Efeito == EfeitoDoFollower.Critico
                    ? $"+{atual.Efeito * 100:0.#}% de chance crítica"
                    : d.Efeito == EfeitoDoFollower.DanoExtra ? "ataque mais forte" : $"+{atual.Efeito * 100:0}%";
                detalhes.text = $"<color=#{ColorUtility.ToHtmlStringRGB(CorDa(d.Raridade))}><b>{d.Nome}</b>  {RegrasDosItens.Nome(d.Raridade)}</color>" +
                                $"  {(atual.Estrelas > 0 ? new string('★', atual.Estrelas) : "(ainda não tem)")}\n" +
                                $"{d.Descricao}\n{NomesDosEfeitos[(int)d.Efeito]}: {efeito}\n" +
                                $"Ataca a cada {d.IntervaloDeAtaque:0.#} s com {atual.FracaoDoAutoAtaque * 100:0}% do seu auto-ataque" +
                                (atual.Ativo ? "\n<color=#73FF80>Lutando com você</color>" : "");
                escolherBotao.interactable = atual.Estrelas > 0 && !atual.Ativo;
            }

            var g = catalogo.Gacha;
            cristais.text = $"Cristais: {FormatadorDeNumeros.Formatar(p.Cristais)}";
            um.interactable = p.Cristais >= global::ALotOfDamage.Core.Invocar.Custo(g, 1);
            dez.interactable = p.Cristais >= global::ALotOfDamage.Core.Invocar.Custo(g, 10);
            taxas.text = $"×1: {g.CustoPorInvocacao} · ×10: {g.CustoDeDez} cristais\n" +
                         $"Taxas: Comum {g.Taxas[0]}% · Incomum {g.Taxas[1]}% · Raro {g.Taxas[2]}% · Épico {g.Taxas[3]}% · Lendário {g.Taxas[4]}%\n" +
                         $"Épico garantido em {g.PityEpico - p.InvocacoesSemEpico} · Lendário garantido em {g.PityLendario - p.InvocacoesSemLendario}";
        }
    }
}
