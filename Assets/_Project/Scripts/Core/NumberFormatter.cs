using System;
using System.Globalization;

namespace ALotOfDamage.Core
{
    // Formata números gigantes: 845.320 / 12,4 K / 3,51 T / 4,7 aa / 1,3 aaa
    public static class NumberFormatter
    {
        static readonly string[] Short = { "", "K", "M", "B", "T" };
        static readonly CultureInfo Br = new CultureInfo("pt-BR");

        public static bool Scientific;

        public static string Format(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "∞";
            if (value < 0) return "-" + Format(-value);
            if (value < 1_000_000) return Math.Floor(value).ToString("#,0", Br);
            if (Scientific) return value.ToString("0.##e0", Br);

            int group = (int)Math.Floor(Math.Log10(value) / 3);
            double scaled = value / Math.Pow(1000, group);

            // Evita mostrar "1000 K" por arredondamento
            if (scaled >= 999.995) { scaled /= 1000; group++; }

            string number = scaled >= 100 ? scaled.ToString("0", Br)
                          : scaled >= 10 ? scaled.ToString("0.#", Br)
                          : scaled.ToString("0.##", Br);

            return number + " " + Suffix(group);
        }

        static string Suffix(int group)
        {
            if (group < Short.Length) return Short[group];

            // Depois de T: aa, ab ... zz, aaa ...
            int n = group - Short.Length;
            int letters = 2;
            int block = 26 * 26;
            while (n >= block) { n -= block; letters++; block *= 26; }

            var chars = new char[letters];
            for (int i = letters - 1; i >= 0; i--) { chars[i] = (char)('a' + n % 26); n /= 26; }
            return new string(chars);
        }
    }
}
