using ALotOfDamage.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ALotOfDamage.Infrastructure
{
    // Raiz da composição: serviços que vivem o jogo inteiro (progresso, catálogo, encontro, casos de uso).
    // Criada antes da primeira cena e mantida entre as trocas de cena.
    public class EscopoDoJogo : LifetimeScope
    {
        public static EscopoDoJogo Instancia { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CriarAoIniciar()
        {
            if (Instancia) return;
            var go = new GameObject(nameof(EscopoDoJogo));
            DontDestroyOnLoad(go);
            Instancia = go.AddComponent<EscopoDoJogo>();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IRepositorioDeProgresso, RepositorioDeProgressoEmMemoria>(Lifetime.Singleton);
            builder.Register<ICatalogo, CatalogoDeBalanceamento>(Lifetime.Singleton);
            builder.Register<ISorteio, SorteioDaUnity>(Lifetime.Singleton);
            builder.Register<FluxoDeEncontro>(Lifetime.Singleton);

            builder.Register<ComprarUpgrade>(Lifetime.Singleton);
            builder.Register<ForjarItem>(Lifetime.Singleton);
            builder.Register<RegistrarVitoria>(Lifetime.Singleton);
            builder.Register<CalcularAtributos>(Lifetime.Singleton);
        }
    }
}
