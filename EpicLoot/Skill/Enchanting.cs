using System;
using System.Collections.Generic;
using EpicLoot.BaseEL;
using EpicLoot.BaseEL.Crafting;
using EpicLoot_UnityLib;
using Jotunn.Configs;
using Jotunn.Managers;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace EpicLoot.Skill;

public static class Enchanting
{
    public static EnchantingSkillConfig Config;

    public static Skills.SkillType SkillType;

    public static void AddEnchantingSkill()
    {
        var item = PrefabManager.Instance.GetPrefab("RunestoneMythic");
        if (item == null)
            Logger.LogError("RunestoneMythic prefab not exists");

        var config = new SkillConfig
        {
            Name = "$mod_epicloot_skill_name",
            Description = "$mod_epicloot_skill_description",
            Identifier = "mod_epicloot_skill_enchanting",
            Icon = item.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0],
            IncreaseStep = 1f
        };
        SkillType = SkillManager.Instance.AddSkill(config);
        Logger.LogInfo($"Added skill {SkillType}");
    }

    public static void InitSkillConfig(EnchantingSkillConfig config)
    {
        Config = config;
    }

    public static float GetEnchantingSkillLevel()
    {
        return Player.m_localPlayer.GetSkillLevel(SkillType);
    }


    public static void SuccessfulOperation(int targetRarity, OperationType operationType)
    {
        var multiplier = 1f;
        switch (operationType)
        {
            case OperationType.Enchant:
                multiplier = Config.SuccessEnchantSkillMultiplier;
                break;
            case OperationType.Disenchant:
                multiplier = Config.SuccessDisenchantSkillMultiplier;
                break;
            case OperationType.Augment:
                multiplier = Config.SuccessAugmentationSkillMultiplier;
                break;
            case OperationType.Convert:
                multiplier = Config.SuccessConvertSkillMultiplier;
                break;
            case OperationType.Sacrifice:
                multiplier = Config.SuccessSacrificeSkillMultiplier;
                break;
            default:
                break;
        }

        var raisingValue = multiplier * (targetRarity + 1f);

        Logger.LogInfo($"Raising Enchanting: {raisingValue}; multiplier: {multiplier}; targetRarity: {targetRarity}");

        Player.m_localPlayer.RaiseSkill(SkillType, raisingValue);
    }

    public static MagicItemEffectDefinition.ValueDef GetSkillCappedValueDef(MagicItemEffectDefinition.ValueDef original,
        ItemRarity rarity)
    {
        if (original == null || Mathf.Approximately(original.MinValue, original.MaxValue)) return original;

        var skillLevel = GetEnchantingSkillLevel();
        var rarityLevel = Config.EnchantLevels[(int) rarity];
        var rarityGap = 100 - rarityLevel;

        var distance = (original.MaxValue - original.MinValue) / 2;

        var diff = skillLevel - rarityLevel;
        var power = diff / rarityGap;

        var offset = distance * power;

        return new MagicItemEffectDefinition.ValueDef
        {
            MinValue = original.MinValue + offset,
            MaxValue = original.MinValue + distance + offset,
            Increment = original.Increment
        };
    }


    public static bool AugmentAvailableForPlayerSkill(MagicItem magicItem)
    {
        var skillLevel = GetEnchantingSkillLevel();
        var rarity = magicItem.Rarity;
        return skillLevel >= Config.EnchantLevels[(int) rarity];
    }

    public static float ClampEffectValue(string effectType, float scale, float totalValue)
    {
        var cap = Config.CapEffects.Find(ce => ce.EffectName == effectType);

        if (cap != null)
        {
            EpicLootBase.Log(
                $"GetTotalActiveMagicEffectValue totalBefore: {totalValue}; cap: {cap.Cap * scale}");
            return Math.Min(totalValue, cap.Cap * scale);
        }

        return totalValue;
    }

    public enum OperationType
    {
        Enchant,
        Disenchant,
        Augment,
        Convert,
        Sacrifice
    }
}

[Serializable]
public class EnchantingSkillConfig
{
    public List<int> EnchantLevels = new();
    public float SuccessEnchantSkillMultiplier = 1f;
    public float SuccessDisenchantSkillMultiplier = 1f;
    public float SuccessAugmentationSkillMultiplier = 1f;
    public float SuccessConvertSkillMultiplier = 1f;
    public float SuccessSacrificeSkillMultiplier = 1f;
    public List<CapResistance> CapEffects = new();
}

[Serializable]
public class CapResistance
{
    public string EffectName;
    public int Cap;
}