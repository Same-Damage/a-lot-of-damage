using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Metas de todo o jogo e seus degraus. O asset fica em Data/Resources/BalanceamentoDasConquistas.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento das Conquistas")]
    public class BalanceamentoDasConquistas : Balanceamento<BalanceamentoDasConquistas>
    {
        public ParametrosDasConquistas parametros = new ParametrosDasConquistas();
        public List<DefinicaoDeConquista> conquistas = Padrao();

        static DefinicaoDeConquista Meta(string id, string nome, string descricao, MetaDaConquista meta, params double[] degraus) =>
            new DefinicaoDeConquista { Id = id, Nome = nome, Descricao = descricao, Meta = meta, Degraus = degraus };

        public static List<DefinicaoDeConquista> Padrao() => new List<DefinicaoDeConquista>
        {
            Meta("vitorias", "Guerreiro incansável", "Vença {0} batalhas", MetaDaConquista.Vitorias, 10, 100, 1000, 10000, 100000),
            Meta("chefes", "Caçador de chefes", "Derrote {0} chefes", MetaDaConquista.ChefesDerrotados, 1, 5, 25, 100, 500),
            Meta("estagio", "Desbravador", "Chegue ao estágio {0}", MetaDaConquista.EstagioMaximo, 10, 25, 50, 100, 200),
            Meta("ascensoes", "Renascido", "Ascenda {0} vezes", MetaDaConquista.Ascensoes, 1, 5, 10, 25, 50),
            Meta("epicos", "Tesouro raro", "Obtenha {0} itens Épicos ou Lendários", MetaDaConquista.ItensEpicosObtidos, 1, 10, 50, 200),
            Meta("refino", "Mestre ferreiro", "Refine um item até +{0}", MetaDaConquista.MaiorRefino, 5, 10, 20, 40),
            Meta("fabricados", "Artesão", "Fabrique {0} itens na forja", MetaDaConquista.ItensFabricados, 1, 10, 50, 200),
            Meta("ouro", "Fortuna", "Ganhe {0} de ouro no total", MetaDaConquista.OuroTotalGanho, 1e3, 1e6, 1e9, 1e12, 1e15),
            Meta("bestiario", "Estudioso", "Alcance {0} marcos no bestiário", MetaDaConquista.MarcosDoBestiario, 1, 5, 10, 20),
        };
    }
}
