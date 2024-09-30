﻿using HarmonyLib;

namespace EpicLoot.BaseEL.MagicItemEffects
{
    [HarmonyPatch]
    public static class MultiShot
    {
        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        [HarmonyPrefix]
        public static void Attack_FireProjectileBurst_Prefix(Attack __instance)
        {
            if (__instance?.GetWeapon() == null || __instance.m_character == null || !__instance.m_character.IsPlayer())
                return;

            var weaponDamage = __instance.GetWeapon()?.GetDamage();
            if (!weaponDamage.HasValue)
                return;

            var player = (Player)__instance.m_character;
            var doubleShot = player.HasActiveMagicEffect(MagicEffectType.DoubleMagicShot);
            var tripleShot = player.HasActiveMagicEffect(MagicEffectType.TripleBowShot);

            
            if (tripleShot)
            {
                // The accuracy on the fireball staff is 1, so the projectiles appear right on top of each other,
                // this forces them to appear distinct and still feels good (greater AOE in lieu of accuracy)
                if (__instance.m_projectileAccuracy < 2)
                    __instance.m_projectileAccuracy = 2;
                __instance.m_projectiles = 3;
            }
            else if (doubleShot)
            {
                if (__instance.m_projectileAccuracy < 2)
                    __instance.m_projectileAccuracy = 2;
                __instance.m_projectiles = 2;
            }
        }
    }
}
