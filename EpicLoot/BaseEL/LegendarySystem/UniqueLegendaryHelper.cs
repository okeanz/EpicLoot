﻿using System.Collections.Generic;
using System.Linq;
using EpicLoot.BaseEL.Common;
using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.BaseEL.LegendarySystem
{
    public static class UniqueLegendaryHelper
    {
        public static readonly Dictionary<string, LegendaryInfo> LegendaryInfo = new Dictionary<string, LegendaryInfo>();
        public static readonly Dictionary<string, LegendarySetInfo> LegendarySets = new Dictionary<string, LegendarySetInfo>();

        private static readonly Dictionary<string, LegendarySetInfo> _itemsToSetMap = new Dictionary<string, LegendarySetInfo>();

        public static readonly LegendaryInfo GenericLegendaryInfo = new LegendaryInfo
        {
            ID = nameof(GenericLegendaryInfo)
        };

        public static void Initialize(LegendaryItemConfig config)
        {
            LegendaryInfo.Clear();
            AddLegendaryInfo(config.LegendaryItems);

            LegendarySets.Clear();
            _itemsToSetMap.Clear();
            AddLegendarySets(config.LegendarySets);
        }

        private static void AddLegendaryInfo([NotNull] IEnumerable<LegendaryInfo> legendaryItems)
        {
            foreach (var legendaryInfo in legendaryItems)
            {
                LegendaryInfo.Add(legendaryInfo.ID, legendaryInfo);
            }
        }

        private static void AddLegendarySets(List<LegendarySetInfo> legendarySets)
        {
            foreach (var legendarySetInfo in legendarySets)
            {
                LegendarySets.Add(legendarySetInfo.ID, legendarySetInfo);
                foreach (var legendaryID in legendarySetInfo.LegendaryIDs)
                {
                    _itemsToSetMap.Add(legendaryID, legendarySetInfo);
                }
            }
        }

        public static bool TryGetLegendaryInfo(string legendaryID, out LegendaryInfo legendaryInfo)
        {
            return LegendaryInfo.TryGetValue(legendaryID, out legendaryInfo);
        }

        public static bool IsGenericLegendary(LegendaryInfo legendaryInfo)
        {
            return legendaryInfo == GenericLegendaryInfo;
        }

        public static IList<LegendaryInfo> GetAvailableLegendaries(ItemDrop.ItemData baseItem, MagicItem magicItem, bool rollSetItem)
        {
            var availableLegendaries = LegendaryInfo.Values.Where(x => x.IsSetItem == rollSetItem && x.Requirements.CheckRequirements(baseItem, magicItem)).AddItem(GenericLegendaryInfo).ToList();
            if (rollSetItem && availableLegendaries.Count > 1)
            {
                availableLegendaries.Remove(UniqueLegendaryHelper.GenericLegendaryInfo);
            }

            return availableLegendaries;
        }

        public static MagicItemEffectDefinition.ValueDef GetLegendaryEffectValues(string legendaryID, string effectType)
        {
            if (LegendaryInfo.TryGetValue(legendaryID, out var legendaryInfo))
            {
                if (legendaryInfo.GuaranteedMagicEffects.TryFind(x => x.Type == effectType, out var guaranteedMagicEffect))
                {
                    return guaranteedMagicEffect.Values;
                }
            }
            return null;
        }

        public static bool TryGetLegendarySetInfo(string setID, out LegendarySetInfo legendarySetInfo)
        {
            if (string.IsNullOrEmpty(setID))
            {
                legendarySetInfo = null;
                return false;
            }

            return LegendarySets.TryGetValue(setID, out legendarySetInfo);
        }

        public static string GetSetForLegendaryItem(LegendaryInfo legendary)
        {
            if (legendary != null && legendary.IsSetItem && !string.IsNullOrEmpty(legendary.ID))
            {
                _itemsToSetMap.TryGetValue(legendary.ID, out var setInfo);
                return setInfo?.ID;
            }

            return null;
        }
    }
}
