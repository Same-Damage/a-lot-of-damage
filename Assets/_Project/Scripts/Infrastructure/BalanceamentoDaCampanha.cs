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

        static List<ParDeValor> Receita(params (string id, int n)[] itens)
        {
            var l = new List<ParDeValor>();
            foreach (var (id, n) in itens) l.Add(new ParDeValor { Id = id, Valor = n });
            return l;
        }

        // Alquimia: ingredientes da coleta e dos monstros
        public List<DefinicaoDePocao> pocoes = new List<DefinicaoDePocao>
        {
            new DefinicaoDePocao { Id = "forca", Nome = "Poção de Força", Descricao = "×2 em todo o dano por 10 min",
                Efeito = EfeitoDaPocao.Dano, Valor = 1, DuracaoEmMinutos = 10, CustoEmVitorias = 4,
                Ingredientes = Receita(("erva_lunar", 3), ("presa", 2)) },
            new DefinicaoDePocao { Id = "fortuna", Nome = "Elixir da Fortuna", Descricao = "+100% de ouro por 10 min",
                Efeito = EfeitoDaPocao.Ouro, Valor = 1, DuracaoEmMinutos = 10, CustoEmVitorias = 4,
                Ingredientes = Receita(("cogumelo_rubro", 3), ("gosma", 2)) },
            new DefinicaoDePocao { Id = "sorte", Nome = "Tônico da Sorte", Descricao = "+100% de chance de item por 10 min",
                Efeito = EfeitoDaPocao.Drop, Valor = 1, DuracaoEmMinutos = 10, CustoEmVitorias = 6,
                Ingredientes = Receita(("minerio_celeste", 2), ("cogumelo_rubro", 3)) },
            new DefinicaoDePocao { Id = "precisao", Nome = "Óleo da Precisão", Descricao = "+10% de chance crítica por 10 min",
                Efeito = EfeitoDaPocao.Critico, Valor = 0.1, DuracaoEmMinutos = 10, CustoEmVitorias = 6,
                Ingredientes = Receita(("minerio_celeste", 2), ("erva_lunar", 3)) },
            new DefinicaoDePocao { Id = "sono", Nome = "Poção do Sono Profundo", Descricao = "×2 na próxima recompensa AFK",
                Efeito = EfeitoDaPocao.Afk, Valor = 1, DuracaoEmMinutos = 0, CustoEmVitorias = 10,
                Ingredientes = Receita(("erva_lunar", 3), ("cogumelo_rubro", 3), ("minerio_celeste", 2)) },
        };

        // Pontos de coleta espalhados pelo mapa (ingredientes da alquimia)
        public List<DefinicaoDeRecurso> recursos = new List<DefinicaoDeRecurso>
        {
            new DefinicaoDeRecurso
            {
                Id = "moita_erva", Nome = "Moita de erva-lunar", MaterialId = "erva_lunar", NomeDoMaterial = "Erva-lunar",
                Forma = FormaDoRecurso.Erva, Cor = "#8CE07A", Golpes = 4, QuantidadeBase = 3, SegundosParaRebrotar = 40,
            },
            new DefinicaoDeRecurso
            {
                Id = "cogumelo", Nome = "Cogumelo rubro", MaterialId = "cogumelo_rubro", NomeDoMaterial = "Cogumelo rubro",
                Forma = FormaDoRecurso.Cogumelo, Cor = "#D84A4A", Golpes = 5, QuantidadeBase = 2, SegundosParaRebrotar = 50,
            },
            new DefinicaoDeRecurso
            {
                Id = "veio_minerio", Nome = "Veio de minério celeste", MaterialId = "minerio_celeste", NomeDoMaterial = "Minério celeste",
                Forma = FormaDoRecurso.Minerio, Cor = "#5AB8F0", Golpes = 8, QuantidadeBase = 2, SegundosParaRebrotar = 70,
            },
        };

        public List<DefinicaoDeRegiao> regioes = new List<DefinicaoDeRegiao>
        {
            // Dia claro, campo aberto e folhas ao vento
            new DefinicaoDeRegiao
            {
                Nome = "Campos Verdes", Monstros = new List<string> { "slime", "lobo" },
                Visual = new VisualDaRegiao(),
            },
            // Mata fechada, luz fria de fim de tarde e vaga-lumes
            new DefinicaoDeRegiao
            {
                Nome = "Bosque Sombrio", Monstros = new List<string> { "lobo", "golem" },
                Visual = new VisualDaRegiao
                {
                    CorDaGrama = "#4C7A3E", CorDaTerra = "#5C4632", CorDaRocha = "#6A6E6C", CorDaFolhagem = "#2C5A36", CorDoTronco = "#4A3424",
                    CorDoCristal = "#9B7CFF", CorDoSol = "#FFE2C2", CorDoCeu = "#8A9BC4", CorDaNevoa = "#8C9AB4",
                    CorDasNuvens = "#B8BFD6", IntensidadeDoSol = 1.05f, DensidadeDaNevoa = 0.005f,
                    Arvores = 45, Pinheiros = 110, Pedras = 30, Cristais = 14, Ruinas = 0, Arbustos = 110, Grama = 900,
                    Particulas = TipoDeParticula.Vagalumes,
                },
            },
            // Pedra clara e areia, sol forte e poeira no ar
            new DefinicaoDeRegiao
            {
                Nome = "Ruínas Antigas", Monstros = new List<string> { "slime", "golem" },
                Visual = new VisualDaRegiao
                {
                    CorDaGrama = "#A9A46C", CorDaTerra = "#C8B48A", CorDaRocha = "#CFC4AE", CorDaFolhagem = "#7E8A45", CorDoTronco = "#6E5236",
                    CorDoCristal = "#FFB347", CorDoSol = "#FFF0D8", CorDoCeu = "#E9D6B4", CorDaNevoa = "#E6D8BE",
                    CorDasNuvens = "#FBEBD3", IntensidadeDoSol = 1.25f, DensidadeDaNevoa = 0.004f,
                    Arvores = 18, Pinheiros = 0, Pedras = 60, Cristais = 6, Ruinas = 30, Arbustos = 60, Grama = 500,
                    Particulas = TipoDeParticula.Poeira,
                },
            },
        };
    }
}
