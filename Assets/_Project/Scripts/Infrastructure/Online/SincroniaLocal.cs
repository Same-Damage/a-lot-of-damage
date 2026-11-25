using System;
using System.IO;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    public class SerializadorDeProgresso : ISerializadorDeProgresso
    {
        public string Escrever(DadosDoProgresso dados) => JsonUtility.ToJson(dados);

        public DadosDoProgresso Ler(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return null;
            try { return JsonUtility.FromJson<DadosDoProgresso>(texto); }
            catch (ArgumentException) { return null; }
        }
    }

    // Arquivos ao lado do save: <save>-sincronia.json e <save>-backup-nuvem.json (o último lado que perdeu um conflito)
    public class GuardaDaSincroniaEmArquivo : IGuardaDaSincronia
    {
        readonly string estado, backup;

        public GuardaDaSincroniaEmArquivo(string caminhoDoSave, string dispositivo)
        {
            string raiz = Path.ChangeExtension(caminhoDoSave, null);
            estado = raiz + "-sincronia.json";
            backup = raiz + "-backup-nuvem.json";
            Dispositivo = dispositivo;
        }

        public string Dispositivo { get; }

        public EstadoDaSincronia Ler()
        {
            try { return File.Exists(estado) ? JsonUtility.FromJson<EstadoDaSincronia>(File.ReadAllText(estado)) : null; }
            catch (Exception e) when (e is ArgumentException || e is IOException) { return null; }   // sem estado, a próxima comparação pergunta
        }

        public void Gravar(EstadoDaSincronia e) => Escrever(estado, JsonUtility.ToJson(e));
        public void GuardarBackup(string progresso) => Escrever(backup, progresso);

        static void Escrever(string caminho, string texto)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));
            File.WriteAllText(caminho, texto);
        }
    }

    // Sem projeto da Unity configurado (e nos testes): o jogo fica só offline
    public class OnlineDesligado : IContaOnline, INuvemDeProgresso, IRanking
    {
        static Task<T> Nao<T>() => Task.FromException<T>(new FalhaOnline("O online não está configurado nesta versão."));

        public bool Disponivel => false;
        public bool Conectada => false;
        public string JogadorId => "";
        public string Apelido => "";
        public bool Vinculada => false;
        public Task Entrar() => Nao<bool>();
        public Task Vincular(string usuario, string senha) => Nao<bool>();
        public Task EntrarComConta(string usuario, string senha) => Nao<bool>();
        public Task TrocarApelido(string apelido) => Nao<bool>();
        Task IContaOnline.Apagar() => Nao<bool>();
        public Task<SaveNaNuvem> Carregar() => Nao<SaveNaNuvem>();
        public Task<RespostaDoEnvio> Enviar(PedidoDeEnvio pedido) => Nao<RespostaDoEnvio>();
        Task INuvemDeProgresso.Apagar() => Nao<bool>();
        public Task<PaginaDoRanking> Buscar(TipoDeRanking tipo, int quantos) => Nao<PaginaDoRanking>();
    }
}
