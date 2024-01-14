﻿using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.BaseEL.MagicItemEffects
{
	[HarmonyPatch(typeof(SE_Rested), nameof(SE_Rested.CalculateComfortLevel), typeof(Player))]
	public class Comfortable_SE_Rested_CalculateComfortLevel_Patch
	{
		[UsedImplicitly]
		private static void Postfix(Player player, ref int __result)
		{
			var skillBonus = player.GetTotalActiveMagicEffectValue(MagicEffectType.Comfortable);
			__result += (int)skillBonus;
		}
	}
}