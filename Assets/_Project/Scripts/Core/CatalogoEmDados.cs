using System;
using System.Collections.Generic;

namespace ALotOfDamage.Core
{
    // Catálogo em campos simples, para ir em JSON ao servidor (que não lê ScriptableObjects).
    // As regiões ficam de fora: são só o visual do mapa
    [Serializable]
    public class CatalogoEmDados : ICatalogo
    {
        public List<DefinicaoDeUpgrade> upgrades = new List<DefinicaoDeUpgrade>();
        public List<DefinicaoDeItem> itens = new List<DefinicaoDeItem>();
        public List<DefinicaoDeMonstro> monstros = new List<DefinicaoDeMonstro>();
        public ParametrosDoBestiario bestiario = new ParametrosDoBestiario();
        public List<DefinicaoDeClasse> classes = new List<DefinicaoDeClasse>();
        public List<DefinicaoDeFollower> followers = new List<DefinicaoDeFollower>();
        public ParametrosDoGacha gacha = new ParametrosDoGacha();
        public ParametrosDaCampanha campanha = new ParametrosDaCampanha();
        public ParametrosDosItens parametrosDosItens = new ParametrosDosItens();
        public List<DefinicaoDeConquista> conquistas = new List<DefinicaoDeConquista>();
        public List<DefinicaoDeReliquia> reliquias = new List<DefinicaoDeReliquia>();
        public List<DefinicaoDePredio> predios = new List<DefinicaoDePredio>();
        public List<DefinicaoDeMissao> missoes = new List<DefinicaoDeMissao>();
        public List<DefinicaoDeRecurso> recursos = new List<DefinicaoDeRecurso>();
        public List<DefinicaoDePocao> pocoes = new List<DefinicaoDePocao>();
        public List<LiberacaoDeSistema> liberacoes = new List<LiberacaoDeSistema>();
        public List<DefinicaoDeCapitulo> capitulos = new List<DefinicaoDeCapitulo>();
        public int vagasDeExpedicao;
        public double limiteDaTavernaEmHoras;
        public ParametrosDasReliquias parametrosDasReliquias = new ParametrosDasReliquias();
        public ParametrosDasConquistas parametrosDasConquistas = new ParametrosDasConquistas();

        public CatalogoEmDados() { }

        public static CatalogoEmDados De(ICatalogo c) => new CatalogoEmDados
        {
            upgrades = new List<DefinicaoDeUpgrade>(c.Upgrades),
            itens = new List<DefinicaoDeItem>(c.Itens),
            monstros = new List<DefinicaoDeMonstro>(c.Monstros),
            bestiario = c.Bestiario,
            classes = new List<DefinicaoDeClasse>(c.Classes),
            followers = new List<DefinicaoDeFollower>(c.Followers),
            gacha = c.Gacha,
            campanha = c.Campanha,
            parametrosDosItens = c.ParametrosDosItens,
            conquistas = new List<DefinicaoDeConquista>(c.Conquistas),
            reliquias = new List<DefinicaoDeReliquia>(c.Reliquias),
            predios = new List<DefinicaoDePredio>(c.Predios),
            missoes = new List<DefinicaoDeMissao>(c.Missoes),
            recursos = new List<DefinicaoDeRecurso>(c.Recursos),
            pocoes = new List<DefinicaoDePocao>(c.Pocoes),
            liberacoes = new List<LiberacaoDeSistema>(c.Liberacoes),
            capitulos = new List<DefinicaoDeCapitulo>(c.Capitulos),
            vagasDeExpedicao = c.VagasDeExpedicao,
            limiteDaTavernaEmHoras = c.LimiteDaTavernaEmHoras,
            parametrosDasReliquias = c.ParametrosDasReliquias,
            parametrosDasConquistas = c.ParametrosDasConquistas,
        };

        // implementação explícita: para o serializador só existem os campos
        IReadOnlyList<DefinicaoDeUpgrade> ICatalogo.Upgrades => upgrades;
        IReadOnlyList<DefinicaoDeItem> ICatalogo.Itens => itens;
        IReadOnlyList<DefinicaoDeMonstro> ICatalogo.Monstros => monstros;
        ParametrosDoBestiario ICatalogo.Bestiario => bestiario;
        IReadOnlyList<DefinicaoDeClasse> ICatalogo.Classes => classes;
        IReadOnlyList<DefinicaoDeFollower> ICatalogo.Followers => followers;
        ParametrosDoGacha ICatalogo.Gacha => gacha;
        ParametrosDaCampanha ICatalogo.Campanha => campanha;
        ParametrosDosItens ICatalogo.ParametrosDosItens => parametrosDosItens;
        IReadOnlyList<DefinicaoDeRegiao> ICatalogo.Regioes => Array.Empty<DefinicaoDeRegiao>();
        IReadOnlyList<DefinicaoDeConquista> ICatalogo.Conquistas => conquistas;
        IReadOnlyList<DefinicaoDeReliquia> ICatalogo.Reliquias => reliquias;
        IReadOnlyList<DefinicaoDePredio> ICatalogo.Predios => predios;
        IReadOnlyList<DefinicaoDeMissao> ICatalogo.Missoes => missoes;
        IReadOnlyList<DefinicaoDeRecurso> ICatalogo.Recursos => recursos;
        IReadOnlyList<DefinicaoDePocao> ICatalogo.Pocoes => pocoes;
        IReadOnlyList<LiberacaoDeSistema> ICatalogo.Liberacoes => liberacoes;
        IReadOnlyList<DefinicaoDeCapitulo> ICatalogo.Capitulos => capitulos;
        int ICatalogo.VagasDeExpedicao => vagasDeExpedicao;
        double ICatalogo.LimiteDaTavernaEmHoras => limiteDaTavernaEmHoras;
        ParametrosDasReliquias ICatalogo.ParametrosDasReliquias => parametrosDasReliquias;
        ParametrosDasConquistas ICatalogo.ParametrosDasConquistas => parametrosDasConquistas;
    }
}
