using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Valores de balanceamento dos upgrades. O asset fica em Data/Resources/BalanceamentoDeUpgrades.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento de Upgrades")]
    public class BalanceamentoDeUpgrades : Balanceamento<BalanceamentoDeUpgrades>
    {
        public List<DefinicaoDeUpgrade> upgrades = Padrao();

        public static List<DefinicaoDeUpgrade> Padrao() => new List<DefinicaoDeUpgrade>
        {
            new DefinicaoDeUpgrade
            {
                Id = IdsDosUpgrades.Forca, Nome = "Força do Golpe", Descricao = "Dano de cada click",
                CustoBase = 10, CrescimentoDoCusto = 1.07, ValorBase = 3, Tipo = TipoDeUpgrade.ComMarcos,
            },
            new DefinicaoDeUpgrade
            {
                Id = IdsDosUpgrades.Automatico, Nome = "Ataque Automático", Descricao = "Dano do auto-ataque por segundo",
                CustoBase = 15, CrescimentoDoCusto = 1.08, ValorBase = 5, Tipo = TipoDeUpgrade.ComMarcos,
            },
            new DefinicaoDeUpgrade
            {
                Id = IdsDosUpgrades.Precisao, Nome = "Precisão", Descricao = "Chance de crítico",
                CustoBase = 50, CrescimentoDoCusto = 1.15, ValorBase = 0.05, ValorPorNivel = 0.01, Teto = 0.5,
                Tipo = TipoDeUpgrade.Linear,
            },
        };
    }
}
