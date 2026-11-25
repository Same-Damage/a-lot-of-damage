using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Shared;
using Unity.Services.CloudSave.Model;

namespace ALotOfDamage.Nuvem
{
    public static class Armazens
    {
        public const string PrefixoDoJogador = "jogador:";
        // dados protegidos de outro jogador (só o servidor chega neles)
        public static string DoJogador(string id) => PrefixoDoJogador + id;
    }

    // Cloud Save: dono vazio são os dados protegidos do jogador; os outros são itens privados, que só o servidor lê e grava
    public class ArmazemDaNuvem : IArmazem
    {
        readonly IExecutionContext ctx;
        readonly IGameApiClient api;

        public ArmazemDaNuvem(IExecutionContext ctx, IGameApiClient api)
        {
            this.ctx = ctx;
            this.api = api;
        }

        bool DoJogador(string dono, out string jogador)
        {
            jogador = dono == "" ? ctx.PlayerId : dono.StartsWith(Armazens.PrefixoDoJogador) ? dono.Substring(Armazens.PrefixoDoJogador.Length) : null;
            return jogador != null;
        }

        async Task<List<Item>> Itens(string dono, List<string> chaves)
        {
            var itens = new List<Item>();
            string depois = null;
            do
            {
                var r = DoJogador(dono, out var jogador)
                    ? await api.CloudSaveData.GetProtectedItemsAsync(ctx, ctx.ServiceToken, ctx.ProjectId, jogador, chaves, depois)
                    : await api.CloudSaveData.GetPrivateCustomItemsAsync(ctx, ctx.ServiceToken, ctx.ProjectId, dono, chaves, depois);
                itens.AddRange(r.Data.Results);
                // com as chaves informadas, cabe tudo em poucas páginas; a próxima começa na última chave lida
                depois = string.IsNullOrEmpty(r.Data.Links?.Next) || r.Data.Results.Count == 0 ? null : r.Data.Results[r.Data.Results.Count - 1].Key;
            }
            while (depois != null);
            return itens;
        }

        public async Task<(string texto, string trava)> Ler(string dono, string chave)
        {
            foreach (var i in await Itens(dono, new List<string> { chave })) if (i.Key == chave) return (i.Value?.ToString(), i.WriteLock);
            return (null, null);
        }

        public async Task<Dictionary<string, string>> LerVarios(string dono, List<string> chaves)
        {
            var r = new Dictionary<string, string>();
            if (chaves.Count == 0) return r;
            foreach (var i in await Itens(dono, chaves)) r[i.Key] = i.Value?.ToString();
            return r;
        }

        public async Task<bool> Gravar(string dono, string chave, string texto, string trava)
        {
            var corpo = new SetItemBody(chave, texto, trava);
            try
            {
                if (DoJogador(dono, out var jogador)) await api.CloudSaveData.SetProtectedItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, jogador, corpo);
                else await api.CloudSaveData.SetPrivateCustomItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, dono, corpo);
                return true;
            }
            catch (ApiException e) when (e.Response != null && e.Response.StatusCode == HttpStatusCode.Conflict) { return false; }
        }

        public async Task Apagar(string dono, string chave)
        {
            try
            {
                if (DoJogador(dono, out var jogador)) await api.CloudSaveData.DeleteProtectedItemAsync(ctx, ctx.ServiceToken, chave, ctx.ProjectId, jogador);
                else await api.CloudSaveData.DeletePrivateCustomItemAsync(ctx, ctx.ServiceToken, ctx.ProjectId, dono, chave);
            }
            catch (ApiException e) when (e.Response != null && e.Response.StatusCode == HttpStatusCode.NotFound) { }
        }
    }
}
