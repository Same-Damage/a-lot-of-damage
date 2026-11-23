using UnityEngine;

namespace ALotOfDamage.Core
{
    // Base dos assets de balanceamento: cada um mora em Data/Resources com o nome da própria classe
    public abstract class Balanceamento<T> : ScriptableObject where T : Balanceamento<T>
    {
        static T carregado;

        public static T Atual
        {
            get
            {
                if (carregado) return carregado;
                carregado = Resources.Load<T>(typeof(T).Name);
                if (!carregado) carregado = CreateInstance<T>();
                return carregado;
            }
        }
    }
}
