using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Guilda, chefe da semana e arena. O asset fica em Data/Resources/BalanceamentoDaGuilda; o servidor usa a cópia do catálogo exportado.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento da Guilda")]
    public class BalanceamentoDaGuilda : Balanceamento<BalanceamentoDaGuilda>
    {
        public ParametrosDaGuilda parametros = new ParametrosDaGuilda();
        public ParametrosDaArena arena = new ParametrosDaArena();
    }
}
