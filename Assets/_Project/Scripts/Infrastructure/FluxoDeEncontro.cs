using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALotOfDamage.Infrastructure
{
    // Leva o encontro do mapa para a arena e de volta. Vive no EscopoDoJogo, então sobrevive à troca de cena.
    public class FluxoDeEncontro
    {
        public const string CenaDoMapa = "Mapa";
        public const string CenaDaBatalha = "Batalha";
        const float SegundosDeProtecao = 2.5f;
        public const float SegundosParaRenascer = 30f;

        public bool Ativo { get; private set; }
        public string IdDoInimigo { get; private set; }
        public string TipoDoInimigo { get; private set; }
        public string NomeDoInimigo { get; private set; }
        public double MultiplicadorDeVida { get; private set; } = 1;
        public Vantagem Vantagem { get; private set; }

        public bool TemPosicaoDeRetorno { get; private set; }
        public Vector3 PosicaoDeRetorno { get; private set; }

        float protecaoAte;
        public bool EmProtecao => Time.realtimeSinceStartup < protecaoAte;

        // Id do monstro no mapa → momento da derrota (tempo real)
        readonly Dictionary<string, float> derrotados = new Dictionary<string, float>();

        public bool EstaDerrotado(string idDoInimigo) =>
            derrotados.TryGetValue(idDoInimigo, out var momento) &&
            !RegrasDoEncontro.Renasceu(momento, Time.realtimeSinceStartup, SegundosParaRenascer);

        public void Iniciar(string idDoInimigo, string tipo, string nome, double multiplicadorDeVida, Vantagem vantagem, Vector3 posicaoDoJogador)
        {
            if (Ativo) return;
            Ativo = true;
            IdDoInimigo = idDoInimigo;
            TipoDoInimigo = tipo;
            NomeDoInimigo = nome;
            MultiplicadorDeVida = multiplicadorDeVida;
            Vantagem = vantagem;
            PosicaoDeRetorno = posicaoDoJogador;
            TemPosicaoDeRetorno = true;
            SceneManager.LoadScene(CenaDaBatalha);
        }

        public void Encerrar(bool venceu)
        {
            if (venceu && IdDoInimigo != null) derrotados[IdDoInimigo] = Time.realtimeSinceStartup;
            Ativo = false;
            protecaoAte = Time.realtimeSinceStartup + SegundosDeProtecao;
            SceneManager.LoadScene(CenaDoMapa);
        }
    }
}
