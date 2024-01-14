﻿using HarmonyLib;
using UnityEngine;

namespace EpicLoot.BaseEL.Adventure
{
    [HarmonyPatch(typeof(StoreGui))]
    public static class StoreGui_Patch
    {
        public static GameObject MerchantPanel;

        [HarmonyPatch("Show")]
        [HarmonyPostfix]
        public static void Show_Postfix(StoreGui __instance)
        {
            if (!EpicLootBase.IsAdventureModeEnabled() || __instance == null)
            {
                return;
            }

            if (__instance.m_trader.m_name != "$npc_haldor")
            {
                //Adds compatibility for other mods that may add other trader NPC's that are not Haldor.
                return;
            }

            if (__instance.transform.Find(nameof(MerchantPanel)) == null)
            {
                if (MerchantPanel != null)
                {
                    Object.Destroy(MerchantPanel);
                }

                MerchantPanel = (GameObject)Object.Instantiate((Object) EpicLootBase.Assets.MerchantPanel, __instance.transform, false);
                MerchantPanel.AddComponent<MerchantPanel>();
            }

            MerchantPanel.gameObject.SetActive(true);
        }

        [HarmonyPatch("Hide")]
        [HarmonyPostfix]
        public static void Hide(StoreGui __instance)
        {
            if (MerchantPanel == null)
            {
                return;
            }

            MerchantPanel.SetActive(false);
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        public static void OnDestroy(StoreGui __instance)
        {
            MerchantPanel = null;
        }
    }
}
