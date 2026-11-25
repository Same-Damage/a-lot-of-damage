using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Leaderboards.Model;

namespace ALotOfDamage.Nuvem
{
    public class PosicoesDaNuvem : IPosicoesDaArena
    {
        public const string Ranking = "arena";

        readonly IExecutionContext ctx;
        readonly IGameApiClient api;

        public PosicoesDaNuvem(IExecutionContext ctx, IGameApiClient api)
        {
            this.ctx = ctx;
            this.api = api;
        }

        public Task Pontuar(double pontos) =>
            api.Leaderboards.AddLeaderboardPlayerScoreAsync(ctx, ctx.ServiceToken, Guid.Parse(ctx.ProjectId), Ranking, ctx.PlayerId, new AddLeaderboardScore(pontos));

        public async Task<List<LinhaDoRanking>> Perto(int alcance)
        {
            var r = await api.Leaderboards.GetLeaderboardScoresPlayerRangeAsync(ctx, ctx.ServiceToken, Guid.Parse(ctx.ProjectId), Ranking, ctx.PlayerId, null, alcance);
            var lista = new List<LinhaDoRanking>();
            foreach (var e in r.Data.Results) lista.Add(new LinhaDoRanking { Posicao = e.Rank + 1, JogadorId = e.PlayerId, Nome = e.PlayerName, Pontos = e.Score });
            return lista;
        }
    }
}
