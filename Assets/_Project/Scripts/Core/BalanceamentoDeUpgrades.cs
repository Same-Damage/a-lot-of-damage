using System.Collections.Generic;
using UnityEngine;

namespace ALotOfDamage.Core
{
    // Valores de balanceamento dos upgrades. O asset fica em Data/Resources/BalanceamentoDeUpgrades.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento de Upgrades")]
    public class BalanceamentoDeUpgrades : ScriptableObject
    {
        public const string IdForca = "forca_do_golpe";
        public const string IdAutomatico = "ataque_automatico";
        public const string IdPrecisao = "precisao";

        public List<DefinicaoDeUpgrade> upgrades = Padrao();

        static BalanceamentoDeUpgrades carregado;

        public static BalanceamentoDeUpgrades Atual
        {
            get
            {
                if (carregado) return carregado;
                carregado = Resources.Load<BalanceamentoDeUpgrades>(nameof(BalanceamentoDeUpgrades));
                if (!carregado) carregado = CreateInstance<BalanceamentoDeUpgrades>();
                return carregado;
            }
        }

        public DefinicaoDeUpgrade Buscar(string id) => upgrades.Find(u => u.Id == id);

        public static List<DefinicaoDeUpgrade> Padrao() => new List<DefinicaoDeUpgrade>
        {
            new DefinicaoDeUpgrade
            {
                Id = IdForca, Nome = "Força do Golpe", Descricao = "Dano de cada click",
                CustoBase = 10, CrescimentoDoCusto = 1.07, ValorBase = 3, Tipo = TipoDeUpgrade.ComMarcos,
            },
            new DefinicaoDeUpgrade
            {
                Id = IdAutomatico, Nome = "Ataque Automático", Descricao = "Dano do auto-ataque por segundo",
                CustoBase = 15, CrescimentoDoCusto = 1.08, ValorBase = 5, Tipo = TipoDeUpgrade.ComMarcos,
            },
            new DefinicaoDeUpgrade
            {
                Id = IdPrecisao, Nome = "Precisão", Descricao = "Chance de crítico",
                CustoBase = 50, CrescimentoDoCusto = 1.15, ValorBase = 0.05, ValorPorNivel = 0.01, Teto = 0.5,
                Tipo = TipoDeUpgrade.Linear,
            },
        };
    }
}
