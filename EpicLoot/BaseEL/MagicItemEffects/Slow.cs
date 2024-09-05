﻿using System;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace EpicLoot.BaseEL.MagicItemEffects
{
	public class Slow : MonoBehaviour
    {
        public const string RPCKey = "epic loot slow";

		public float Multiplier;
		public float TimeToLive;

		private void Start()
		{
			var character = GetComponent<Character>();

			character.m_acceleration *= Multiplier;
			character.m_runSpeed *= Multiplier;
			character.m_flyFastSpeed *= Multiplier;
			character.m_swimSpeed *= Multiplier;
		}

		private void FixedUpdate()
		{
			TimeToLive -= Time.fixedDeltaTime;
			if (TimeToLive > 0)
			{
				return;
			}

			var character = GetComponent<Character>();

			character.m_acceleration /= Multiplier;
			character.m_runSpeed /= Multiplier;
			character.m_flyFastSpeed /= Multiplier;
			character.m_swimSpeed /= Multiplier;

			Destroy(this);
		}
	}

	[HarmonyPatch(typeof(Character), nameof(Character.Awake))]
	public static class SlowAddRPC_Character_Awake_Patch
	{
		[UsedImplicitly]
		private static void Postfix(Character __instance) => __instance.m_nview.Register<float>(Slow.RPCKey, (s, multiplier) => RPC_Slow(__instance, multiplier));

		private static void RPC_Slow(Character character, float multiplier)
		{
			if (!(character.GetComponent<Slow>() is Slow slow))
			{
				slow = character.gameObject.AddComponent<Slow>();
				slow.Multiplier = multiplier;
			}
			
			slow.TimeToLive = 2;
		}
	}

	[HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
	public static class ApplySlow_Character_RPC_Damage_Patch
	{
		[UsedImplicitly]
		private static void Postfix(Character __instance, HitData hit)
		{
			if (!__instance.IsBoss() && hit.GetAttacker() is Player player && player.HasActiveMagicEffect(MagicEffectType.Slow))
			{
                var slowMultiplier = 1 - player.GetTotalActiveMagicEffectValue(MagicEffectType.Slow, 0.01f);
                if (!Mathf.Approximately(slowMultiplier, 1))
                {
                    __instance.m_nview.InvokeRPC(ZRoutedRpc.Everybody, Slow.RPCKey, slowMultiplier);
                }
            }
		}
	}

	[HarmonyPatch(typeof(Game), nameof(Game.Awake))]
	public static class ModifyEnemyAttackSpeed_AnimationHandler_Patch
	{
		public static double ModifyAttackSpeed(Character character, double speed)
		{
			if (character.InAttack() && character.GetComponent<Slow>()?.Multiplier is { } slowMultiplier)
			{
				if (speed > 0.001f && (speed * 1e4f % 10 > 3 || speed * 1e4f % 10 < 1))
				{
					speed = (float) Math.Round(speed * slowMultiplier, 3) + speed % 1e-4f + 2e-4f;
				}
			}

			return speed;
		}
		
		[UsedImplicitly]
		private static void Postfix(Game __instance)
		{
			AnimationSpeedManager.Add((character, speed) => ModifyAttackSpeed(character,speed));
		}
	}
}