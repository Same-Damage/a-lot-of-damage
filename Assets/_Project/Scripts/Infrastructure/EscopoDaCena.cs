using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ALotOfDamage.Infrastructure
{
    // Um por cena: injeta os serviços do EscopoDoJogo em todos os MonoBehaviours da cena com [Inject].
    // Roda antes dos outros scripts para os Start() já encontrarem as dependências.
    [DefaultExecutionOrder(-5000)]
    public class EscopoDaCena : LifetimeScope
    {
        protected override LifetimeScope FindParent() => EscopoDoJogo.Instancia;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(container =>
            {
                foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (mb != this && mb.gameObject.scene == gameObject.scene && EhDoJogo(mb)) container.Inject(mb);
            });
        }

        static bool EhDoJogo(MonoBehaviour mb) => mb.GetType().Namespace?.StartsWith("ALotOfDamage") == true;
    }
}
