using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Monstros, materiais que dropam e bônus do bestiário. O asset fica em Data/Resources/BalanceamentoDoBestiario.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento do Bestiário")]
    public class BalanceamentoDoBestiario : Balanceamento<BalanceamentoDoBestiario>
    {
        public ParametrosDoBestiario parametros = new ParametrosDoBestiario();
        public List<DefinicaoDeMonstro> monstros = Padrao();

        public static List<DefinicaoDeMonstro> Padrao() => new List<DefinicaoDeMonstro>
        {
            new DefinicaoDeMonstro { Id = "slime", Nome = "Slime", MaterialId = "gosma", NomeDoMaterial = "Gosma", MaterialPorVitoria = 2 },
            new DefinicaoDeMonstro { Id = "lobo", Nome = "Lobo", MaterialId = "presa", NomeDoMaterial = "Presa", MaterialPorVitoria = 2 },
            new DefinicaoDeMonstro { Id = "golem", Nome = "Golem", MaterialId = "pedra_runica", NomeDoMaterial = "Pedra Rúnica", MaterialPorVitoria = 3 },
        };
    }
}
