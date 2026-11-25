using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALotOfDamage.Infrastructure
{
    // Leva o encontro do mapa para a arena e de volta. Vive no EscopoDoJogo, então sobrevive à troca de cena.
    public class FluxoDeEncontro : IEntradaNaArena, IPortaoDaCidade
    {
        public const string CenaDoMapa = "Mapa";
        public const string CenaDaBatalha = "Batalha";
        public const string CenaDaCidade = "Cidade";
        const float SegundosDeProtecao = 2.5f;
        public const float SegundosParaRenascer = 30f;

        public bool Ativo { get; private set; }
        public string IdDoInimigo { get; private set; }
        public string TipoDoInimigo { get; private set; }
        public bool Chefe { get; private set; }
        public Vantagem Vantagem { get; private set; }

        // Luta contra o chefe da guilda; null numa luta comum
        public DesafioDaGuilda Guilda { get; private set; }

        public bool TemPosicaoDeRetorno { get; private set; }
        public Vector3 PosicaoDeRetorno { get; private set; }

        float protecaoAte;
        public bool EmProtecao => Time.realtimeSinceStartup < protecaoAte;

        // Id do monstro no mapa → momento da derrota (tempo real)
        readonly Dictionary<string, float> derrotados = new Dictionary<string, float>();

        public bool EstaDerrotado(string idDoInimigo) =>
            derrotados.TryGetValue(idDoInimigo, out var momento) &&
            !RegrasDoEncontro.Renasceu(momento, Time.realtimeSinceStartup, SegundosParaRenascer);

        public void Iniciar(string idDoInimigo, string tipo, bool chefe, Vantagem vantagem, Vector3 posicaoDoJogador)
        {
            if (Ativo) return;
            Ativo = true;
            IdDoInimigo = idDoInimigo;
            TipoDoInimigo = tipo;
            Chefe = chefe;
            Vantagem = vantagem;
            PosicaoDeRetorno = posicaoDoJogador;
            TemPosicaoDeRetorno = true;
            SceneManager.LoadScene(CenaDaBatalha);
        }

        public void LutarPelaGuilda(DesafioDaGuilda desafio)
        {
            if (Ativo) return;
            Guilda = desafio;
            Iniciar(null, null, true, Vantagem.Normal, PosicaoDoJogador());
        }

        Vector3 PosicaoDoJogador()
        {
            var jogador = GameObject.Find("Jogador");
            return jogador ? jogador.transform.position : PosicaoDeRetorno;
        }

        // Ida e volta da cidade, guardando o ponto do mapa
        public void IrParaCidade()
        {
            if (Ativo || SceneManager.GetActiveScene().name != CenaDoMapa) return;
            PosicaoDeRetorno = PosicaoDoJogador();
            TemPosicaoDeRetorno = true;
            SceneManager.LoadScene(CenaDaCidade);
        }

        public void VoltarAoMapa()
        {
            protecaoAte = Time.realtimeSinceStartup + SegundosDeProtecao;
            SceneManager.LoadScene(CenaDoMapa);
        }

        public void Encerrar(bool venceu)
        {
            if (venceu && IdDoInimigo != null) derrotados[IdDoInimigo] = Time.realtimeSinceStartup;
            Ativo = false;
            Guilda = null;
            protecaoAte = Time.realtimeSinceStartup + SegundosDeProtecao;
            SceneManager.LoadScene(CenaDoMapa);
        }
    }
}
