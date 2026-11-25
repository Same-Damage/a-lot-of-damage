using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Taverna própria: linha 0 é o ouro guardado (com o botão de coletar); as outras, um prédio cada
    public class JanelaDaTaverna : JanelaDoJogo
    {
        protected override string Titulo => "Taverna";
        protected override Key Tecla => Key.T;
        protected override IconeDoHud Icone => IconeDoHud.Taverna;
        protected override SistemaDoJogo? Sistema => SistemaDoJogo.Taverna;
        protected override int Ordem => 8;
        protected override int Linhas => catalogo.Predios.Count + 1;
        protected override float AlturaDaLinha => 84;

        ConsultarTaverna consultar;
        ColetarTaverna coletar;
        MelhorarPredio melhorar;
        Text[] nomes, textos, rotulos;
        Button[] botoes;
        SituacaoDaTaverna situacao;

        [Inject]
        public void Construir(ConsultarTaverna consultar, ColetarTaverna coletar, MelhorarPredio melhorar)
        {
            this.consultar = consultar;
            this.coletar = coletar;
            this.melhorar = melhorar;
        }

        protected override void Start()
        {
            int n = catalogo.Predios.Count + 1;
            nomes = new Text[n]; textos = new Text[n]; rotulos = new Text[n]; botoes = new Button[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            nomes[i] = Texto(linha, 24, 8, 24, i == 0 ? KitDeInterface.Verde : KitDeInterface.Dourado, 600, true);
            textos[i] = Texto(linha, 24, 42, 19, KitDeInterface.TextoClaro, 640);
            if (i == 0)
                botoes[i] = Botao(linha, "Coletar", 30, 280, Coletar);
            else
            {
                string id = catalogo.Predios[i - 1].Id;
                botoes[i] = Botao(linha, "Melhorar", 30, 280, () => Melhorar(id));
            }
            rotulos[i] = botoes[i].GetComponentInChildren<Text>();
        }

        void Coletar()
        {
            double ouro = coletar.Executar();
            if (ouro > 0) faixa.Mostrar($"+{FormatadorDeNumeros.Formatar(ouro)} ouro da taverna", 1.5f);
        }

        void Melhorar(string id)
        {
            if (melhorar.Executar(id)) faixa.Mostrar("Taverna melhorada!", 1.2f);
        }

        protected override void AtualizarConteudo()
        {
            situacao = consultar.Executar();
            base.AtualizarConteudo();
        }

        protected override void AtualizarLinha(int i)
        {
            if (i == 0)
            {
                nomes[0].text = $"Guardado: {FormatadorDeNumeros.Formatar(situacao.Guardado)} ouro";
                textos[0].text = $"{FormatadorDeNumeros.Formatar(situacao.OuroPorSegundo)} ouro/s, mesmo com o jogo fechado (guarda até {situacao.LimiteEmHoras:0} h)";
                botoes[0].interactable = situacao.Guardado > 0;
                return;
            }
            var s = situacao.Predios[i - 1];
            nomes[i].text = $"{s.Definicao.Nome}   nível {s.Nivel}";
            textos[i].text = s.Definicao.Descricao;
            botoes[i].interactable = s.Custo <= Progresso.Ouro;
            rotulos[i].text = $"Melhorar ({FormatadorDeNumeros.Formatar(s.Custo)})";
        }
    }
}
