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

        public static readonly HashSet<string> Derrotados = new HashSet<string>();

        public static void Iniciar(string idDoInimigo, string nome, double multiplicadorDeVida, Vantagem vantagem, Vector3 posicaoDoJogador)
        {
            if (Ativo) return;
            Ativo = true;
            IdDoInimigo = idDoInimigo;
            NomeDoInimigo = nome;
            MultiplicadorDeVida = multiplicadorDeVida;
            Vantagem = vantagem;
            PosicaoDeRetorno = posicaoDoJogador;
            TemPosicaoDeRetorno = true;
            SceneManager.LoadScene(CenaDaBatalha);
        }

        public static void Encerrar(bool venceu)
        {
            if (venceu && IdDoInimigo != null) Derrotados.Add(IdDoInimigo);
            Ativo = false;
            protecaoAte = Time.realtimeSinceStartup + SegundosDeProtecao;
            SceneManager.LoadScene(CenaDoMapa);
        }
    }
}
