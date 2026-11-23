using System.Collections.Generic;
using ALotOfDamage.Core;

namespace ALotOfDamage.Infrastructure
{
    // Progresso só em memória, até existir save local ou o servidor
    public class RepositorioDeProgressoEmMemoria : IRepositorioDeProgresso
    {
        readonly Progresso progresso = new Progresso();

        public Progresso Carregar() => progresso;

        public void Salvar(Progresso p) { }
    }

    // Catálogo lido dos ScriptableObjects de Data/Resources
    public class CatalogoDeBalanceamento : ICatalogo
    {
        public IReadOnlyList<DefinicaoDeUpgrade> Upgrades { get; }
        public IReadOnlyList<DefinicaoDeItem> Itens { get; }
        public IReadOnlyList<DefinicaoDeMonstro> Monstros { get; }
        public ParametrosDoBestiario Bestiario { get; }

        public CatalogoDeBalanceamento()
        {
            var bestiario = BalanceamentoDoBestiario.Carregar();
            Upgrades = BalanceamentoDeUpgrades.Carregar().upgrades;
            Itens = BalanceamentoDaForja.Carregar().itens;
            Monstros = bestiario.monstros;
            Bestiario = bestiario.parametros;
        }
    }

    public class SorteioDaUnity : ISorteio
    {
        public double Proximo() => UnityEngine.Random.value;
    }
}
