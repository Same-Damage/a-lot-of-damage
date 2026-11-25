using System.IO;
using ALotOfDamage.Core;
using ALotOfDamage.Ferramentas;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;
using UnityEngine;

namespace ALotOfDamage.Testes
{
    public class CatalogoDoServidorTestes
    {
        static string Arquivo(string caminho) => File.ReadAllText(caminho).Replace("\r\n", "\n");

        [Test]
        public void Catalogo_do_servidor_esta_atualizado() => Assert.AreEqual(ExportadorDeBalanceamento.TextoDoCatalogo(), Arquivo(ExportadorDeBalanceamento.CaminhoDoCatalogo),
            "Balanceamento mudou: use o menu A Lot of Damage > Exportar catálogo do servidor e publique o módulo.");

        [Test]
        public void Catalogo_em_dados_da_o_mesmo_poder()
        {
            var prova = JsonUtility.FromJson<ExportadorDeBalanceamento.Prova>(Arquivo(ExportadorDeBalanceamento.CaminhoDaProva));
            var dados = JsonUtility.FromJson<DadosDoProgresso>(prova.Progresso);
            var lido = JsonUtility.FromJson<CatalogoEmDados>(ExportadorDeBalanceamento.TextoDoCatalogo());
            Assert.AreEqual(prova.Poder, CalcularPoder.De(dados, new CatalogoDeBalanceamento()), prova.Poder * 1e-9);
            Assert.AreEqual(prova.Poder, CalcularPoder.De(dados, lido), prova.Poder * 1e-9);
        }
    }
}
