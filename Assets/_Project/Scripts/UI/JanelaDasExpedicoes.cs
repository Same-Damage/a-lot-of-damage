using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace ALotOfDamage.UI
{
    // Expedições: linha 0 escolhe quem vai (followers que tem, fora o ativo e os que já estão fora);
    // as outras são as vagas: livre (botões das missões) ou ocupada (tempo restante e Coletar)
    public class JanelaDasExpedicoes : JanelaDoJogo
    {
        protected override string Titulo => "Expedições";
        protected override Key Tecla => Key.E;
        protected override IconeDoHud Icone => IconeDoHud.Expedicoes;
        protected override SistemaDoJogo? Sistema => SistemaDoJogo.Expedicoes;
        protected override int Ordem => 9;
        protected override int Linhas => catalogo.VagasDeExpedicao + 1;
        protected override float AlturaDaLinha => 96;

        ListarExpedicoes listar;
        IniciarExpedicao iniciar;
        ColetarExpedicao coletar;
        ListarFollowers listarFollowers;

        Text[] textos;
        Button[] coletarBotoes;
        Button[][] missaoBotoes;
        Text escolhido;
        readonly List<DefinicaoDeFollower> disponiveis = new List<DefinicaoDeFollower>();
        int indiceEscolhido;
        List<SituacaoDaExpedicao> expedicoes;

        [Inject]
        public void Construir(ListarExpedicoes listar, IniciarExpedicao iniciar, ColetarExpedicao coletar, ListarFollowers listarFollowers)
        {
            this.listar = listar;
            this.iniciar = iniciar;
            this.coletar = coletar;
            this.listarFollowers = listarFollowers;
        }

        protected override void Start()
        {
            int n = catalogo.VagasDeExpedicao + 1;
            textos = new Text[n]; coletarBotoes = new Button[n]; missaoBotoes = new Button[n][];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            if (i == 0)
            {
                escolhido = Texto(linha, 24, 20, 24, KitDeInterface.Dourado, 640, true);
                Botao(linha, "Trocar follower", 30, 260, () => { indiceEscolhido++; AtualizarConteudo(); });
                return;
            }
            textos[i] = Texto(linha, 24, 10, 22, KitDeInterface.TextoClaro, 600);
            int vaga = i - 1;
            coletarBotoes[i] = Botao(linha, "Coletar", 30, 220, () => Coletar(vaga));
            var missoes = catalogo.Missoes;
            missaoBotoes[i] = new Button[missoes.Count];
            for (int m = 0; m < missoes.Count; m++)
            {
                string id = missoes[m].Id;
                missaoBotoes[i][m] = Botao(linha, $"{missoes[m].Nome} ({missoes[m].Horas:0} h)", 30 + m * 210, 200, () => Mandar(id));
                ((RectTransform)missaoBotoes[i][m].transform).anchoredPosition = new Vector2(-(30 + m * 210), -36);
            }
        }

        void Mandar(string missaoId)
        {
            if (disponiveis.Count == 0) return;
            var f = disponiveis[indiceEscolhido % disponiveis.Count];
            if (iniciar.Executar(missaoId, f.Id)) faixa.Mostrar($"{f.Nome} partiu em expedição!", 1.5f);
            AtualizarConteudo();
        }

        void Coletar(int vaga)
        {
            var r = coletar.Executar(vaga);
            if (r == null) return;
            string texto = $"{r.Value.Follower} voltou: +{r.Value.Cristais} cristais";
            foreach (var m in r.Value.Materiais) texto += $"  +{m.Valor} {catalogo.NomeDoMaterial(m.Id)}";
            if (r.Value.Itens > 0) texto += $"  +{r.Value.Itens} item(ns)";
            faixa.Mostrar(texto, 2.5f);
            AtualizarConteudo();
        }

        protected override void AtualizarConteudo()
        {
            expedicoes = listar.Executar();
            var p = Progresso;
            disponiveis.Clear();
            foreach (var s in listarFollowers.Executar())
                if (s.Estrelas > 0 && !s.Ativo && !p.EmExpedicao(s.Definicao.Id)) disponiveis.Add(s.Definicao);
            // os mais raros primeiro: trazem mais
            disponiveis.Sort((a, b) => b.Raridade.CompareTo(a.Raridade));
            escolhido.text = disponiveis.Count == 0
                ? "Nenhum follower livre (o ativo luta com você)"
                : $"Mandar: {disponiveis[indiceEscolhido % disponiveis.Count].Nome} ({RegrasDosItens.Nome(disponiveis[indiceEscolhido % disponiveis.Count].Raridade)})";
            base.AtualizarConteudo();
        }

        protected override void AtualizarLinha(int i)
        {
            if (i == 0) return;
            int vaga = i - 1;
            bool ocupada = vaga < expedicoes.Count;
            coletarBotoes[i].gameObject.SetActive(ocupada);
            foreach (var b in missaoBotoes[i])
            {
                b.gameObject.SetActive(!ocupada);
                b.interactable = disponiveis.Count > 0;
            }
            if (!ocupada)
            {
                textos[i].text = $"Vaga {i}: livre";
                return;
            }
            var e = expedicoes[vaga];
            int min = (int)System.Math.Ceiling(e.SegundosRestantes / 60);
            textos[i].text = e.Pronta
                ? $"{e.Follower?.Nome} voltou de {e.Missao?.Nome}!"
                : $"{e.Follower?.Nome} em {e.Missao?.Nome}: volta em {min / 60} h {min % 60:00} min";
            coletarBotoes[i].interactable = e.Pronta;
        }
    }
}
