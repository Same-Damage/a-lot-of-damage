using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Tipos de item (um por slot), refino na forja e regras de drop, fabricação, venda e desmonte.
    // O asset fica em Data/Resources/BalanceamentoDaForja.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento da Forja")]
    public class BalanceamentoDaForja : Balanceamento<BalanceamentoDaForja>
    {
        public List<DefinicaoDeItem> itens = Padrao();
        public ParametrosDosItens parametros = new ParametrosDosItens();

        static DefinicaoDeItem Tipo(string id, string nome, SlotDeItem slot, AlvoDoItem alvo, double bonus, double custo, string material, int materialPorNivel) =>
            new DefinicaoDeItem
            {
                Id = id, Nome = nome, Slot = slot, Alvo = alvo,
                Descricao = alvo == AlvoDoItem.Click ? "Dano do click" : "Dano do auto-ataque",
                BonusPorNivel = bonus, CustoBaseEmOuro = custo, CrescimentoDoCusto = 1.6, MaterialId = material, MaterialPorNivel = materialPorNivel,
            };

        // Os ids "espada", "manopla" e "anel" são os da forja antiga: saves de antes dos itens viram estes tipos
        public static List<DefinicaoDeItem> Padrao() => new List<DefinicaoDeItem>
        {
            Tipo("espada", "Espada", SlotDeItem.Arma, AlvoDoItem.Click, 0.5, 50, "presa", 2),
            Tipo("elmo", "Elmo", SlotDeItem.Elmo, AlvoDoItem.Automatico, 0.3, 60, "pedra_runica", 2),
            Tipo("peitoral", "Peitoral", SlotDeItem.Peitoral, AlvoDoItem.Automatico, 0.4, 80, "pedra_runica", 3),
            Tipo("manopla", "Manopla", SlotDeItem.Luvas, AlvoDoItem.Click, 0.4, 70, "pedra_runica", 2),
            Tipo("botas", "Botas", SlotDeItem.Botas, AlvoDoItem.Automatico, 0.3, 60, "presa", 2),
            Tipo("cinto", "Cinto", SlotDeItem.Cinto, AlvoDoItem.Automatico, 0.3, 60, "gosma", 2),
            Tipo("anel", "Anel", SlotDeItem.Anel, AlvoDoItem.Click, 0.4, 75, "gosma", 3),
            Tipo("colar", "Colar", SlotDeItem.Colar, AlvoDoItem.Automatico, 0.4, 75, "gosma", 3),
        };
    }
}
