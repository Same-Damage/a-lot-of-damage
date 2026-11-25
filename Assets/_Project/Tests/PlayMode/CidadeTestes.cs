using System.Collections;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.Network;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace ALotOfDamage.Testes
{
    public class CidadeTestes
    {
        const ushort Porta = 7979;

        [UnityTearDown]
        public IEnumerator Fechar()
        {
            foreach (var n in Object.FindObjectsByType<NetworkManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                n.Shutdown();
                yield return null;
                yield return null;
                if (n) Object.Destroy(n.gameObject);
            }
            yield return null;
            SceneManager.LoadScene(FluxoDeEncontro.CenaDoMapa);
            yield return null;
        }

        static IEnumerator Abrir()
        {
            SceneManager.LoadScene(FluxoDeEncontro.CenaDaCidade);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Cidade_abre_vazia_sem_internet_e_volta_ao_mapa()
        {
            yield return Abrir();
            Assert.IsNotNull(Object.FindAnyObjectByType<CidadeOnline>());
            Assert.IsFalse(NetworkManager.Singleton.IsListening);
            Assert.IsEmpty(JogadorNaCidade.Presentes);
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(GameObject.Find("Jogador").transform.position.y, 0.5f, "o jogador pisa na praça");
            EscopoDoJogo.Instancia.Container.Resolve<IPortaoDaCidade>().VoltarAoMapa();
            yield return null;
            yield return null;
            Assert.AreEqual(FluxoDeEncontro.CenaDoMapa, SceneManager.GetActiveScene().name);
            Assert.IsNull(Object.FindAnyObjectByType<NetworkManager>(), "a rede não fica para trás no mapa");
        }

        // Dois jogadores de verdade na rede local: o que um escreve chega no outro
        [UnityTest]
        public IEnumerator Um_jogador_ve_o_outro_andar_e_sair()
        {
            yield return Abrir();
            var dono = NetworkManager.Singleton;
            dono.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", Porta);
            Assert.IsTrue(dono.StartHost());

            var go = new GameObject("Segundo jogador");
            var transporte = go.AddComponent<UnityTransport>();
            transporte.SetConnectionData("127.0.0.1", Porta);
            var outro = go.AddComponent<NetworkManager>();
            outro.NetworkConfig = new NetworkConfig { NetworkTransport = transporte, PlayerPrefab = dono.NetworkConfig.PlayerPrefab, EnableSceneManagement = false, TickRate = 10 };
            Assert.IsTrue(outro.StartClient());
            for (float t = 0; t < 8 && !(outro.IsConnectedClient && outro.LocalClient?.PlayerObject); t += Time.unscaledDeltaTime) yield return null;
            Assert.IsTrue(outro.IsConnectedClient);

            var dele = outro.LocalClient.PlayerObject.GetComponent<JogadorNaCidade>();
            dele.Escrever(new PresencaNaCidade { JogadorId = "b", Nome = "<b>Bia", Guilda = "Os Testes", HeroiId = "Mago", FollowerId = "pip" });
            dele.Escrever(new Vector3(3, 0, 4));
            JogadorNaCidade copia = null;
            for (float t = 0; t < 5; t += Time.unscaledDeltaTime)
            {
                copia = JogadorNaCidade.Presentes.Find(j => j.NetworkManager == dono && !j.EhMeu);
                if (copia && copia.Presenca().HeroiId == "Mago" && copia.Destino.x > 2) break;
                yield return null;
            }
            Assert.IsNotNull(copia);
            Assert.AreEqual("Mago", copia.Presenca().HeroiId);
            Assert.AreEqual("bBia", copia.Presenca().Nome);
            Assert.AreEqual(new Vector3(3, 0, 4), copia.Destino);
            yield return new WaitForSeconds(0.5f);
            Assert.IsNotNull(GameObject.Find("Visitante"));
            copia.Escrever(Vector3.one);
            Assert.AreEqual(new Vector3(3, 0, 4), copia.Destino, "ninguém move o jogador do outro");

            outro.Shutdown();
            for (float t = 0; t < 5 && JogadorNaCidade.Presentes.Exists(j => j && j.NetworkManager == dono && !j.EhMeu); t += Time.unscaledDeltaTime) yield return null;
            yield return null;
            Assert.IsFalse(JogadorNaCidade.Presentes.Exists(j => j && j.NetworkManager == dono && !j.EhMeu));
        }
    }
}
