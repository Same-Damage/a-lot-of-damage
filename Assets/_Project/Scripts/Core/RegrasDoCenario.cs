using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    [Serializable]
    public class ParametrosDaIlha
    {
        public float RaioAndavel = 46;        // área plana onde se anda e luta
        public float RaioDaBorda = 58;        // topo dos morros; daqui para fora a praia desce até o mar
        public float RaioDaPraia = 72;        // onde a areia entra na água
        public float NivelDoMar = -1.5f;
        public float RaioLivreNoInicio = 7;   // em volta do ponto inicial do jogador
        public float DistanciaMinima = 2.4f;  // entre obstáculos, para sobrar caminho
        public float AlturaDosMorros = 4.5f;
    }

    // Como a região se parece: cores (em hexadecimal), luz, névoa, quantidade de cada objeto e partículas
    [Serializable]
    public class VisualDaRegiao
    {
        public string CorDaGrama = "#6FA34F";
        public string CorDaTerra = "#9C7A4E";
        public string CorDaRocha = "#8A7E6C";
        public string CorDaFolhagem = "#4F8A3C";
        public string CorDoTronco = "#6B4A2E";
        public string CorDoCristal = "#5FD8FF";
        public string CorDoSol = "#FFE9C4";
        public string CorDoCeu = "#9CC8F0";
        public string CorDaNevoa = "#C9DDEB";
        public string CorDoMar = "#3E8DB5";
        public string CorDaAreia = "#DCC795";
        public float IntensidadeDoSol = 1.25f;
        public float DensidadeDaNevoa = 0.003f;
        public int Arvores = 70;
        public int Pinheiros = 30;
        public int Pedras = 35;
        public int Cristais = 8;
        public int Ruinas = 4;
        public int Arbustos = 80;
        public int Grama = 900;
        public TipoDeParticula Particulas = TipoDeParticula.Folhas;
    }

    public enum TipoDeParticula { Nenhuma, Folhas, Vagalumes, Poeira }

    public enum TipoDeItemDoCenario { Ruina, Arvore, Pinheiro, Pedra, Cristal, Arbusto, Grama }

    public struct ItemDoCenario
    {
        public TipoDeItemDoCenario Tipo;
        public float X, Y, Z, Escala, Rotacao;
    }

    // Lugar que precisa ficar vazio (ponto de monstro, entrada de cena)
    public readonly struct AreaLivre
    {
        public readonly float X, Z, Raio;
        public AreaLivre(float x, float z, float raio) { X = x; Z = z; Raio = raio; }
    }

    // Onde cada objeto do cenário nasce e o relevo da ilha. Tudo depende só da semente,
    // então a mesma região monta sempre o mesmo mapa (e o servidor pode reproduzir).
    public static class RegrasDoCenario
    {
        const int Tentativas = 40;

        public static bool Bloqueia(TipoDeItemDoCenario tipo) =>
            tipo != TipoDeItemDoCenario.Grama && tipo != TipoDeItemDoCenario.Arbusto;

        // Ilha de terra no mar: plano na área andável, morros na borda com altura variando pela volta da ilha,
        // depois a praia descendo até entrar na água e o fundo do mar afundando devagar
        public static float Altura(ParametrosDaIlha p, float x, float z, int semente)
        {
            float r = (float)Math.Sqrt(x * x + z * z);
            if (r <= p.RaioAndavel) return 0f;
            double ang = Math.Atan2(z, x);
            var fase = new Random(semente);
            double f1 = fase.NextDouble() * Math.PI * 2, f2 = fase.NextDouble() * Math.PI * 2;
            float onda = (float)(0.6 + 0.3 * Math.Sin(3 * ang + f1) + 0.2 * Math.Sin(7 * ang + f2));
            float morro = p.AlturaDosMorros * Math.Max(0.15f, onda);
            if (r <= p.RaioDaBorda) return Suave((r - p.RaioAndavel) / (p.RaioDaBorda - p.RaioAndavel)) * morro;
            float naAgua = p.NivelDoMar - 1f;
            if (r <= p.RaioDaPraia)
            {
                float s = Suave((r - p.RaioDaBorda) / (p.RaioDaPraia - p.RaioDaBorda));
                return morro + (naAgua - morro) * s;
            }
            return naAgua - (r - p.RaioDaPraia) * 0.3f;
        }

        static float Suave(float t) => t * t * (3 - 2 * t);

        public static List<ItemDoCenario> Espalhar(ParametrosDaIlha p, VisualDaRegiao v, int semente, IReadOnlyList<AreaLivre> livres)
        {
            var sorteio = new Random(semente);
            var itens = new List<ItemDoCenario>();
            var obstaculos = new List<ItemDoCenario>();

            // Os grandes primeiro, para os pequenos preencherem os vãos
            Colocar(TipoDeItemDoCenario.Ruina, v.Ruinas, 0f);
            Colocar(TipoDeItemDoCenario.Arvore, v.Arvores, 0.45f);
            Colocar(TipoDeItemDoCenario.Pinheiro, v.Pinheiros, 0.45f);
            Colocar(TipoDeItemDoCenario.Pedra, v.Pedras, 0.3f);
            Colocar(TipoDeItemDoCenario.Cristal, v.Cristais, 0f);
            Colocar(TipoDeItemDoCenario.Arbusto, v.Arbustos, 0.2f);
            Colocar(TipoDeItemDoCenario.Grama, v.Grama, 0f);
            return itens;

            void Colocar(TipoDeItemDoCenario tipo, int quantidade, float naBorda)
            {
                bool bloqueia = Bloqueia(tipo);
                for (int n = 0; n < quantidade; n++)
                for (int tentativa = 0; tentativa < Tentativas; tentativa++)
                {
                    bool borda = sorteio.NextDouble() < naBorda;
                    float rMin = borda ? p.RaioAndavel + 1 : p.RaioLivreNoInicio;
                    float rMax = borda ? p.RaioDaBorda - 2 : p.RaioAndavel - 1;
                    // raiz quadrada para a densidade ficar igual no disco todo
                    double u = sorteio.NextDouble();
                    float r = (float)Math.Sqrt(rMin * rMin + u * (rMax * rMax - rMin * rMin));
                    double ang = sorteio.NextDouble() * Math.PI * 2;
                    float x = (float)(Math.Cos(ang) * r), z = (float)(Math.Sin(ang) * r);
                    float escala = (float)(borda ? 1.1 + sorteio.NextDouble() * 0.4 : 0.75 + sorteio.NextDouble() * 0.6);
                    float rotacao = (float)(sorteio.NextDouble() * 360);

                    if (DentroDeAreaLivre(livres, x, z)) continue;
                    float folga = bloqueia ? p.DistanciaMinima : 0.9f;
                    if (Perto(obstaculos, x, z, folga)) continue;

                    var item = new ItemDoCenario { Tipo = tipo, X = x, Y = Altura(p, x, z, semente), Z = z, Escala = escala, Rotacao = rotacao };
                    itens.Add(item);
                    if (bloqueia) obstaculos.Add(item);
                    break;
                }
            }
        }

        static bool DentroDeAreaLivre(IReadOnlyList<AreaLivre> livres, float x, float z)
        {
            if (livres == null) return false;
            foreach (var a in livres)
                if ((x - a.X) * (x - a.X) + (z - a.Z) * (z - a.Z) < a.Raio * a.Raio) return true;
            return false;
        }

        static bool Perto(List<ItemDoCenario> outros, float x, float z, float distancia)
        {
            float d2 = distancia * distancia;
            foreach (var o in outros)
                if ((x - o.X) * (x - o.X) + (z - o.Z) * (z - o.Z) < d2) return true;
            return false;
        }
    }
}
