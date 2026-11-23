using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Classes e followers. O asset fica em Data/Resources/BalanceamentoDePersonagens.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento de Personagens")]
    public class BalanceamentoDePersonagens : Balanceamento<BalanceamentoDePersonagens>
    {
        public List<DefinicaoDeClasse> classes = new List<DefinicaoDeClasse>
        {
            new DefinicaoDeClasse
            {
                Id = Progresso.ClassePadrao, Nome = "Guerreiro", NomeDaPassiva = "Fúria",
                BonusDeFuriaPorAcumulo = 0.05, CombosPorAcumulo = 10, MaximoDeAcumulos = 10,
                NomeDaUltimate = "Golpe Devastador", MultiplicadorDaUltimate = 50,
            },
        };

        public List<DefinicaoDeFollower> followers = new List<DefinicaoDeFollower>
        {
            new DefinicaoDeFollower
            {
                Id = Progresso.FollowerPadrao, Nome = "Pip", Descricao = "Falcão que ataca junto e marca o ponto fraco",
                FracaoDoAutoAtaque = 0.2, IntervaloDeAtaque = 1.5f, BonusNaJanelaDoPontoFraco = 0.3,
            },
        };
    }
}
