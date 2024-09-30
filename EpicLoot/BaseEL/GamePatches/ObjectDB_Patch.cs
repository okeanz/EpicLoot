using HarmonyLib;

namespace EpicLoot.BaseEL.GamePatches
{
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
    public static class ObjectDB_CopyOtherDB_Patch
    {
        public static void Postfix()
        {
            EpicLootBase.TryRegisterItems();
            EpicLootBase.TryRegisterRecipes();
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class ObjectDB_Awake_Patch
    {
        public static void Postfix()
        {
            EpicLootBase.TryRegisterItems();
            EpicLootBase.TryRegisterRecipes();
        }
    }
}
