using System.Collections.Generic;
using UnityEngine;

namespace ALotOfDamage.Visual
{
    // Os heróis 3D montados na cena (um por pasta de Art/Characters); só o escolhido fica ativo
    public class HeroisDaCena : MonoBehaviour
    {
        public PersonagemVisual[] herois;

        string[] nomes;

        public IReadOnlyList<string> Nomes
        {
            get
            {
                if (nomes != null) return nomes;
                nomes = new string[herois.Length];
                for (int i = 0; i < herois.Length; i++) nomes[i] = herois[i].name;
                return nomes;
            }
        }

        // Liga o herói com esse nome e desliga os outros; nome desconhecido liga o primeiro
        public PersonagemVisual Mostrar(string nome)
        {
            int escolhido = 0;
            for (int i = 0; i < herois.Length; i++) if (herois[i].name == nome) escolhido = i;
            for (int i = 0; i < herois.Length; i++) herois[i].gameObject.SetActive(i == escolhido);
            return herois[escolhido];
        }
    }
}
