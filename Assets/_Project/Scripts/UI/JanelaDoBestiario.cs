using System.Linq;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ALotOfDamage.UI
{
    // Tela provisória do bestiário (OnGUI) até existir o kit de interface
    public class JanelaDoBestiario : JanelaProvisoria
    {
        const float AlturaItem = 70;

        protected override string Titulo => "Bestiário";
        protected override Key Tecla => Key.B;
        protected override int Ordem => 2;
        protected override float AlturaDoConteudo => (catalogo.Monstros.Count + 1) * AlturaItem;

        protected override void DesenharConteudo(Rect area)
        {
            var monstros = catalogo.Monstros;
            var p = catalogo.Bestiario;

            double geral = RegrasDoBestiario.MultiplicadorGeral(p, monstros.Select(m => Progresso.Derrotas(m.Id)));
            GUI.Label(new Rect(area.x + 16, area.y, 600, 24), $"Bônus geral de dano: x{geral:0.##}", EstiloTitulo);

            for (int i = 0; i < monstros.Count; i++)
            {
                var m = monstros[i];
                int derrotas = Progresso.Derrotas(m.Id);
                long proximo = RegrasDoBestiario.ProximoMarco(p, derrotas);
                float y = area.y + (i + 1) * AlturaItem;

                GUI.Label(new Rect(area.x + 16, y, 600, 24),
                    $"{m.Nome}   {derrotas} derrotados   contra ele: x{RegrasDoBestiario.MultiplicadorContraTipo(p, derrotas):0.##}", EstiloTitulo);
                long anterior = proximo / p.FatorEntreMarcos;
                if (derrotas < p.PrimeiroMarco) anterior = 0;
                Barra(new Rect(area.x + 16, y + 30, 300, 14), (float)(derrotas - anterior) / (proximo - anterior), new Color(0.4f, 0.8f, 1f));
                GUI.Label(new Rect(area.x + 324, y + 26, 400, 22), $"próximo marco: {proximo}   drop: {m.NomeDoMaterial}", EstiloTexto);
            }
        }
    }
}
