using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ALotOfDamage.Testes.EditMode
{
    // Herói com cabeça própria: <nome>_Malha.asset troca a malha do FBX e usa o esqueleto dele
    public class MalhaComCabecaTestes
    {
        const string Pasta = "Assets/_Project/Art/Characters/Ladino/";

        [Test]
        public void Malha_do_ladino_usa_o_esqueleto_do_modelo_e_separa_corpo_e_cabeca()
        {
            var malha = AssetDatabase.LoadAssetAtPath<Mesh>(Pasta + "Ladino_Malha.asset");
            var modelo = AssetDatabase.LoadAssetAtPath<GameObject>(Pasta + "Ladino.fbx").GetComponentInChildren<SkinnedMeshRenderer>();

            Assert.IsNotNull(malha);
            Assert.AreEqual(2, malha.subMeshCount);
            CollectionAssert.AreEqual(modelo.sharedMesh.bindposes, malha.bindposes);
            foreach (var p in malha.boneWeights)
                Assert.AreEqual(1, p.weight0 + p.weight1 + p.weight2 + p.weight3, 0.01f);
        }
    }
}
