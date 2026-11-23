using System.IO;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ALotOfDamage.Testes
{
    public class RepositorioEmArquivoTestes
    {
        string caminho;

        [SetUp]
        public void Criar() => caminho = Path.Combine(Path.GetTempPath(), $"alod-teste-{System.Guid.NewGuid():N}", "progresso.json");

        [TearDown]
        public void Apagar()
        {
            var pasta = Path.GetDirectoryName(caminho);
            if (Directory.Exists(pasta)) Directory.Delete(pasta, true);
        }

        [Test]
        public void Sem_arquivo_comeca_do_zero()
        {
            var p = new RepositorioDeProgressoEmArquivo(caminho).Carregar();
            Assert.AreEqual(0, p.Ouro, 1e-9);
            Assert.AreEqual(1, p.Estagio);
        }

        [Test]
        public void O_que_foi_salvo_volta_ao_abrir_o_jogo_de_novo()
        {
            var primeiro = new RepositorioDeProgressoEmArquivo(caminho);
            var p = primeiro.Carregar();
            p.ReceberOuro(42);
            p.AdicionarMaterial("gosma", 3);
            primeiro.Salvar(p);

            var reaberto = new RepositorioDeProgressoEmArquivo(caminho).Carregar();
            Assert.AreEqual(42, reaberto.Ouro, 1e-9);
            Assert.AreEqual(3, reaberto.Material("gosma"));
        }

        [Test]
        public void Arquivo_corrompido_nao_quebra_o_jogo()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));
            File.WriteAllText(caminho, "{ isso não é json");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Save ilegível"));

            var p = new RepositorioDeProgressoEmArquivo(caminho).Carregar();
            Assert.AreEqual(0, p.Ouro, 1e-9);
        }
    }
}
