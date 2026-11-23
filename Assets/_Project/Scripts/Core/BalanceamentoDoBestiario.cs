using System.Collections.Generic;
using UnityEngine;

namespace ALotOfDamage.Core
{
    // Monstros, materiais que dropam e bônus do bestiário. O asset fica em Data/Resources/BalanceamentoDoBestiario.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento do Bestiário")]
    public class BalanceamentoDoBestiario : Balanceamento<BalanceamentoDoBestiario>
    {
        public ParametrosDoBestiario parametros = new ParametrosDoBestiario();
        public List<DefinicaoDeMonstro> monstros = Padrao();

        public DefinicaoDeMonstro Buscar(string id) => monstros.Find(m => m.Id == id);

        public string NomeDoMaterial(string materialId) =>
            monstros.Find(m => m.MaterialId == materialId)?.NomeDoMaterial ?? materialId;

        public static List<DefinicaoDeMonstro> Padrao() => new List<DefinicaoDeMonstro>
        {
            new DefinicaoDeMonstro { Id = "slime", Nome = "Slime", MaterialId = "gosma", NomeDoMaterial = "Gosma", MaterialPorVitoria = 2 },
            new DefinicaoDeMonstro { Id = "lobo", Nome = "Lobo", MaterialId = "presa", NomeDoMaterial = "Presa", MaterialPorVitoria = 2 },
            new DefinicaoDeMonstro { Id = "golem", Nome = "Golem", MaterialId = "pedra_runica", NomeDoMaterial = "Pedra Rúnica", MaterialPorVitoria = 3 },
        };
    }
}
