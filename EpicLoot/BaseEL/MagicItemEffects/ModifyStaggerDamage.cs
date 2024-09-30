﻿using EpicLoot.BaseEL.GamePatches;
using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.BaseEL.MagicItemEffects
{
	[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
	public class ModifyStaggerDamage_Character_Damage_Patch
	{
		public static float? HandlingProjectileDamage;
		
		[UsedImplicitly]
		private static void Prefix(Character __instance, HitData hit)
		{
			var attacker = hit.GetAttacker();
			if (attacker is Player player && __instance.IsStaggering())
			{
				if (HandlingProjectileDamage == null)
				{
					HandlingProjectileDamage = ReadStaggerDamageValue(player);
				}
				hit.ApplyModifier((float)HandlingProjectileDamage);
			}
		}

		public static float ReadStaggerDamageValue(Player player)
		{
			if (Attack_Patch.ActiveAttack != null)
				return 1 + MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, Attack_Patch.ActiveAttack.m_weapon, MagicEffectType.ModifyStaggerDamage, 0.01f);
			else
                return 1 + player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyStaggerDamage, 0.01f);
		}
	}

	[HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
	public class ModifyStaggerDamageProjectileHit_Projectile_OnHit_Patch
	{
		[UsedImplicitly]
		private static void Prefix(Projectile __instance)
        {
            if (__instance == null || __instance.m_nview == null)
            {
                ModifyStaggerDamage_Character_Damage_Patch.HandlingProjectileDamage = null;
                return;
            }

            ModifyStaggerDamage_Character_Damage_Patch.HandlingProjectileDamage = __instance.m_nview.GetZDO()?.GetFloat("epic loot modify stagger damage", 1f);
        }

        [UsedImplicitly]
		private static void Postfix()
        {
            ModifyStaggerDamage_Character_Damage_Patch.HandlingProjectileDamage = null;
        }
    }

	[HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
	public class ModifyStaggerDamage_Projectile_Setup_Patch
	{
		[UsedImplicitly]
		private static void Prefix(Character owner, Projectile __instance)
		{
			if (owner != null && owner.IsPlayer() && __instance != null && __instance.m_nview != null)
			{
				__instance.m_nview.GetZDO().Set("epic loot modify stagger damage", ModifyStaggerDamage_Character_Damage_Patch.ReadStaggerDamageValue((Player)owner));
			}
		}
	}
}