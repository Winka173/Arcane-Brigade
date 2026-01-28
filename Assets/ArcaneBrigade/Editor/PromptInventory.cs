using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class PromptInventory
    {
        [Serializable] private class Inventory { public UnitDefinition[] Champions, Companions; public WeaponDefinition[] Weapons; public ItemDefinition[] Items; public BondDefinition[] Bonds; }
        public static void Export()
        {
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog"); Directory.CreateDirectory("Library/PromptReference");
            // Unity object references need individual serialization, not reference IDs.
            string Rows<T>(T[] rows) => "[" + string.Join(",", Array.ConvertAll(rows, row => JsonUtility.ToJson(row))) + "]";
            File.WriteAllText("Library/PromptReference/Inventory.json", "{\"Champions\":" + Rows(c.Champions) + ",\"Companions\":" + Rows(c.Companions) + ",\"Weapons\":" + Rows(c.Weapons) + ",\"Items\":" + Rows(c.Items) + ",\"Bonds\":" + Rows(c.Bonds) + "}");
            Debug.Log("PROMPT_INVENTORY " + c.Champions.Length + "/" + c.Companions.Length + "/" + c.Items.Length + "/" + c.Bonds.Length);
        }
    }
}
