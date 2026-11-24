using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Ascensão: mostra as Almas, o multiplicador e quanto ganharia agora; ascender pede um segundo clique para confirmar
    public class JanelaDaAscensao : JanelaDoJogo
    {
        const float TempoParaConfirmar = 4f;

        protected override string Titulo => "Ascensão";
        protected override Key Tecla => Key.K;
        protected override int Ordem => 6;
        protected override IconeDoHud Icone => IconeDoHud.Ascensao;
        protected override TipoDePendencia? Pendencia => TipoDePendencia.Ascensao;
        protected override int Linhas => 1;
        protected override float AlturaDaLinha => 300;

        ConsultarAscensao consultar;
        Ascender ascender;
        Text almas, ganho, aviso, rotulo;
        Button botao;
        float confirmarAte = -1;

        [Inject]
        public void Construir(ConsultarAscensao consultar, Ascender ascender)
        {
            this.consultar = consultar;
            this.ascender = ascender;
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            almas = Texto(linha, 24, 10, 30, KitDeInterface.Dourado, 940, true);
            ganho = Texto(linha, 24, 60, 24, KitDeInterface.TextoClaro, 940);
            aviso = Texto(linha, 24, 150, 20, KitDeInterface.TextoClaro, 940);
            aviso.text = "Recomeça no estágio 1 com upgrades no nível 1 e sem ouro.\nMantém itens, materiais, bestiário e as Almas, que multiplicam todo o dano para sempre.";
            botao = Botao(linha, "Ascender", 30, 260, Clicar);
            rotulo = botao.GetComponentInChildren<Text>();
            ((RectTransform)botao.transform).anchoredPosition = new Vector2(-30, -230);
        }

        void Clicar()
        {
            if (Time.unscaledTime > confirmarAte)
            {
                confirmarAte = Time.unscaledTime + TempoParaConfirmar;
                return;
            }
            confirmarAte = -1;
            var antes = consultar.Executar();
            if (ascender.Executar())
                faixa.Mostrar($"ASCENSÃO! +{FormatadorDeNumeros.Formatar(antes.AlmasGanhas)} Almas · dano ×{FormatadorDeNumeros.Formatar(antes.MultiplicadorDepois)}", 3f);
        }

        protected override void AtualizarLinha(int i)
        {
            var a = consultar.Executar();
            almas.text = $"Almas: {FormatadorDeNumeros.Formatar(a.Almas)}   ·   dano ×{FormatadorDeNumeros.Formatar(a.MultiplicadorAtual)}   ·   ascensões: {a.Ascensoes}";
            ganho.text = a.Pode
                ? $"Ascender agora: +{FormatadorDeNumeros.Formatar(a.AlmasGanhas)} Almas  →  dano ×{FormatadorDeNumeros.Formatar(a.MultiplicadorDepois)}\nQuanto mais longe você for antes de ascender, mais Almas ganha."
                : $"Libera no estágio {a.EstagioMinimo}.";
            ganho.color = a.Pode ? KitDeInterface.Verde : KitDeInterface.TextoClaro;
            botao.interactable = a.Pode;
            rotulo.text = Time.unscaledTime <= confirmarAte ? "Confirmar?" : "Ascender";
        }
    }
}
