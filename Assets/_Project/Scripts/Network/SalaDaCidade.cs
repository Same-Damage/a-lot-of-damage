using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ALotOfDamage.Core;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace ALotOfDamage.Network
{
    // Sala da cidade nos serviços da Unity: entra numa com vaga ou abre outra. O controle é dividido entre os jogadores
    // (Distributed Authority), então a sala continua quando alguém sai
    public class SalaDaCidade
    {
        const string Tipo = "cidade";

        ISession sessao;
        bool cancelada;

        public string Id => sessao?.Id ?? "";

        public async Task Entrar()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized) throw new FalhaOnline("Sem conexão com a cidade.");
            try
            {
                var opcoes = new SessionOptions
                {
                    MaxPlayers = RegrasDaCidade.MaximoPorSala,
                    SessionProperties = new Dictionary<string, SessionProperty> { ["tipo"] = new SessionProperty(Tipo, VisibilityPropertyOptions.Public, PropertyIndex.String1) },
                }.WithDistributedAuthorityNetwork();
                var busca = new QuickJoinOptions { CreateSession = true, Timeout = TimeSpan.FromSeconds(5) };
                busca.Filters.Add(new FilterOption(FilterField.StringIndex1, Tipo, FilterOperation.Equal));
                busca.Filters.Add(new FilterOption(FilterField.AvailableSlots, "0", FilterOperation.Greater));
                sessao = await MultiplayerService.Instance.MatchmakeSessionAsync(busca, opcoes);
            }
            catch (Exception e) when (!(e is FalhaOnline))
            {
                Debug.LogWarning($"Cidade: {e.GetType().Name}");
                throw new FalhaOnline("A cidade está indisponível.", e);
            }
            // saiu da cidade enquanto entrava
            if (cancelada) await Sair();
        }

        public async Task Sair()
        {
            cancelada = true;
            var s = sessao;
            sessao = null;
            if (s != null)
            {
                try { await s.LeaveAsync(); }
                catch (Exception e) { Debug.LogWarning($"Cidade: saída {e.GetType().Name}"); }
            }
            var rede = NetworkManager.Singleton;
            if (rede && rede.IsListening) rede.Shutdown();
        }
    }
}
