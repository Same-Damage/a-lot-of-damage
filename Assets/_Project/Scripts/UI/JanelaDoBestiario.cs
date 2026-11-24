using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ALotOfDamage.UI
{
    public class JanelaDoBestiario : JanelaDoJogo
    {
        protected override string Titulo => "Bestiário";
        protected override Key Tecla => Key.B;
        protected override int Ordem => 5;
        protected override IconeDoHud Icone => IconeDoHud.Bestiario;
        protected override TipoDePendencia? Pendencia => TipoDePendencia.Bestiario;
        // Linha 0 é o bônus geral; as outras, um monstro cada
        protected override int Linhas => catalogo.Monstros.Count + 1;
        protected override float AlturaDaLinha => 90;

        Text[] textos, detalhes;
        Barra[] barras;

        protected override void Start()
        {
            int n = catalogo.Monstros.Count + 1;
            textos = new Text[n]; detalhes = new Text[n]; barras = new Barra[n];
            base.Start();
        }

        protected override void MontarLinha(int i, RectTransform linha)
        {
            textos[i] = Texto(linha, 24, 10, 26, i == 0 ? KitDeInterface.Verde : KitDeInterface.Dourado, 900, true);
            if (i == 0) return;
            barras[i] = Barra(linha, 24, 52, 360, KitDeInterface.Azul);
            detalhes[i] = Texto(linha, 400, 46, 20, KitDeInterface.TextoClaro, 560);
        }

        protected override void AtualizarLinha(int i)
        {
            var p = catalogo.Bestiario;
            if (i == 0)
            {
                textos[0].text = $"Bônus geral de dano: ×{RegrasDoBestiario.MultiplicadorGeral(p, Progresso.TodasAsDerrotas):0.##}";
                return;
            }

            var m = catalogo.Monstros[i - 1];
            int derrotas = Progresso.Derrotas(m.Id);
            long proximo = RegrasDoBestiario.ProximoMarco(p, derrotas);
            long anterior = derrotas < p.PrimeiroMarco ? 0 : proximo / p.FatorEntreMarcos;
            textos[i].text = $"{m.Nome}   {derrotas} derrotados   contra ele: ×{RegrasDoBestiario.MultiplicadorContraTipo(p, derrotas):0.##}";
            barras[i].Definir((float)(derrotas - anterior) / (proximo - anterior));
            detalhes[i].text = $"próximo marco: {proximo}   drop: {m.NomeDoMaterial}";
        }
    }
}
