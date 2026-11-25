using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // História e liberação dos sistemas por capítulo. O asset fica em Data/Resources/BalanceamentoDaHistoria.
    //
    // O mundo: ilhas de terra num mar sem fim. Quando a Coroa Celeste se partiu, os fragmentos caíram espalhados pelas ilhas
    // e despertaram monstros. Ilhas próximas se alcançam de barco; as mais perigosas, só pelo céu, em naves aéreas.
    // O Rei Caído caça os fragmentos para refazer a coroa e governar mar e céu.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento da História")]
    public class BalanceamentoDaHistoria : Balanceamento<BalanceamentoDaHistoria>
    {
        // Um sistema novo a cada poucos estágios, sempre apresentado por uma cena
        public List<LiberacaoDeSistema> liberacoes = new List<LiberacaoDeSistema>
        {
            L(SistemaDoJogo.Forja, 3),
            L(SistemaDoJogo.Inventario, 5),
            L(SistemaDoJogo.Bestiario, 7),
            L(SistemaDoJogo.Followers, 10),
            L(SistemaDoJogo.Conquistas, 12),
            L(SistemaDoJogo.Reliquias, 15),
            L(SistemaDoJogo.Taverna, 18),
            L(SistemaDoJogo.Expedicoes, 21),
            L(SistemaDoJogo.Alquimia, 24),
            L(SistemaDoJogo.Ascensao, 50),
        };

        public List<DefinicaoDeCapitulo> capitulos = new List<DefinicaoDeCapitulo>
        {
            new DefinicaoDeCapitulo
            {
                Numero = 1, Nome = "O Porto de Partida", EstagioFinal = 30,
                NomeDoChefeFinal = "Almirante Espectral", MultiplicadorDoChefeFinal = 3,
                Cenas = new List<DefinicaoDeCena>
                {
                    C("cap1_inicio", 1,
                        ("Capitã Marina", "Acordou, finalmente! Você foi o único que sobreviveu ao naufrágio."),
                        ("Capitã Marina", "Desde que a Coroa Celeste se partiu, os mares ficaram cheios de monstros. Esta ilha também."),
                        ("Capitã Marina", "Meu navio precisa de reparos. Limpe os campos para mim e eu te levo para onde quiser.")),
                    C("cap1_forja", 3,
                        ("Ferreira Brunhilda", "Essa lâmina não aguenta nem uma gosma. Traga ouro e presas e eu deixo ela digna de um herói."),
                        ("Narrador", "A Forja está aberta: refine seus itens e fabrique novos.")),
                    C("cap1_inventario", 5,
                        ("Capitã Marina", "Um chefe guardava esta trilha! Olhe só o que ele deixou cair."),
                        ("Capitã Marina", "Os monstros carregam equipamentos de quem eles venceram. Guarde, equipe e venda o que sobrar."),
                        ("Narrador", "O Inventário está aberto.")),
                    C("cap1_bestiario", 7,
                        ("Sábio Odran", "Cada criatura tem seus pontos fracos. Quanto mais você enfrenta, mais aprende sobre ela."),
                        ("Narrador", "O Bestiário está aberto: derrotar um tipo de monstro muitas vezes dá bônus permanentes.")),
                    C("cap1_followers", 10,
                        ("Oráculo dos Pactos", "Viajante... criaturas e heróis de todas as ilhas atendem ao chamado dos cristais."),
                        ("Oráculo dos Pactos", "Faça um pacto e alguém lutará ao seu lado."),
                        ("Narrador", "Os Followers estão abertos: invoque companheiros com cristais.")),
                    C("cap1_bosque", 11,
                        ("Capitã Marina", "Além dos campos fica o Bosque Sombrio. Dizem que um fragmento da coroa caiu lá dentro."),
                        ("Capitã Marina", "Tome cuidado. As árvores lá têm olhos.")),
                    C("cap1_conquistas", 12,
                        ("Narrador", "Seus feitos começam a ser contados nas tavernas do porto."),
                        ("Narrador", "As Conquistas estão abertas: cada meta cumprida dá cristais e fortalece todo o seu dano.")),
                    C("cap1_reliquias", 15,
                        ("Sábio Odran", "Estes cacos brilhantes... são fragmentos da Coroa Celeste! Os chefes os carregam sem saber."),
                        ("Sábio Odran", "Junte os cacos e eu monto relíquias com o poder que restou neles."),
                        ("Narrador", "As Relíquias estão abertas: chefes dão fragmentos para montar e melhorar relíquias.")),
                    C("cap1_taverna", 18,
                        ("Taverneira Rosa", "O porto está renascendo graças a você. Que tal cuidar desta taverna velha?"),
                        ("Taverneira Rosa", "Clientes pagam até quando você está longe, e uma boa cozinha deixa qualquer um mais forte."),
                        ("Narrador", "A Taverna está aberta.")),
                    C("cap1_ruinas", 21,
                        ("Capitã Marina", "O navio está pronto! Vamos às Ruínas Antigas, do outro lado da baía."),
                        ("Capitã Marina", "E seus companheiros que estão parados podem navegar comigo em expedições pelas ilhas próximas."),
                        ("Narrador", "As Expedições estão abertas.")),
                    C("cap1_alquimia", 24,
                        ("Alquimista Vex", "Ervas-lunares, cogumelos rubros, minério celeste... a ilha está cheia de ingredientes."),
                        ("Alquimista Vex", "Traga para mim e eu preparo poções que nem o Rei Caído conhece."),
                        ("Narrador", "A Alquimia está aberta: colha ingredientes no mapa golpeando os pontos de coleta.")),
                    C("cap1_chefe", 30,
                        ("Capitã Marina", "Aquele navio-fantasma nas ruínas... é o Almirante Espectral. Ele guarda o maior fragmento da ilha."),
                        ("Capitã Marina", "Derrote-o e o caminho para o mar aberto estará livre.")),
                    C("cap1_fim", 31,
                        ("Capitã Marina", "Você conseguiu! O fragmento é seu."),
                        ("Rei Caído", "...Um fragmento a menos para mim. Aproveite enquanto pode, herói."),
                        ("Capitã Marina", "Meu barco não chega às ilhas do céu. Para seguir os fragmentos, vamos precisar de uma nave aérea."),
                        ("Narrador", "Fim do Capítulo 1. Os estágios seguem: fique mais forte para o que vem pela frente.")),
                    C("ascensao", 50,
                        ("Oráculo dos Pactos", "Sua alma já carrega o peso de muitas batalhas. Você pode renascer mais forte."),
                        ("Narrador", "A Ascensão está aberta: recomece em troca de Almas, que multiplicam todo o dano para sempre.")),
                },
            },
        };

        static LiberacaoDeSistema L(SistemaDoJogo s, int estagio) => new LiberacaoDeSistema { Sistema = s, Estagio = estagio };

        static DefinicaoDeCena C(string id, int estagio, params (string quem, string texto)[] falas)
        {
            var cena = new DefinicaoDeCena { Id = id, Estagio = estagio };
            foreach (var (quem, texto) in falas) cena.Falas.Add(new Fala { Quem = quem, Texto = texto });
            return cena;
        }
    }
}
