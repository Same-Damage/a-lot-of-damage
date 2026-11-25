using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALotOfDamage.Core
{
    // Falha de rede ou do serviço; o jogo segue offline
    public class FalhaOnline : Exception
    {
        public FalhaOnline(string mensagem, Exception causa = null) : base(mensagem, causa) { }
    }

    // Conta do jogador. Os métodos lançam FalhaOnline, com uma mensagem que pode ser mostrada
    public interface IContaOnline
    {
        bool Disponivel { get; }        // há projeto configurado nesta build
        bool Conectada { get; }
        string JogadorId { get; }
        string Apelido { get; }
        bool Vinculada { get; }         // tem usuário e senha, então pode entrar em outro aparelho
        Task Entrar();                  // como convidado, ou na conta usada da última vez
        Task Vincular(string usuario, string senha);
        Task EntrarComConta(string usuario, string senha);
        Task TrocarApelido(string apelido);
        Task Apagar();
    }

    public interface INuvemDeProgresso
    {
        Task<SaveNaNuvem> Carregar();
        Task<RespostaDoEnvio> Enviar(PedidoDeEnvio pedido);
        Task Apagar();
    }

    public enum TipoDeRanking { Estagio, Poder }

    public class LinhaDoRanking
    {
        public int Posicao;
        public string JogadorId;
        public string Nome;
        public double Pontos;
    }

    public class PaginaDoRanking
    {
        public List<LinhaDoRanking> Primeiros = new List<LinhaDoRanking>();
        public LinhaDoRanking Minha;    // null se o jogador ainda não pontuou
    }

    public interface IRanking
    {
        Task<PaginaDoRanking> Buscar(TipoDeRanking tipo, int quantos);
    }

    // O que este aparelho sabe da nuvem, guardado junto do save local
    [Serializable]
    public class EstadoDaSincronia
    {
        public string JogadorId = "";
        public int Revisao;
        public string Impressao = "";
    }

    public interface IGuardaDaSincronia
    {
        string Dispositivo { get; }
        EstadoDaSincronia Ler();
        void Gravar(EstadoDaSincronia estado);
        void GuardarBackup(string progresso);   // o lado que perdeu um conflito
    }

    public interface ISerializadorDeProgresso
    {
        string Escrever(DadosDoProgresso dados);
        DadosDoProgresso Ler(string texto);     // null se o texto não for um progresso
    }
}
