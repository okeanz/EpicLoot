﻿using HarmonyLib;

namespace EpicLoot.BaseEL.Healing
{
    public class AddHealingQueueToPlayerPatch
    {
        [HarmonyPatch(typeof(Player), nameof(Player.Start))]
        public static class Player_Start_Patch
        {
            private static void Postfix(Player __instance)
            {
                __instance.gameObject.AddComponent<HealingQueueMono>();
            }
        }
    }
}