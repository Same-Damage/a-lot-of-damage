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

        public static string CaminhoDoCatalogo => Path.Combine(Application.dataPath, "_Project", "Server~", "Nuvem", "catalogo.json");
        public static string CaminhoDaProva => Path.Combine(Application.dataPath, "_Project", "Server~", "Nuvem.Testes", "prova.json");

        public static string TextoDoCatalogo() => JsonUtility.ToJson(CatalogoEmDados.De(new CatalogoDeBalanceamento()), true);

        // Um progresso de exemplo e o Poder que o jogo calcula: o servidor precisa chegar ao mesmo número
        [System.Serializable]
        public class Prova { public string Progresso; public double Poder; }

        public static string TextoDaProva()
        {
            var catalogo = new CatalogoDeBalanceamento();
            var p = new Progresso();
            p.ReceberOuro(1e9);
            foreach (var d in catalogo.Upgrades) p.Comprar(d, 25);
            for (int i = 0; i < 40; i++) p.RegistrarVitoria(catalogo.Monstros[i % catalogo.Monstros.Count], catalogo.Bestiario);
            var tipo = catalogo.Itens[0];
            p.Equipar(p.ReceberItem(tipo.Id, tipo.Slot, Raridade.Epico, 12, 99).Id);
            p.ReceberFragmentos(1000);
            foreach (var r in catalogo.Reliquias) p.MelhorarReliquia(r.Id, 3);
            p.Ascender(4);
            p.ReceberOuro(1e9);
            foreach (var d in catalogo.Upgrades) p.Comprar(d, 10);
            foreach (var d in catalogo.Predios) p.MelhorarPredio(d.Id, 10);
            return JsonUtility.ToJson(new Prova { Progresso = JsonUtility.ToJson(p.Exportar()), Poder = CalcularPoder.De(p.Exportar(), catalogo) }, true);
        }

        // O servidor (Server~) usa o mesmo balanceamento: gere de novo e publique o módulo ao mudar valores
        [MenuItem("A Lot of Damage/Exportar catálogo do servidor")]
        public static void ExportarCatalogo()
        {
            Gravar(CaminhoDoCatalogo, TextoDoCatalogo());
            Gravar(CaminhoDaProva, TextoDaProva());
        }

        static void Gravar(string caminho, string texto)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));
            File.WriteAllText(caminho, texto, new UTF8Encoding(false));
        }

        // Usado em linha de comando: recria as cenas e exporta a planilha
        public static void CriarTudo()
        {
            ConstrutorDeCenas.CriarTodas();
            Exportar();
            ExportarCatalogo();
        }
    }
}
