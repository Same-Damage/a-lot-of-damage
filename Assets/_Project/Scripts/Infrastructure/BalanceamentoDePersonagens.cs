using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Classes, os 40 followers do gacha e as regras do gacha. O asset fica em Data/Resources/BalanceamentoDePersonagens.
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

        public ParametrosDoGacha gacha = new ParametrosDoGacha();
        public List<DefinicaoDeFollower> followers = Followers();

        // Quanto o efeito vale em cada raridade (Comum → Lendário); os raros dão efeitos maiores
        static readonly Dictionary<EfeitoDoFollower, double[]> ValoresDosEfeitos = new Dictionary<EfeitoDoFollower, double[]>
        {
            [EfeitoDoFollower.DanoExtra] = new[] { 0.1, 0.15, 0.25, 0.4, 0.6 },
            [EfeitoDoFollower.PontoFraco] = new[] { 0.3, 0.4, 0.55, 0.8, 1.2 },
            [EfeitoDoFollower.Ouro] = new[] { 0.1, 0.15, 0.25, 0.4, 0.6 },
            [EfeitoDoFollower.Afk] = new[] { 0.1, 0.15, 0.25, 0.4, 0.6 },
            [EfeitoDoFollower.Critico] = new[] { 0.02, 0.03, 0.05, 0.08, 0.12 },
            [EfeitoDoFollower.Drop] = new[] { 0.1, 0.15, 0.25, 0.4, 0.6 },
        };

        // Parte do auto-ataque por golpe do follower, por raridade
        static readonly double[] FracaoPorRaridade = { 0.2, 0.25, 0.35, 0.5, 0.75 };

        static readonly Dictionary<EfeitoDoFollower, string> TextosDosEfeitos = new Dictionary<EfeitoDoFollower, string>
        {
            [EfeitoDoFollower.DanoExtra] = "Ataca com muito mais força",
            [EfeitoDoFollower.PontoFraco] = "Deixa o ponto fraco aberto por mais tempo",
            [EfeitoDoFollower.Ouro] = "Mais ouro a cada vitória",
            [EfeitoDoFollower.Afk] = "Luta mais enquanto você está fora",
            [EfeitoDoFollower.Critico] = "Mais chance de golpe crítico",
            [EfeitoDoFollower.Drop] = "Mais chance de item cair",
        };

        static DefinicaoDeFollower F(string id, string nome, Raridade r, EfeitoDoFollower e, FamiliaDoFollower familia, string cor, string cor2, float escala = 1) =>
            new DefinicaoDeFollower
            {
                Id = id, Nome = nome, Raridade = r, Efeito = e, ValorDoEfeito = ValoresDosEfeitos[e][(int)r],
                Descricao = TextosDosEfeitos[e], FracaoDoAutoAtaque = FracaoPorRaridade[(int)r], IntervaloDeAtaque = 1.5f,
                Familia = familia, Cor = cor, CorSecundaria = cor2, Escala = escala,
            };

        const Raridade C = Raridade.Comum, I = Raridade.Incomum, R = Raridade.Raro, E = Raridade.Epico, L = Raridade.Lendario;

        public static List<DefinicaoDeFollower> Followers() => new List<DefinicaoDeFollower>
        {
            // Comuns (14)
            F(Progresso.FollowerPadrao, "Pip, o falcão", C, EfeitoDoFollower.PontoFraco, FamiliaDoFollower.Ave, "#8A6A4A", "#E8D8B0"),
            F("escudeiro", "Escudeiro Tobias", C, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Humanoide, "#6E7C8A", "#C9A86A"),
            F("aprendiz", "Aprendiz Lia", C, EfeitoDoFollower.Critico, FamiliaDoFollower.Humanoide, "#5A4AA0", "#E6D2A0"),
            F("mercador", "Mercador Bento", C, EfeitoDoFollower.Ouro, FamiliaDoFollower.Humanoide, "#8C5A2E", "#D8B060"),
            F("goblin_ladrao", "Goblin Ladrão", C, EfeitoDoFollower.Drop, FamiliaDoFollower.Humanoide, "#5E8C3A", "#6A4A2E", 0.75f),
            F("lobo_cinzento", "Lobo Cinzento", C, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Besta, "#8C8C96", "#D8D8E0"),
            F("javali", "Javali Bravo", C, EfeitoDoFollower.Afk, FamiliaDoFollower.Besta, "#6A4A32", "#D8C8A8"),
            F("raposa", "Raposa Ágil", C, EfeitoDoFollower.Critico, FamiliaDoFollower.Besta, "#D2703A", "#F4EAD8", 0.8f),
            F("corvo", "Corvo Mensageiro", C, EfeitoDoFollower.Drop, FamiliaDoFollower.Ave, "#2A2A36", "#6A6A80", 0.8f),
            F("coruja", "Coruja Sábia", C, EfeitoDoFollower.PontoFraco, FamiliaDoFollower.Ave, "#9A7A52", "#E8D8B8"),
            F("fada_jardim", "Fada do Jardim", C, EfeitoDoFollower.Afk, FamiliaDoFollower.Espirito, "#7AD88A", "#F4F0A0", 0.6f),
            F("fogo_fatuo", "Fogo-fátuo", C, EfeitoDoFollower.Ouro, FamiliaDoFollower.Espirito, "#5AC8FF", "#E0F8FF", 0.6f),
            F("boneco_lata", "Boneco de Lata", C, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Constructo, "#9AA0A8", "#C8A050", 0.85f),
            F("cogumelo", "Cogumelo Saltitante", C, EfeitoDoFollower.Afk, FamiliaDoFollower.Mitico, "#C84A4A", "#F4EAD8", 0.7f),

            // Incomuns (10)
            F("arqueira_elfa", "Arqueira Élfica", I, EfeitoDoFollower.Critico, FamiliaDoFollower.Humanoide, "#3A8A5A", "#E8D090"),
            F("anao_ferreiro", "Anão Ferreiro", I, EfeitoDoFollower.Drop, FamiliaDoFollower.Humanoide, "#8A4A2A", "#D8A040", 0.8f),
            F("monge", "Monge Errante", I, EfeitoDoFollower.PontoFraco, FamiliaDoFollower.Humanoide, "#D88A2A", "#F4E2B8"),
            F("urso_pardo", "Urso Pardo", I, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Besta, "#5A3A22", "#A87A52", 1.25f),
            F("pantera", "Pantera das Sombras", I, EfeitoDoFollower.Critico, FamiliaDoFollower.Besta, "#22222E", "#6A4AA0"),
            F("aguia_real", "Águia Real", I, EfeitoDoFollower.Ouro, FamiliaDoFollower.Ave, "#6A4A2A", "#F4F0E8", 1.1f),
            F("golem_musgo", "Golem de Musgo", I, EfeitoDoFollower.Afk, FamiliaDoFollower.Constructo, "#6A7A5A", "#7AB84A", 1.1f),
            F("automato", "Autômato de Cobre", I, EfeitoDoFollower.Drop, FamiliaDoFollower.Constructo, "#B87A4A", "#5AC8C8"),
            F("salamandra", "Salamandra", I, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Dragao, "#E8602A", "#F4C82A", 0.7f),
            F("espirito_lago", "Espírito do Lago", I, EfeitoDoFollower.Ouro, FamiliaDoFollower.Espirito, "#4AA8E8", "#C8F0FF", 0.8f),

            // Raros (8)
            F("cavaleira", "Cavaleira de Prata", R, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Humanoide, "#C8CCD8", "#D8B050"),
            F("feiticeira", "Feiticeira Rubra", R, EfeitoDoFollower.Critico, FamiliaDoFollower.Humanoide, "#A82A3A", "#F4C860"),
            F("robo_guardiao", "Robô Guardião", R, EfeitoDoFollower.Afk, FamiliaDoFollower.Constructo, "#5A6A8A", "#5AE8FF", 1.15f),
            F("lobo_gelido", "Lobo Gélido", R, EfeitoDoFollower.PontoFraco, FamiliaDoFollower.Besta, "#B8D8F0", "#5AA8E8", 1.1f),
            F("grifo", "Grifo", R, EfeitoDoFollower.Drop, FamiliaDoFollower.Mitico, "#C8963A", "#F4F0E8", 1.2f),
            F("dragonete", "Dragonete Verde", R, EfeitoDoFollower.Ouro, FamiliaDoFollower.Dragao, "#3A9A4A", "#E8D860", 0.85f),
            F("unicornio", "Unicórnio", R, EfeitoDoFollower.Afk, FamiliaDoFollower.Mitico, "#F4F0F8", "#C88AE8", 1.15f),
            F("sombra", "Sombra Errante", R, EfeitoDoFollower.Critico, FamiliaDoFollower.Espirito, "#3A2A5A", "#A85AE8"),

            // Épicos (5)
            F("draconato", "Draconato Guerreiro", E, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Dragao, "#8A2A2A", "#E8B040", 1.2f),
            F("fenix", "Fênix", E, EfeitoDoFollower.Afk, FamiliaDoFollower.Ave, "#F4602A", "#F4E02A", 1.3f),
            F("quimera", "Quimera", E, EfeitoDoFollower.Critico, FamiliaDoFollower.Mitico, "#B8782A", "#6A3A8A", 1.3f),
            F("tita_runico", "Titã Rúnico", E, EfeitoDoFollower.Drop, FamiliaDoFollower.Constructo, "#5A5A6A", "#5AE8A8", 1.4f),
            F("rainha_fada", "Rainha das Fadas", E, EfeitoDoFollower.Ouro, FamiliaDoFollower.Espirito, "#E88AD8", "#F4F0A0", 0.9f),

            // Lendários (3)
            F("dragao_ancestral", "Dragão Ancestral", L, EfeitoDoFollower.DanoExtra, FamiliaDoFollower.Dragao, "#2A2A3A", "#E84A2A", 1.6f),
            F("leviata", "Leviatã das Nuvens", L, EfeitoDoFollower.PontoFraco, FamiliaDoFollower.Mitico, "#4A8AE8", "#E8F4FF", 1.6f),
            F("arcanjo", "Arcanjo Dourado", L, EfeitoDoFollower.Ouro, FamiliaDoFollower.Humanoide, "#F4E8C8", "#F4C82A", 1.2f),
        };
    }
}
