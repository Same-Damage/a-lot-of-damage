using System;

namespace ALotOfDamage.Core
{
    // Cidade online: o que vem dos outros jogadores é conferido antes de aparecer
    public static class RegrasDaCidade
    {
        public const int MaximoPorSala = 20;
        public const float Raio = 30f;                  // área onde se anda
        public const float EnviosPorSegundo = 5f;
        public const float DistanciaMinima = 0.15f;     // andou menos que isso, não envia
        public const float DistanciaDoSalto = 9f;       // atrasou mais que isso: aparece direto no lugar

        public static bool DeveEnviar(float distancia, float segundosDesdeOUltimo) =>
            distancia >= DistanciaMinima && segundosDesdeOUltimo >= 1f / EnviosPorSegundo;

        // Ponto válido dentro da cidade; false para números impossíveis
        public static bool Ponto(ref float x, ref float z)
        {
            if (float.IsNaN(x) || float.IsInfinity(x) || float.IsNaN(z) || float.IsInfinity(z)) return false;
            double d = Math.Sqrt((double)x * x + (double)z * z);
            if (d > Raio)
            {
                x = (float)(x / d * Raio);
                z = (float)(z / d * Raio);
            }
            return true;
        }

        public static string NomeParaMostrar(string nome)
        {
            string s = RegrasDaGuilda.NomeLimpo(nome);
            return s.Length <= 30 ? s : s.Substring(0, 30);
        }
    }
}
