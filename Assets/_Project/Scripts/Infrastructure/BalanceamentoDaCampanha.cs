using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Estágios, chefes e regiões. O asset fica em Data/Resources/BalanceamentoDaCampanha.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento da Campanha")]
    public class BalanceamentoDaCampanha : Balanceamento<BalanceamentoDaCampanha>
    {
        public ParametrosDaCampanha parametros = new ParametrosDaCampanha();

        public List<DefinicaoDeRegiao> regioes = new List<DefinicaoDeRegiao>
        {
            new DefinicaoDeRegiao { Nome = "Campos Verdes", Monstros = new List<string> { "slime", "lobo" }, CorDoChao = "#598C4C" },
            new DefinicaoDeRegiao { Nome = "Bosque Sombrio", Monstros = new List<string> { "lobo", "golem" }, CorDoChao = "#2F4A33" },
            new DefinicaoDeRegiao { Nome = "Ruínas Antigas", Monstros = new List<string> { "slime", "golem" }, CorDoChao = "#7A6E5A" },
        };
    }
}
