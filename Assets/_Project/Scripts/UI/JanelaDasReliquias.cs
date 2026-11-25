using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Uma linha por relíquia: nível, bônus atual e o botão de montar ou melhorar com fragmentos de chefe
    public class JanelaDasReliquias : JanelaDoJogo
    {
        protected override string Titulo => "Relíquias";
        protected override Key Tecla => Key.R;
        protected override IconeDoHud Icone => IconeDoHud.Reliquias;
        protected override SistemaDoJogo? Sistema => SistemaDoJogo.Reliquias;
        protected override int Ordem => 7;
        protected override int Linhas => catalogo.Reliquias.Count;
        protected override float AlturaDaLinha => 64;

        ListarReliquias listar;
        MelhorarReliquia melhorar;
        Text[] nomes, textos, rotulos;
        Button[] botoes;
        List<SituacaoDaReliquia> situacoes;

        [Inject]
        public void Construir(ListarReliquias listar, MelhorarReliquia melhorar)
        {
            this.listar = listar;
            this.melhorar = melhorar;
        }

        protected override void Start()
        {
            int n = catalogo.Reliquias.Count;
            nomes = new Text[n]; textos = new Text[n]; rotulos = new Text[n]; botoes = new Button[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            string id = catalogo.Reliquias[i].Id;
            nomes[i] = Texto(linha, 24, 4, 22, KitDeInterface.Dourado, 600, true);
            textos[i] = Texto(linha, 24, 34, 18, KitDeInterface.TextoClaro, 640);
            botoes[i] = Botao(linha, "Montar", 30, 280, () => Melhorar(id));
            ((RectTransform)botoes[i].transform).sizeDelta = new Vector2(280, 48);
            ((RectTransform)botoes[i].transform).anchoredPosition = new Vector2(-30, -8);
            rotulos[i] = botoes[i].GetComponentInChildren<Text>();
        }

        void Melhorar(string id)
        {
            if (melhorar.Executar(id)) faixa.Mostrar($"{catalogo.Reliquias.Find(id).Nome} nível {Progresso.NivelDaReliquia(id)}!", 1.5f);
        }

        protected override void AtualizarConteudo()
        {
            situacoes = listar.Executar();
            base.AtualizarConteudo();
        }

        protected override void AtualizarLinha(int i)
        {
            var s = situacoes[i];
            var p = Progresso;
            nomes[i].text = s.Nivel > 0 ? $"{s.Definicao.Nome}   nível {s.Nivel}" : $"{s.Definicao.Nome}   (não montada)";
            nomes[i].color = s.Nivel > 0 ? KitDeInterface.Dourado : KitDeInterface.TextoClaro;
            textos[i].text = s.Definicao.Descricao;
            botoes[i].interactable = s.Custo >= 0 && s.Custo <= p.Fragmentos;
            rotulos[i].text = s.Custo < 0 ? "Nível máximo" : $"{(s.Nivel == 0 ? "Montar" : "Melhorar")} ({s.Custo} frag., tem {p.Fragmentos})";
        }
    }

    static class ExtensoesDasReliquias
    {
        public static DefinicaoDeReliquia Find(this IReadOnlyList<DefinicaoDeReliquia> lista, string id)
        {
            foreach (var r in lista) if (r.Id == id) return r;
            return null;
        }
    }
}
