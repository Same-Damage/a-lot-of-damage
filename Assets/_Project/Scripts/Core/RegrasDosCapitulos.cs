using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    public enum SistemaDoJogo
    {
        Upgrades, Forja, Inventario, Bestiario, Followers, Conquistas, Reliquias, Taverna, Expedicoes, Alquimia, Ascensao, Cidade,
    }

    // A partir de que estágio (o maior já alcançado) o sistema aparece; sistema sem regra já vem aberto
    [Serializable]
    public class LiberacaoDeSistema
    {
        public SistemaDoJogo Sistema;
        public int Estagio = 1;
    }

    [Serializable]
    public class Fala
    {
        public string Quem;
        public string Texto;
    }

    // Cena de história: aparece uma vez, quando o maior estágio alcançado chega ao dela
    [Serializable]
    public class DefinicaoDeCena
    {
        public string Id;
        public int Estagio = 1;
        public List<Fala> Falas = new List<Fala>();
    }

    [Serializable]
    public class DefinicaoDeCapitulo
    {
        public int Numero;
        public string Nome;
        public int EstagioFinal = 30;              // o chefe deste estágio é o chefe do capítulo
        public string NomeDoChefeFinal;
        public double MultiplicadorDoChefeFinal = 3;
        public List<DefinicaoDeCena> Cenas = new List<DefinicaoDeCena>();
    }

    public static class RegrasDosCapitulos
    {
        public static bool Liberado(IEnumerable<LiberacaoDeSistema> liberacoes, SistemaDoJogo sistema, int estagioMaximo)
        {
            foreach (var l in liberacoes) if (l.Sistema == sistema) return estagioMaximo >= l.Estagio;
            return true;
        }

        // Capítulo que contém o estágio (os capítulos vêm em ordem); null depois do último
        public static DefinicaoDeCapitulo Capitulo(IReadOnlyList<DefinicaoDeCapitulo> capitulos, int estagio)
        {
            foreach (var c in capitulos) if (estagio <= c.EstagioFinal) return c;
            return null;
        }
    }
}
