using System.Collections;
using ALotOfDamage.Combat;
using ALotOfDamage.Core;
using ALotOfDamage.Infrastructure;
using ALotOfDamage.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace ALotOfDamage.Testes
{
    // Fluxos que precisam das cenas: composição (VContainer) e ida e volta da batalha
    public class FluxoDoMapaTestes
    {
        static IEnumerator Carregar(string cena)
        {
            SceneManager.LoadScene(cena);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Mapa_abre_com_o_escopo_do_jogo_montado()
        {
            yield return Carregar(FluxoDeEncontro.CenaDoMapa);

            Assert.IsNotNull(EscopoDoJogo.Instancia);
            Assert.IsNotNull(EscopoDoJogo.Instancia.Container.Resolve<IRepositorioDeProgresso>());
            Assert.IsNotNull(Object.FindAnyObjectByType<AtaqueNoMapa>());
        }

        [UnityTest]
        public IEnumerator Follower_acompanha_o_jogador_no_mapa()
        {
            yield return Carregar(FluxoDeEncontro.CenaDoMapa);
            var follower = Object.FindAnyObjectByType<SeguidorNoMapa>();
            Assert.IsNotNull(follower);

            // Alvo de teste longe do ponto inicial (o jogador tem CharacterController e não pode ser teletransportado assim)
            follower.alvo = new GameObject("alvo de teste").transform;
            follower.alvo.position = follower.transform.position + new Vector3(10, 0, 0);
            float limite = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < limite) yield return null;

            Vector3 d = follower.transform.position - follower.alvo.position;
            d.y = 0;
            Assert.Less(d.magnitude, follower.distancia + 0.5f);
        }

        [UnityTest]
        public IEnumerator Atacar_monstro_leva_para_a_batalha_e_a_vitoria_volta_ao_mapa()
        {
            yield return Carregar(FluxoDeEncontro.CenaDoMapa);
            var fluxo = EscopoDoJogo.Instancia.Container.Resolve<FluxoDeEncontro>();
            var repositorio = EscopoDoJogo.Instancia.Container.Resolve<IRepositorioDeProgresso>();
            double ouroAntes = repositorio.Carregar().Ouro;

            var monstro = System.Array.Find(Object.FindObjectsByType<InimigoDoMapa>(FindObjectsSortMode.None), m => m.tipo == "slime");
            monstro.Enfrentar(Vantagem.Preventivo);
            yield return null;
            yield return null;

            Assert.AreEqual(FluxoDeEncontro.CenaDaBatalha, SceneManager.GetActiveScene().name);
            Assert.IsTrue(fluxo.Ativo);
            Assert.IsNotNull(Object.FindAnyObjectByType<CombateClicker>());

            // Slime tem pouca vida: o golpe preventivo e o auto-ataque vencem sozinhos
            float limite = Time.realtimeSinceStartup + 30f;
            while (fluxo.Ativo && Time.realtimeSinceStartup < limite) yield return null;
            yield return null;

            Assert.IsFalse(fluxo.Ativo, "a batalha não terminou sozinha em 30 s");
            Assert.AreEqual(FluxoDeEncontro.CenaDoMapa, SceneManager.GetActiveScene().name);
            Assert.Greater(repositorio.Carregar().Ouro, ouroAntes);
        }
    }
}
