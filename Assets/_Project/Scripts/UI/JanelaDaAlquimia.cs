using System.Collections.Generic;
using System.Text;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Alquimia: uma linha por poção, com receita (ingredientes e ouro), tempo restante e o botão de preparar e beber
    public class JanelaDaAlquimia : JanelaDoJogo
    {
        protected override string Titulo => "Alquimia";
        protected override Key Tecla => Key.L;
        protected override IconeDoHud Icone => IconeDoHud.Alquimia;
        protected override SistemaDoJogo? Sistema => SistemaDoJogo.Alquimia;
        protected override int Ordem => 10;
        protected override int Linhas => catalogo.Pocoes.Count;
        protected override float AlturaDaLinha => 96;

        ListarPocoes listar;
        PrepararPocao preparar;
        Text[] nomes, receitas;
        Button[] botoes;
        List<SituacaoDaPocao> situacoes;

        [Inject]
        public void Construir(ListarPocoes listar, PrepararPocao preparar)
        {
            this.listar = listar;
            this.preparar = preparar;
        }

        protected override void Start()
        {
            int n = catalogo.Pocoes.Count;
            nomes = new Text[n]; receitas = new Text[n]; botoes = new Button[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            string id = catalogo.Pocoes[i].Id;
            nomes[i] = Texto(linha, 24, 6, 24, KitDeInterface.Dourado, 640, true);
            receitas[i] = Texto(linha, 24, 42, 19, KitDeInterface.TextoClaro, 640);
            receitas[i].verticalOverflow = VerticalWrapMode.Overflow;
            botoes[i] = Botao(linha, "Preparar", 30, 220, () => Preparar(id));
        }

        void Preparar(string id)
        {
            if (preparar.Executar(id)) faixa.Mostrar("Poção preparada!", 1.2f);
        }

        protected override void AtualizarConteudo()
        {
            situacoes = listar.Executar();
            base.AtualizarConteudo();
        }

        protected override void AtualizarLinha(int i)
        {
            var s = situacoes[i];
            var d = s.Definicao;
            var p = Progresso;
            string estado = !s.Ativa ? "" : double.IsInfinity(s.MinutosRestantes) ? "   (ativa até a próxima coleta AFK)" : $"   (ativa: {System.Math.Ceiling(s.MinutosRestantes)} min)";
            nomes[i].text = $"{d.Nome}{estado}";
            nomes[i].color = s.Ativa ? KitDeInterface.Verde : KitDeInterface.Dourado;
            var sb = new StringBuilder(d.Descricao).Append("\n");
            foreach (var ing in d.Ingredientes)
                sb.Append($"{ing.Valor} {catalogo.NomeDoMaterial(ing.Id)} (tem {p.Material(ing.Id)})   ");
            sb.Append($"{FormatadorDeNumeros.Formatar(s.CustoEmOuro)} ouro");
            receitas[i].text = sb.ToString();
            botoes[i].interactable = s.TemIngredientes && s.CustoEmOuro <= p.Ouro;
        }
    }
}
