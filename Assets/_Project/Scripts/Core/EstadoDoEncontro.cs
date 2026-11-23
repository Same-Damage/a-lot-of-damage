using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALotOfDamage.Core
{
    // Dados que passam do mapa para a batalha e voltam
    public static class EstadoDoEncontro
    {
        public const string CenaDoMapa = "Mapa";
        public const string CenaDaBatalha = "Batalha";
        const float SegundosDeProtecao = 2.5f;

        public static bool Ativo { get; private set; }
        public static string IdDoInimigo { get; private set; }
        public static string NomeDoInimigo { get; private set; }
        public static double MultiplicadorDeVida { get; private set; } = 1;
        public static Vantagem Vantagem { get; private set; }

        public static bool TemPosicaoDeRetorno { get; private set; }
        public static Vector3 PosicaoDeRetorno { get; private set; }

        static float protecaoAte;
        public static bool EmProtecao => Time.realtimeSinceStartup < protecaoAte;

        public static string TipoDoInimigo { get; private set; }

        public const float SegundosParaRenascer = 30f;

        // Id do monstro no mapa → momento da derrota (tempo real)
        static readonly Dictionary<string, float> derrotados = new Dictionary<string, float>();

        public static bool EstaDerrotado(string idDoInimigo) =>
            derrotados.TryGetValue(idDoInimigo, out var momento) &&
            !RegrasDoEncontro.Renasceu(momento, Time.realtimeSinceStartup, SegundosParaRenascer);

        public static void Iniciar(string idDoInimigo, string tipo, string nome, double multiplicadorDeVida, Vantagem vantagem, Vector3 posicaoDoJogador)
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

        public static void Encerrar(bool venceu)
        {
            if (venceu && IdDoInimigo != null) derrotados[IdDoInimigo] = Time.realtimeSinceStartup;
            Ativo = false;
            protecaoAte = Time.realtimeSinceStartup + SegundosDeProtecao;
            SceneManager.LoadScene(CenaDoMapa);
        }
    }
}
