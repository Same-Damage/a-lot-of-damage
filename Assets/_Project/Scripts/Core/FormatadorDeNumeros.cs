using System;
using System.Globalization;

namespace ALotOfDamage.Core
{
    // Formata números gigantes: 845.320 / 12,4 M / 3,51 T / 4,7 aa / 1 aaa
    public static class FormatadorDeNumeros
    {
        static readonly string[] Curtos = { "", "K", "M", "B", "T" };
        static readonly CultureInfo Br = new CultureInfo("pt-BR");

        public static bool NotacaoCientifica;

        public static string Formatar(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor)) return "∞";
            if (valor < 0) return "-" + Formatar(-valor);
            if (valor < 1_000_000) return Math.Floor(valor).ToString("#,0", Br);
            if (NotacaoCientifica) return valor.ToString("0.##e0", Br);

            int grupo = (int)Math.Floor(Math.Log10(valor) / 3);
            double escalado = valor / Math.Pow(1000, grupo);

            // Corrige erro de ponto flutuante e evita "1000 M" por arredondamento
            if (escalado < 1) { escalado *= 1000; grupo--; }
            if (escalado >= 999.995) { escalado /= 1000; grupo++; }

            string numero = escalado >= 100 ? escalado.ToString("0", Br)
                          : escalado >= 10 ? escalado.ToString("0.#", Br)
                          : escalado.ToString("0.##", Br);

            return numero + " " + Sufixo(grupo);
        }

        public static string Sufixo(int grupo)
        {
            if (grupo < Curtos.Length) return Curtos[grupo];

            // Depois de T: aa, ab ... zz, aaa ...
            int n = grupo - Curtos.Length;
            int letras = 2;
            int bloco = 26 * 26;
            while (n >= bloco) { n -= bloco; letras++; bloco *= 26; }

            var chars = new char[letras];
            for (int i = letras - 1; i >= 0; i--) { chars[i] = (char)('a' + n % 26); n /= 26; }
            return new string(chars);
        }
    }
}
