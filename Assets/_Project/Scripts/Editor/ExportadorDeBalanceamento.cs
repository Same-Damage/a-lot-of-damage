using System.Globalization;
using System.IO;
using System.Text;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace ALotOfDamage.Ferramentas
{
    // Gera docs/balanceamento.csv com as mesmas fórmulas do jogo, para ver a curva numa planilha
    public static class ExportadorDeBalanceamento
    {
        const int NivelMaximo = 200;

        [MenuItem("A Lot of Damage/Exportar planilha de balanceamento")]
        public static void Exportar()
        {
            var br = new CultureInfo("pt-BR");
            var upgrades = BalanceamentoDeUpgrades.Carregar().upgrades;
            var sb = new StringBuilder();

            sb.Append("nivel;marcos");
            foreach (var d in upgrades)
                sb.Append($";{d.Id}_valor;{d.Id}_custo_proximo;{d.Id}_custo_acumulado;{d.Id}_ganho_proximo_%");
            sb.AppendLine();

            for (int n = 1; n <= NivelMaximo; n++)
            {
                sb.Append(n).Append(';').Append(RegrasDeUpgrade.MultiplicadorDeMarcos(n).ToString("G6", br));
                foreach (var d in upgrades)
                {
                    double acumulado = n == 1 ? 0 : RegrasDeUpgrade.CustoDeVarios(d, 1, n - 1);
                    sb.Append(';').Append(RegrasDeUpgrade.Valor(d, n).ToString("G6", br));
                    sb.Append(';').Append(RegrasDeUpgrade.Custo(d, n).ToString("G6", br));
                    sb.Append(';').Append(acumulado.ToString("G6", br));
                    sb.Append(';').Append((RegrasDeUpgrade.GanhoRelativo(d, n) * 100).ToString("0.##", br));
                }
                sb.AppendLine();
            }

            string caminho = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "docs", "balanceamento.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));
            // BOM para o Excel abrir os acentos corretamente
            File.WriteAllText(caminho, sb.ToString(), new UTF8Encoding(true));
            Debug.Log($"Planilha exportada em {caminho}");
        }

        // Usado em linha de comando: recria as cenas e exporta a planilha
        public static void CriarTudo()
        {
            ConstrutorDeCenas.CriarTodas();
            Exportar();
        }
    }
}
