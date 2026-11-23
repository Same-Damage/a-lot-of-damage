using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Itens da forja. O asset fica em Data/Resources/BalanceamentoDaForja.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento da Forja")]
    public class BalanceamentoDaForja : Balanceamento<BalanceamentoDaForja>
    {
        public List<DefinicaoDeItem> itens = Padrao();

        public static List<DefinicaoDeItem> Padrao() => new List<DefinicaoDeItem>
        {
            new DefinicaoDeItem
            {
                Id = "espada", Nome = "Espada", Descricao = "Dano do click", Alvo = AlvoDoItem.Click,
                BonusPorNivel = 0.5, CustoBaseEmOuro = 50, CrescimentoDoCusto = 1.6, MaterialId = "presa", MaterialPorNivel = 2,
            },
            new DefinicaoDeItem
            {
                Id = "anel", Nome = "Anel", Descricao = "Dano do auto-ataque", Alvo = AlvoDoItem.Automatico,
                BonusPorNivel = 0.5, CustoBaseEmOuro = 75, CrescimentoDoCusto = 1.6, MaterialId = "gosma", MaterialPorNivel = 3,
            },
            new DefinicaoDeItem
            {
                Id = "manopla", Nome = "Manopla", Descricao = "Dano do click", Alvo = AlvoDoItem.Click,
                BonusPorNivel = 1, CustoBaseEmOuro = 500, CrescimentoDoCusto = 1.8, MaterialId = "pedra_runica", MaterialPorNivel = 3,
            },
        };
    }
}
