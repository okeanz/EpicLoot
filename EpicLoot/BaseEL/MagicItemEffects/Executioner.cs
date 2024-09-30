﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using EpicLoot.BaseEL.Common;
using EpicLoot.BaseEL.GamePatches;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLoot.BaseEL.MagicItemEffects
{
	[HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
	public class ExecutionerProjectileHit_Projectile_OnHit_Patch
	{
		[UsedImplicitly]
		private static void Prefix(Projectile __instance)
		{
			if (__instance != null && __instance.m_nview != null && __instance.m_nview.GetZDO() is ZDO zdo)
			{
				ExecutionerCheckDamage_Character_Damage_Patch.ExecutionerMultiplier = zdo.GetFloat("epic loot executioner multiplier", 1f);
			}
		}

		[UsedImplicitly]
		private static void Postfix() => ExecutionerCheckDamage_Character_Damage_Patch.ExecutionerMultiplier = null;
	}

	[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
	[HarmonyPriority(Priority.VeryLow)]
	public class ExecutionerCheckDamage_Character_Damage_Patch
	{
		public static float? ExecutionerMultiplier;

        [UsedImplicitly]
        private static void Prefix(Character __instance, HitData hit)
        {
            if (__instance == null || hit == null || hit.GetAttacker() == null || !hit.GetAttacker().IsPlayer())
            {
                ExecutionerMultiplier = null;
                return;
            }

            var player = (Player)hit.GetAttacker();
            var znetView = __instance.GetComponent<ZNetView>();
            if (znetView == null)
                return;

            var zdo = znetView.GetZDO();
            if (zdo == null)
                return;

			if (zdo.GetBool("epic loot executioner flag " + player.GetZDO().m_uid))
			{
				return;
			}

			if (ExecutionerMultiplier == null)
			{
				ExecutionerMultiplier = ReadExecutionerValue(player);
			}

			if (ExecutionerMultiplier is float multiplier && __instance.GetHealth() / __instance.GetMaxHealth() < 0.2f)
			{
				hit.m_damage.Modify(multiplier);
				__instance.GetComponent<ZNetView>().GetZDO().Set("epic loot executioner flag " + player.GetZDO().m_uid, true);
			}
            
			ExecutionerMultiplier = null;
		}

		public static float ReadExecutionerValue(Player player)
        {
            float totalMagicEffect;
            if (Attack_Patch.ActiveAttack != null)
                totalMagicEffect = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, Attack_Patch.ActiveAttack.m_weapon, MagicEffectType.Executioner, 0.01f);
            else
                totalMagicEffect = player.GetTotalActiveMagicEffectValue(MagicEffectType.Executioner, 0.01f);
            return 1 + totalMagicEffect;
		}
	}

	[HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
	public class ExecutionerProjectileInstantiation_Attack_FireProjectileBurst_Patch
	{
		private static GameObject MarkAttackProjectile(GameObject attackProjectile, Attack attack)
		{
            if (attack != null && attackProjectile != null && attack.m_character == Player.m_localPlayer)
            {
                var znetView = attackProjectile.GetComponent<ZNetView>();
                if (znetView != null && znetView.GetZDO() != null)
                {
                    znetView.GetZDO().Set("epic loot executioner multiplier", ExecutionerCheckDamage_Character_Damage_Patch.ReadExecutionerValue(Player.m_localPlayer));
                }
            }

			return attackProjectile;
		}

		private static readonly MethodInfo AttackProjectileMarker = AccessTools.DeclaredMethod(typeof(ExecutionerProjectileInstantiation_Attack_FireProjectileBurst_Patch), nameof(MarkAttackProjectile));
		private static readonly MethodInfo Instantiator = AccessTools.GetDeclaredMethods(typeof(Object)).Where(m => m.Name == "Instantiate" && m.GetGenericArguments().Length == 1).Select(m => m.MakeGenericMethod(typeof(GameObject))).First(m => m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType == typeof(Vector3));

		[UsedImplicitly]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if (instruction.opcode == OpCodes.Call && instruction.OperandIs(Instantiator))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0); // this
					yield return new CodeInstruction(OpCodes.Call, AttackProjectileMarker);
				}
			}
		}
	}
}