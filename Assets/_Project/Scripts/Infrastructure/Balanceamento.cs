using UnityEngine;

namespace ALotOfDamage.Infrastructure
{
    // Base dos assets de balanceamento: cada um mora em Data/Resources com o nome da própria classe
    public abstract class Balanceamento<T> : ScriptableObject where T : Balanceamento<T>
    {
        public static T Carregar()
        {
            var asset = Resources.Load<T>(typeof(T).Name);
            return asset ? asset : CreateInstance<T>();
        }
    }
}
