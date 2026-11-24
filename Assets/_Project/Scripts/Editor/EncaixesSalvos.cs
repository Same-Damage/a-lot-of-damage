using System.Collections.Generic;
using System.IO;
using ALotOfDamage.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ALotOfDamage.Ferramentas
{
    // Encaixe das peças ajustado à mão no editor: posição, rotação e escala de cada peça relativas ao osso,
    // guardadas por herói e peça em Art/Equipment/encaixes.json. O ConstrutorDePersonagens3D usa estes valores
    // quando existem; a conta automática da tabela Equipamento fica só para peças ainda sem ajuste.
    public static class EncaixesSalvos
    {
        const string Caminho = "Assets/_Project/Art/Equipment/encaixes.json";

        [System.Serializable]
        public class Encaixe
        {
            public string Heroi, Peca;
            public Vector3 Posicao, Escala;
            public Quaternion Rotacao;
        }

        [System.Serializable]
        class Arquivo { public List<Encaixe> Encaixes = new(); }

        // Grava o encaixe de todas as peças das cenas abertas (fora do Play), mantendo as de heróis que não estão nelas
        [MenuItem("A Lot of Damage/Salvar encaixes das peças")]
        public static void Salvar()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Salvar encaixes", "Saia do Play antes: o que muda no Play se perde ao sair.", "Ok");
                return;
            }
            var porChave = new Dictionary<string, Encaixe>();
            foreach (var e in Ler().Encaixes) porChave[e.Heroi + "/" + e.Peca] = e;

            int salvos = 0;
            foreach (var peca in Object.FindObjectsByType<PecaDeEquipamento>(FindObjectsInactive.Include))
            {
                var heroi = peca.GetComponentInParent<Animator>(true);
                if (!heroi) continue;
                var t = peca.transform;
                porChave[heroi.name + "/" + peca.name] = new Encaixe
                {
                    Heroi = heroi.name, Peca = peca.name,
                    Posicao = t.localPosition, Rotacao = t.localRotation, Escala = t.localScale,
                };
                salvos++;
            }

            var arquivo = new Arquivo { Encaixes = new List<Encaixe>(porChave.Values) };
            arquivo.Encaixes.Sort((a, b) => string.CompareOrdinal(a.Heroi + a.Peca, b.Heroi + b.Peca));
            File.WriteAllText(Caminho, JsonUtility.ToJson(arquivo, true));
            AssetDatabase.ImportAsset(Caminho);
            // a cena aberta também guarda o ajuste, para não sumir ao fechar sem salvar
            EditorSceneManager.SaveOpenScenes();
            EditorUtility.DisplayDialog("Salvar encaixes", $"{salvos} peça(s) salvas em {Caminho}.", "Ok");
        }

        public static Encaixe Buscar(string heroi, string peca)
        {
            foreach (var e in Ler().Encaixes) if (e.Heroi == heroi && e.Peca == peca) return e;
            return null;
        }

        static Arquivo Ler() =>
            File.Exists(Caminho) ? JsonUtility.FromJson<Arquivo>(File.ReadAllText(Caminho)) ?? new Arquivo() : new Arquivo();
    }
}
