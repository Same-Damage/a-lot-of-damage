using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Prédios da taverna própria. O asset fica em Data/Resources/BalanceamentoDaTaverna.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento da Taverna")]
    public class BalanceamentoDaTaverna : Balanceamento<BalanceamentoDaTaverna>
    {
        public double limiteEmHoras = 12;

        public List<DefinicaoDePredio> predios = new List<DefinicaoDePredio>
        {
            new DefinicaoDePredio
            {
                Id = "balcao", Nome = "Balcão", Descricao = "Clientes pagando: 2% do ouro de uma vitória por segundo, por nível",
                Efeito = EfeitoDoPredio.OuroPorSegundo, BonusPorNivel = 0.02, CustoBase = 200, CrescimentoDoCusto = 1.6,
            },
            new DefinicaoDePredio
            {
                Id = "cozinha", Nome = "Cozinha", Descricao = "Refeições fortes: +3% de todo o dano por nível",
                Efeito = EfeitoDoPredio.Dano, BonusPorNivel = 0.03, CustoBase = 500, CrescimentoDoCusto = 1.8,
            },
            new DefinicaoDePredio
            {
                Id = "quartos", Nome = "Quartos", Descricao = "Descanso: +3% da recompensa AFK por nível",
                Efeito = EfeitoDoPredio.Afk, BonusPorNivel = 0.03, CustoBase = 400, CrescimentoDoCusto = 1.7,
            },
            new DefinicaoDePredio
            {
                Id = "adega", Nome = "Adega", Descricao = "Bons negócios: +3% de ouro das vitórias por nível",
                Efeito = EfeitoDoPredio.Ouro, BonusPorNivel = 0.03, CustoBase = 400, CrescimentoDoCusto = 1.7,
            },
        };
    }
}
