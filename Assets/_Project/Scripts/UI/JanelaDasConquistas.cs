using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Uma linha por meta: progresso até o próximo degrau e o botão de coletar os degraus prontos (cristais e +1% de dano cada)
    public class JanelaDasConquistas : JanelaDoJogo
    {
        protected override string Titulo => "Conquistas";
        protected override Key Tecla => Key.C;
        protected override int Ordem => 5;
        protected override int Linhas => catalogo.Conquistas.Count;
        protected override float AlturaDaLinha => 84;

        ListarConquistas listar;
        ColetarConquista coletar;
        Text[] nomes, textos;
        Barra[] barras;
        Button[] botoes;
        Text[] rotulos;

        [Inject]
        public void Construir(ListarConquistas listar, ColetarConquista coletar)
        {
            this.listar = listar;
            this.coletar = coletar;
        }

        protected override void Start()
        {
            int n = catalogo.Conquistas.Count;
            nomes = new Text[n]; textos = new Text[n]; barras = new Barra[n]; botoes = new Button[n]; rotulos = new Text[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            string id = catalogo.Conquistas[i].Id;
            nomes[i] = Texto(linha, 24, 6, 24, KitDeInterface.Dourado, 560, true);
            textos[i] = Texto(linha, 24, 38, 20, KitDeInterface.TextoClaro, 560);
            barras[i] = Barra(linha, 24, 66, 520, KitDeInterface.Azul);
            botoes[i] = Botao(linha, "Coletar", 30, 300, () => Coletar(id));
            rotulos[i] = botoes[i].GetComponentInChildren<Text>();
        }

        void Coletar(string id)
        {
            int cristais = coletar.Executar(id);
            if (cristais > 0) faixa.Mostrar($"CONQUISTA! +{cristais} cristais · +1% de dano por degrau", 2f);
        }

        List<SituacaoDaConquista> situacoes;

        protected override void AtualizarConteudo()
        {
            situacoes = listar.Executar();
            base.AtualizarConteudo();
        }

        protected override void AtualizarLinha(int i)
        {
            var s = situacoes[i];
            nomes[i].text = $"{s.Nome}   {s.Coletadas}/{s.TotalDeDegraus}";
            textos[i].text = s.Completa
                ? "Completa!"
                : $"{s.Texto}   ({FormatadorDeNumeros.Formatar(s.Valor)}/{FormatadorDeNumeros.Formatar(s.ProximaMeta)})";
            barras[i].Definir(s.Completa ? 1 : (float)System.Math.Min(1, s.Valor / s.ProximaMeta));
            botoes[i].interactable = s.Prontas > 0;
            rotulos[i].text = s.Prontas > 0 ? $"Coletar +{s.CristaisProntos} cristais" : (s.Completa ? "Completa" : "Coletar");
        }
    }
}
