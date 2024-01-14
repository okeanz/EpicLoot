using HarmonyLib;

namespace EpicLoot.BaseEL
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    public static class ZNetScene_Awake_Patch
    {
        public static bool Prefix(ZNetScene __instance)
        {
            EpicLootBase.TryRegisterPrefabs(__instance);
            return true;
        }
    }
}
