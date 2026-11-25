using System.Collections.Generic;
using ALotOfDamage.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ALotOfDamage.Network
{
    // Um jogador na sala da cidade: só dados, sem visual. Cada um escreve os próprios; a CidadeOnline desenha os outros
    public class JogadorNaCidade : NetworkBehaviour
    {
        public static readonly List<JogadorNaCidade> Presentes = new List<JogadorNaCidade>();

        static NetworkVariable<T> Meu<T>() => new NetworkVariable<T>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        readonly NetworkVariable<Vector3> destino = Meu<Vector3>();
        readonly NetworkVariable<FixedString64Bytes> jogadorId = Meu<FixedString64Bytes>();
        readonly NetworkVariable<FixedString64Bytes> nome = Meu<FixedString64Bytes>();
        readonly NetworkVariable<FixedString64Bytes> guilda = Meu<FixedString64Bytes>();
        readonly NetworkVariable<FixedString32Bytes> heroi = Meu<FixedString32Bytes>();
        readonly NetworkVariable<FixedString64Bytes> follower = Meu<FixedString64Bytes>();

        public bool EhMeu => IsOwner;
        public Vector3 Destino => destino.Value;

        public override void OnNetworkSpawn() => Presentes.Add(this);
        public override void OnNetworkDespawn() => Presentes.Remove(this);
        public override void OnDestroy()
        {
            Presentes.Remove(this);
            base.OnDestroy();
        }

        public PresencaNaCidade Presenca() => new PresencaNaCidade
        {
            JogadorId = jogadorId.Value.ToString(), Nome = RegrasDaCidade.NomeParaMostrar(nome.Value.ToString()), Guilda = RegrasDaCidade.NomeParaMostrar(guilda.Value.ToString()),
            HeroiId = heroi.Value.ToString(), FollowerId = follower.Value.ToString(),
        };

        public void Escrever(Vector3 posicao)
        {
            if (IsSpawned && IsOwner) destino.Value = posicao;
        }

        public void Escrever(PresencaNaCidade p)
        {
            if (!IsSpawned || !IsOwner) return;
            Definir(jogadorId, p.JogadorId);
            Definir(nome, p.Nome);
            Definir(guilda, p.Guilda);
            Definir(follower, p.FollowerId);
            var h = new FixedString32Bytes();
            h.CopyFromTruncated(p.HeroiId ?? "");
            if (!heroi.Value.Equals(h)) heroi.Value = h;
        }

        static void Definir(NetworkVariable<FixedString64Bytes> v, string texto)
        {
            var f = new FixedString64Bytes();
            f.CopyFromTruncated(texto ?? "");
            if (!v.Value.Equals(f)) v.Value = f;
        }
    }
}
