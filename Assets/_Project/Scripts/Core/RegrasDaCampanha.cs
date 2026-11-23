using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    [Serializable]
    public class ParametrosDaCampanha
    {
        public double VidaBase = 100;
        public double CrescimentoPorEstagio = 1.25;
        public double OuroPorVida = 0.5;
        public int VitoriasParaAvancar = 10;
        public int ChefeACada = 5;
        public double MultiplicadorDeVidaDoChefe = 10;
        public int EstagiosPorRegiao = 10;
    }

    [Serializable]
    public class DefinicaoDeRegiao
    {
        public string Nome;
        public List<string> Monstros = new List<string>();   // ids de DefinicaoDeMonstro que aparecem no mapa
        public string CorDoChao = "#598C4C";
    }

    public struct ProgressoDaCampanha
    {
        public int Estagio;
        public int VitoriasNoEstagio;
        public bool Avancou;
    }

    // Campanha infinita: estágios com vida crescente, chefe periódico e regiões que mudam o mapa
    public static class RegrasDaCampanha
    {
        public static double VidaDoInimigo(ParametrosDaCampanha p, int estagio, double multiplicadorDoTipo, bool chefe) =>
            p.VidaBase * Math.Pow(p.CrescimentoPorEstagio, estagio - 1) * multiplicadorDoTipo
            * (chefe ? p.MultiplicadorDeVidaDoChefe : 1);

        public static double Ouro(ParametrosDaCampanha p, double vida) => vida * p.OuroPorVida;

        public static bool EhEstagioDeChefe(ParametrosDaCampanha p, int estagio) =>
            p.ChefeACada > 0 && estagio % p.ChefeACada == 0;

        public static DefinicaoDeRegiao Regiao(ParametrosDaCampanha p, IReadOnlyList<DefinicaoDeRegiao> regioes, int estagio)
        {
            if (regioes == null || regioes.Count == 0) return null;
            int indice = (estagio - 1) / Math.Max(1, p.EstagiosPorRegiao);
            return regioes[indice % regioes.Count];
        }

        // Qual monstro ocupa cada ponto do mapa: os pontos se revezam entre os monstros da região;
        // o chefe é sempre o último (o mais forte) da lista
        public static string MonstroDoPonto(DefinicaoDeRegiao regiao, int indiceDoPonto, bool chefe)
        {
            if (regiao == null || regiao.Monstros.Count == 0) return null;
            return chefe ? regiao.Monstros[regiao.Monstros.Count - 1] : regiao.Monstros[indiceDoPonto % regiao.Monstros.Count];
        }

        // Estágio normal: N vitórias avançam. Estágio de chefe: só o chefe avança; as outras lutas só dão recompensa.
        public static ProgressoDaCampanha AposVitoria(ParametrosDaCampanha p, int estagio, int vitoriasNoEstagio, bool chefe)
        {
            bool deChefe = EhEstagioDeChefe(p, estagio);
            if (deChefe ? chefe : vitoriasNoEstagio + 1 >= p.VitoriasParaAvancar)
                return new ProgressoDaCampanha { Estagio = estagio + 1, VitoriasNoEstagio = 0, Avancou = true };

            return new ProgressoDaCampanha
            {
                Estagio = estagio,
                VitoriasNoEstagio = Math.Min(vitoriasNoEstagio + 1, p.VitoriasParaAvancar),
            };
        }
    }
}
