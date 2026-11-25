using System.Collections.Generic;
using ALotOfDamage.Core;
using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // As 12 relíquias e o custo em fragmentos de chefe. O asset fica em Data/Resources/BalanceamentoDasReliquias.
    [CreateAssetMenu(menuName = "A Lot of Damage/Balanceamento das Relíquias")]
    public class BalanceamentoDasReliquias : Balanceamento<BalanceamentoDasReliquias>
    {
        public ParametrosDasReliquias parametros = new ParametrosDasReliquias();
        public List<DefinicaoDeReliquia> reliquias = Padrao();

        static DefinicaoDeReliquia R(string id, string nome, string descricao, AlvoDaReliquia alvo, double bonus, int custo) =>
            new DefinicaoDeReliquia { Id = id, Nome = nome, Descricao = descricao, Alvo = alvo, BonusPorNivel = bonus, CustoParaMontar = custo };

        public static List<DefinicaoDeReliquia> Padrao() => new List<DefinicaoDeReliquia>
        {
            R("manopla_tita", "Manopla do Titã", "+100% de dano do click por nível", AlvoDaReliquia.Click, 1, 15),
            R("coroa_rei_caido", "Coroa do Rei Caído", "+50% de dano do click por nível", AlvoDaReliquia.Click, 0.5, 30),
            R("engrenagem_eterna", "Engrenagem Eterna", "+100% de dano do auto-ataque por nível", AlvoDaReliquia.Automatico, 1, 15),
            R("relogio_parado", "Relógio Parado", "+50% de dano do auto-ataque por nível", AlvoDaReliquia.Automatico, 0.5, 30),
            R("apito_bestial", "Apito Bestial", "+100% de dano do follower por nível", AlvoDaReliquia.Follower, 1, 20),
            R("colar_vinculo", "Colar do Vínculo", "+50% de dano do follower por nível", AlvoDaReliquia.Follower, 0.5, 35),
            R("calice_dourado", "Cálice Dourado", "+25% de ouro por nível", AlvoDaReliquia.Ouro, 0.25, 20),
            R("ampulheta_sonho", "Ampulheta do Sonho", "+15% de AFK por nível", AlvoDaReliquia.Afk, 0.15, 25),
            R("olho_falcao", "Olho de Falcão", "+2% de chance crítica por nível", AlvoDaReliquia.Critico, 0.02, 25),
            R("trevo_ancestral", "Trevo Ancestral", "+15% de chance de item por nível", AlvoDaReliquia.Drop, 0.15, 25),
            R("lente_runica", "Lente Rúnica", "+20% de duração do ponto fraco por nível", AlvoDaReliquia.PontoFraco, 0.2, 20),
            R("estrela_cadente", "Estrela Cadente", "+25% de dano do click por nível", AlvoDaReliquia.Click, 0.25, 10),
        };
    }
}
