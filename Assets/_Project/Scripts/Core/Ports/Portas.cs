using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Onde o progresso fica guardado: memória hoje, servidor (Nakama) depois
    public interface IRepositorioDeProgresso
    {
        Progresso Carregar();
        void Salvar(Progresso progresso);
    }

    // Dados de balanceamento: upgrades, itens da forja, monstros e bônus do bestiário
    public interface ICatalogo
    {
        IReadOnlyList<DefinicaoDeUpgrade> Upgrades { get; }
        IReadOnlyList<DefinicaoDeItem> Itens { get; }
        IReadOnlyList<DefinicaoDeMonstro> Monstros { get; }
        ParametrosDoBestiario Bestiario { get; }
        IReadOnlyList<DefinicaoDeClasse> Classes { get; }
        IReadOnlyList<DefinicaoDeFollower> Followers { get; }
        ParametrosDaCampanha Campanha { get; }
        IReadOnlyList<DefinicaoDeRegiao> Regioes { get; }
    }

    // Número aleatório em [0, 1), para o teste controlar a sorte
    public interface ISorteio
    {
        double Proximo();
    }

    public static class ExtensoesDoCatalogo
    {
        static T Buscar<T>(IReadOnlyList<T> lista, System.Func<T, bool> criterio) where T : class
        {
            foreach (var x in lista) if (criterio(x)) return x;
            return null;
        }

        public static DefinicaoDeUpgrade Upgrade(this ICatalogo c, string id) => Buscar(c.Upgrades, u => u.Id == id);
        public static DefinicaoDeItem Item(this ICatalogo c, string id) => Buscar(c.Itens, i => i.Id == id);
        public static DefinicaoDeMonstro Monstro(this ICatalogo c, string id) => Buscar(c.Monstros, m => m.Id == id);

        public static DefinicaoDeClasse Classe(this ICatalogo c, string id) => Buscar(c.Classes, x => x.Id == id);
        public static DefinicaoDeFollower Follower(this ICatalogo c, string id) => Buscar(c.Followers, x => x.Id == id);

        public static string NomeDoMaterial(this ICatalogo c, string materialId) =>
            Buscar(c.Monstros, m => m.MaterialId == materialId)?.NomeDoMaterial ?? materialId;
    }
}
