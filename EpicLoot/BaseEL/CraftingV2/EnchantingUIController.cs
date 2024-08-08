using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EpicLoot.BaseEL.Crafting;
using EpicLoot.BaseEL.Data;
using EpicLoot.Skill;
using EpicLoot_UnityLib;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace EpicLoot.BaseEL.CraftingV2
{
    [Flags]
    public enum EnchantingTabs : uint
    {
        None = 0,
        Sacrifice = 1 << 0,
        ConvertMaterials = 1 << 1,
        Enchant = 1 << 2,
        Augment = 1 << 3,
        Disenchant = 1 << 4,

        //Helheim = 1 << 5,
        Upgrade = 1 << 6
    }

    public class EnchantingUIController : MonoBehaviour
    {
        public static void Initialize()
        {
            EnchantingTableUI.AugaFixup = EnchantingUIAugaFixup.AugaFixup;
            EnchantingTableUI.TabActivation = TabActivation;
            MultiSelectItemList.SortByRarity = SortByRarity;
            MultiSelectItemList.SortByName = SortByName;
            MultiSelectItemListElement.SetMagicItem = SetMagicItem;
            SacrificeUI.GetSacrificeItems = GetSacrificeItems;
            SacrificeUI.GetSacrificeProducts = GetSacrificeProducts;
            ConvertUI.GetConversionRecipes = GetConversionRecipes;
            SetRarityColor.GetRarityColor = GetRarityColor;
            EnchantUI.GetEnchantableItems = GetEnchantableItems;
            EnchantUI.GetEnchantInfo = GetEnchantInfo;
            EnchantUI.GetEnchantCost = GetEnchantCost;
            EnchantUI.EnchantItem = EnchantItemAndReturnSuccessDialog;
            AugmentUI.GetAugmentableItems = GetAugmentableItems;
            AugmentUI.GetAugmentableEffects = GetAugmentableEffects;
            AugmentUI.GetAvailableEffects = GetAvailableAugmentEffects;
            AugmentUI.GetAugmentCost = GetAugmentCost;
            AugmentUI.AugmentItem = AugmentItem;
            EnchantingTable.UpgradesActive = UpgradesActive;
            FeatureStatus.UpgradesActive = UpgradesActive;
            DisenchantUI.GetDisenchantItems = GetDisenchantItems;
            DisenchantUI.GetDisenchantCost = GetDisenchantCost;
            DisenchantUI.DisenchantItem = DisenchantItem;
            FeatureStatus.MakeFeatureUnlockTooltip = MakeFeatureUnlockTooltip;
        }

        private static bool UpgradesActive(EnchantingFeature feature, out bool featureActive)
        {
            featureActive = false;

            var tabEnum = EnchantingTabs.None;

            switch (feature)
            {
                case EnchantingFeature.Augment:
                    tabEnum = EnchantingTabs.Augment;
                    break;
                case EnchantingFeature.Enchant:
                    tabEnum = EnchantingTabs.Enchant;
                    break;
                case EnchantingFeature.Disenchant:
                    tabEnum = EnchantingTabs.Disenchant;
                    break;
                case EnchantingFeature.ConvertMaterials:
                    tabEnum = EnchantingTabs.ConvertMaterials;
                    break;
                case EnchantingFeature.Sacrifice:
                    tabEnum = EnchantingTabs.Sacrifice;
                    break;
            }

            featureActive = (tabEnum & EpicLootBase.EnchantingTableActivatedTabs.Value) != 0;

            return EpicLootBase.EnchantingTableUpgradesActive.Value;
        }

        public static EnchantingTableUI UiHandler;


        private static void TabActivation(EnchantingTableUI ui)
        {
            if (ui == null || ui.TabHandler == null)
                return;

            UiHandler = ui;

            for (int i = 0; i < ui.TabHandler.transform.childCount; i++)
            {
                var tabGo = ui.TabHandler.transform.GetChild(i).gameObject;
                var tabBitwise = 1 << i;
                switch ((EnchantingTabs) tabBitwise)
                {
                    case EnchantingTabs.Upgrade:
                        tabGo.SetActive(EpicLootBase.EnchantingTableUpgradesActive.Value);
                        break;
                    case EnchantingTabs.None:
                        break;
                    default:
                        tabGo.SetActive(
                            (EpicLootBase.EnchantingTableActivatedTabs.Value & (EnchantingTabs) tabBitwise) != 0);
                        break;
                }
            }
        }

        public static void RefreshEnchantRarityButtons()
        {
            if (UiHandler == null) return;
            var enchantPanel = (EnchantUI) UiHandler.Panels.First(x => x is EnchantUI);
            var skillLevel = Enchanting.GetEnchantingSkillLevel();
            var enchantLevelsCount = Enchanting.Config.EnchantLevels.Count;
            for (var i1 = 0; i1 < enchantPanel.RarityButtons.Count; i1++)
            {
                var button = enchantPanel.RarityButtons[i1];
                var targetLevel =
                    Enchanting.Config.EnchantLevels[i1 > enchantLevelsCount ? enchantLevelsCount - 1 : i1];
                if (skillLevel < targetLevel)
                {
                    button.interactable = false;
                    button.graphic.color = Color.gray;
                    var tooltip = button.GetComponent<UITooltip>() ??
                                  button.gameObject.AddComponent<UITooltip>();
                    if (tooltip.m_tooltipPrefab == null)
                    {
                        tooltip.m_tooltipPrefab = PrefabManager.Instance.GetPrefab("InventoryTooltip");
                    }

                    tooltip.enabled = true;
                    tooltip.m_text = Localization.instance.Localize("$mod_epicloot_skill_rarity_blocked_text")
                        .Replace("{0}", $"{targetLevel}");
                    tooltip.m_topic = "$mod_epicloot_skill_rarity_blocked_topic";
                }
                else
                {
                    button.interactable = true;
                    button.graphic.color = Color.white;
                    var tooltip = button.GetComponent<UITooltip>();
                    if (tooltip != null)
                    {
                        tooltip.enabled = false;
                    }
                }
            }
        }

        private static void MakeFeatureUnlockTooltip(GameObject obj)
        {
            if (EpicLootBase.HasAuga)
            {
                Auga.API.Tooltip_MakeSimpleTooltip(obj);
            }
            else
            {
                var uiTooltip = obj.GetComponent<UITooltip>();
                uiTooltip.m_tooltipPrefab = InventoryGui.instance.m_playerGrid.m_elementPrefab.GetComponent<UITooltip>()
                    .m_tooltipPrefab;
            }
        }

        private static void SetMagicItem(MultiSelectItemListElement element, ItemDrop.ItemData item, UITooltip tooltip)
        {
            if (element.ItemIcon != null)
                element.ItemIcon.sprite = item.GetIcon();
            if (element.ItemName != null)
                element.ItemName.text = item.GetDecoratedName();

            if (element.MagicBG != null)
            {
                var useMagicBG = item.UseMagicBackground();
                element.MagicBG.enabled = useMagicBG;

                if (useMagicBG)
                    element.MagicBG.color = item.GetRarityColor();
            }

            if (tooltip)
            {
                if (EpicLootBase.HasAuga)
                {
                    Auga.API.Tooltip_MakeItemTooltip(element.gameObject, item);
                }
                else
                {
                    tooltip.m_topic = Localization.instance.Localize(item.GetDecoratedName());
                    tooltip.m_text = Localization.instance.Localize(item.GetTooltip());
                }
            }
        }

        private static List<IListElement> SortByRarity(List<IListElement> items)
        {
            return items.OrderBy(x => x.GetItem().HasRarity() ? x.GetItem().GetRarity() : (ItemRarity) (-1))
                .ThenBy(x => Localization.instance.Localize(x.GetItem().GetDecoratedName()))
                .ToList();
        }

        private static List<IListElement> SortByName(List<IListElement> items)
        {
            var richTextRegex = new Regex(@"<[^>]*>");
            return items.OrderBy(x =>
                    richTextRegex.Replace(Localization.instance.Localize(x.GetItem().GetDecoratedName()), string.Empty))
                .ThenByDescending(x => x.GetItem().m_stack)
                .ToList();
        }

        private static List<InventoryItemListElement> GetSacrificeItems()
        {
            var player = Player.m_localPlayer;
            var result = new List<InventoryItemListElement>();

            var inventory = player.GetInventory();
            var boundItems = new List<ItemDrop.ItemData>();
            inventory.GetBoundItems(boundItems);
            foreach (var item in inventory.GetAllItems())
            {
                if (!EpicLootBase.ShowEquippedAndHotbarItemsInSacrificeTab.Value)
                {
                    if (item != null && item.m_equipped || boundItems.Contains(item))
                        continue;
                }

                var products = EnchantCostsHelper.GetSacrificeProducts(item);
                if (products != null)
                    result.Add(new InventoryItemListElement() {Item = item});
            }

            return result;
        }

        private static void AddItemToProductSet(Dictionary<string, ItemDrop.ItemData> productSet, string itemID,
            int amount)
        {
            if (amount <= 0)
            {
                EpicLootBase.LogWarning($"Tried to add item ({itemID}) with zero quantity to sacrifice product");
                return;
            }

            var prefab = ObjectDB.instance.GetItemPrefab(itemID);
            if (prefab == null)
            {
                EpicLootBase.LogWarning($"Tried to add unknown item ({itemID}) to sacrifice product");
                return;
            }

            var itemDrop = prefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                EpicLootBase.LogWarning($"Tried to add object with no ItemDrop ({itemID}) to sacrifice product");
                return;
            }

            ItemDrop.ItemData itemData;
            if (productSet.TryGetValue(itemID, out itemData))
            {
                itemData.m_stack += amount;
            }
            else
            {
                itemData = itemDrop.m_itemData.Clone();
                itemData.m_dropPrefab = prefab;
                itemData.m_stack = amount;
                productSet.Add(itemID, itemData);
            }
        }

        private static List<InventoryItemListElement> GetSacrificeProducts(List<Tuple<ItemDrop.ItemData, int>> items)
        {
            var productsSet = new Dictionary<string, ItemDrop.ItemData>();
            foreach (var entry in items)
            {
                var item = entry.Item1;
                var amount = entry.Item2;
                if (amount <= 0)
                    continue;

                var products = EnchantCostsHelper.GetSacrificeProducts(item);
                if (products == null)
                    continue;

                foreach (var itemAmountConfig in products)
                {
                    AddItemToProductSet(productsSet, itemAmountConfig.Item, itemAmountConfig.Amount * amount);
                }
            }

            return productsSet.Values.OrderByDescending(x => x.HasRarity() ? x.GetRarity() : (ItemRarity) (-1))
                .ThenBy(x => Localization.instance.Localize(x.GetDecoratedName()))
                .Select(x => new InventoryItemListElement() {Item = x})
                .ToList();
        }

        private static List<ConversionRecipeUnity> GetConversionRecipes(int mode)
        {
            var conversionType = (MaterialConversionType) mode;
            var conversions = MaterialConversions.Conversions.GetValues(conversionType, true);

            var featureValues =
                EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.ConvertMaterials);
            var materialConversionAmount = float.IsNaN(featureValues.Item1) ? -1 : featureValues.Item1;
            var runestoneConversionAmount = float.IsNaN(featureValues.Item2) ? -1 : featureValues.Item2;

            var player = Player.m_localPlayer;
            var inventory = player.GetInventory();

            var result = new List<ConversionRecipeUnity>();

            foreach (var conversion in conversions)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(conversion.Product);
                if (prefab == null)
                {
                    EpicLootBase.LogWarning($"Could not find conversion product ({conversion.Product})!");
                    continue;
                }

                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    EpicLootBase.LogWarning($"Conversion product ({conversion.Product}) is not an ItemDrop!");
                    continue;
                }

                itemDrop.m_itemData.m_dropPrefab = prefab;

                var recipe = new ConversionRecipeUnity()
                {
                    Product = itemDrop.m_itemData.Clone(),
                    Amount = conversion.Amount,
                    Cost = new List<ConversionRecipeCostUnity>()
                };

                var hasSomeItems = false;
                foreach (var requirement in conversion.Resources)
                {
                    var reqPrefab = ObjectDB.instance.GetItemPrefab(requirement.Item);
                    if (reqPrefab == null)
                    {
                        EpicLootBase.LogWarning($"Could not find conversion requirement ({requirement.Item})!");
                        continue;
                    }

                    var reqItemDrop = reqPrefab.GetComponent<ItemDrop>();
                    if (reqItemDrop == null)
                    {
                        EpicLootBase.LogWarning($"Conversion requirement ({requirement.Item}) is not an ItemDrop!");
                        continue;
                    }

                    reqItemDrop.m_itemData.m_dropPrefab = reqPrefab;

                    var requiredAmount = requirement.Amount;
                    if (runestoneConversionAmount > 0 && conversion.Type == MaterialConversionType.Upgrade &&
                        recipe.Product.IsRunestone() && reqItemDrop.m_itemData.IsRunestone())
                        requiredAmount = Mathf.CeilToInt(runestoneConversionAmount * recipe.Amount);
                    else if (materialConversionAmount > 0 && conversion.Type == MaterialConversionType.Upgrade &&
                             recipe.Product.IsMagicCraftingMaterial() &&
                             reqItemDrop.m_itemData.IsMagicCraftingMaterial())
                        requiredAmount = Mathf.CeilToInt(materialConversionAmount * recipe.Amount);

                    recipe.Cost.Add(new ConversionRecipeCostUnity
                    {
                        Item = reqItemDrop.m_itemData.Clone(),
                        Amount = requiredAmount
                    });

                    if (inventory.CountItems(reqItemDrop.m_itemData.m_shared.m_name) > 0)
                        hasSomeItems = true;
                }

                if (hasSomeItems)
                    result.Add(recipe);
            }

            return result;
        }

        private static Color GetRarityColor(MagicRarityUnity rarity)
        {
            return EpicLootBase.GetRarityColorARGB((ItemRarity) rarity);
        }

        private static List<InventoryItemListElement> GetEnchantableItems()
        {
            return Player.m_localPlayer.GetInventory().GetAllItems()
                .Where(item => !item.IsMagic() && EpicLootBase.CanBeMagicItem(item))
                .Select(item => new InventoryItemListElement() {Item = item})
                .ToList();
        }

        private static string GetEnchantInfo(ItemDrop.ItemData item, MagicRarityUnity _rarity)
        {
            var rarity = (ItemRarity) _rarity;
            var sb = new StringBuilder();
            var rarityColor = EpicLootBase.GetRarityColor(rarity);
            var rarityDisplay = EpicLootBase.GetRarityDisplayName(rarity);
            sb.AppendLine(
                $"{item.m_shared.m_name} \u2794 <color={rarityColor}>{rarityDisplay}</color> {item.GetDecoratedName(rarityColor)}");
            sb.AppendLine($"<color={rarityColor}>");

            var featureValues =
                EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Enchant);
            var highValueBonus = float.IsNaN(featureValues.Item1) ? 0 : featureValues.Item1;
            var midValueBonus = float.IsNaN(featureValues.Item2) ? 0 : featureValues.Item2;

            var effectCountWeights = LootRoller.GetEffectCountsPerRarity(rarity, true);
            float totalWeight = effectCountWeights.Sum(x => x.Value);
            for (var index = 0; index < effectCountWeights.Count; index++)
            {
                var effectCountEntry = effectCountWeights[index];
                var count = effectCountEntry.Key;
                var weight = effectCountEntry.Value;
                var percent = (int) (weight / totalWeight * 100.0f);
                var label = count == 1
                    ? $"{count} $mod_epicloot_enchant_effect"
                    : $"{count} $mod_epicloot_enchant_effects";

                if (index == effectCountWeights.Count - 1 && highValueBonus > 0)
                    sb.AppendLine(
                        $"‣ {label} {percent}% <color=#EAA800>(+{highValueBonus}% $mod_epicloot_bonus)</color>");
                else if (index == effectCountWeights.Count - 2 && midValueBonus > 0)
                    sb.AppendLine(
                        $"‣ {label} {percent}% <color=#EAA800>(+{midValueBonus}% $mod_epicloot_bonus)</color>");
                else
                    sb.AppendLine($"‣ {label} {percent}%");
            }

            sb.Append("</color>");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(Localization.instance.Localize("$mod_epicloot_augment_availableeffects"));
            sb.AppendLine($"<color={rarityColor}>");

            var tempMagicItem = new MagicItem() {Rarity = rarity};
            var availableEffects = MagicItemEffectDefinitions.GetAvailableEffects(item, tempMagicItem);

            foreach (var effectDef in availableEffects)
            {
                var values = effectDef.GetValuesForRarity(rarity);
                var skillCappedValues = Enchanting.GetSkillCappedValueDef(values, rarity);
                var valueDisplay = values != null
                    ? Mathf.Approximately(values.MinValue, values.MaxValue) ? $"{values.MinValue}" :
                    $"({skillCappedValues.MinValue}-{skillCappedValues.MaxValue})[{values.MinValue}-{values.MaxValue}]"
                    : "";
                sb.AppendLine(
                    $"‣ {string.Format(Localization.instance.Localize(effectDef.DisplayText), valueDisplay)}");
            }

            sb.Append("</color>");

            return Localization.instance.Localize(sb.ToString());
        }

        private static List<InventoryItemListElement> GetEnchantCost(ItemDrop.ItemData item, MagicRarityUnity _rarity)
        {
            return EnchantHelper.GetEnchantCosts(item, (ItemRarity) _rarity).Select(entry =>
            {
                var itemData = entry.Key.m_itemData.Clone();
                itemData.m_dropPrefab = entry.Key.gameObject;
                itemData.m_stack = entry.Value;
                return new InventoryItemListElement() {Item = itemData};
            }).ToList();
        }

        private static GameObject EnchantItemAndReturnSuccessDialog(ItemDrop.ItemData item, MagicRarityUnity rarity)
        {
            var player = Player.m_localPlayer;

            Enchanting.SuccessfulOperation((int) rarity, Enchanting.OperationType.Enchant);

            float previousDurabilityPercent = 0;
            if (item.m_shared.m_useDurability)
                previousDurabilityPercent = item.m_durability / item.GetMaxDurability();

            var luckFactor = player.GetTotalActiveMagicEffectValue(MagicEffectType.Luck, 0.01f);
            var magicItem = LootRoller.RollMagicItem((ItemRarity) rarity, item, luckFactor);

            var magicItemComponent = item.Data().GetOrCreate<MagicItemComponent>();
            magicItemComponent.SetMagicItem(magicItem);

            EquipmentEffectCache.Reset(player);

            // Maintain durability
            if (item.m_shared.m_useDurability)
                item.m_durability = previousDurabilityPercent * item.GetMaxDurability();

            CraftSuccessDialog successDialog;
            if (EpicLootBase.HasAuga)
            {
                var resultsPanel = Auga.API.Workbench_CreateNewResultsPanel();
                resultsPanel.transform.SetParent(EnchantingTableUI.instance.transform);
                resultsPanel.SetActive(false);
                successDialog = resultsPanel.gameObject.AddComponent<CraftSuccessDialog>();
                successDialog.NameText = successDialog.transform.Find("Topic").GetComponent<TMP_Text>();
            }
            else
            {
                successDialog = CraftSuccessDialog.Create(EnchantingTableUI.instance.transform);
            }

            successDialog.Show(item.Extended());

            var rt = (RectTransform) successDialog.transform;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 0);

            if (!EpicLootBase.HasAuga)
            {
                var frame = successDialog.transform.Find("Frame");
                if (frame != null)
                {
                    var frameRT = (RectTransform) frame;
                    frameRT.pivot = new Vector2(0.5f, 0.5f);
                    frameRT.anchorMax = new Vector2(0.5f, 0.5f);
                    frameRT.anchorMin = new Vector2(0.5f, 0.5f);
                    frameRT.anchoredPosition = new Vector2(0, 0);
                }
            }

            MagicItemEffects.Indestructible.MakeItemIndestructible(item);

            Game.instance.GetPlayerProfile().m_playerStats.m_stats[PlayerStatType.Crafts]++;
            Gogan.LogEvent("Game", "Enchanted", item.m_shared.m_name, 1);

            return successDialog.gameObject;
        }

        private static List<InventoryItemListElement> GetAugmentableItems()
        {
            return Player.m_localPlayer.GetInventory().GetAllItems()
                .Where(item => item.CanBeAugmented())
                .Select(item => new InventoryItemListElement() {Item = item})
                .ToList();
        }

        private static List<Tuple<string, bool>> GetAugmentableEffects(ItemDrop.ItemData item)
        {
            var result = new List<Tuple<string, bool>>();

            var magicItem = item?.GetMagicItem();
            if (magicItem != null)
            {
                var rarity = magicItem.Rarity;
                var augmentableEffects = magicItem.Effects;

                for (var index = 0; index < augmentableEffects.Count; index++)
                {
                    var augmentableEffect = augmentableEffects[index];
                    var effectDef = MagicItemEffectDefinitions.Get(augmentableEffect.EffectType);
                    var canAugment = effectDef != null && effectDef.CanBeAugmented;

                    var text = AugmentHelper.GetAugmentSelectorText(magicItem, index, augmentableEffects, rarity);
                    var color = EpicLootBase.GetRarityColor(rarity);
                    var alpha = canAugment ? "FF" : "7F";
                    text = $"<color={color}{alpha}>{text}</color>";

                    result.Add(new Tuple<string, bool>(text, canAugment));
                }
            }

            return result;
        }

        private static string GetAvailableAugmentEffects(ItemDrop.ItemData item, int augmentindex)
        {
            var magicItem = item?.GetMagicItem();
            if (magicItem == null)
                return string.Empty;

            var rarity = magicItem.Rarity;
            var rarityColor = EpicLootBase.GetRarityColor(rarity);

            var valuelessEffect = false;
            if (augmentindex >= 0 && augmentindex < magicItem.Effects.Count)
            {
                var currentEffectDef = MagicItemEffectDefinitions.Get(magicItem.Effects[augmentindex].EffectType);
                valuelessEffect = currentEffectDef.GetValuesForRarity(rarity) == null;
            }

            var availableEffects = MagicItemEffectDefinitions.GetAvailableEffects(item.Extended(), item.GetMagicItem(),
                valuelessEffect ? -1 : augmentindex);

            var sb = new StringBuilder();
            sb.Append($"<color={rarityColor}>");
            foreach (var effectDef in availableEffects)
            {
                var values = effectDef.GetValuesForRarity(item.GetRarity());


                string valueDisplay = "";
                if (values != null)
                {
                    var skillCappedValue = Enchanting.GetSkillCappedValueDef(values, rarity);
                    var similar = Mathf.Approximately(values.MinValue, values.MaxValue);
                    if (similar)
                    {
                        valueDisplay = $"{values.MinValue}";
                    }
                    else
                    {
                        valueDisplay =
                            $"({skillCappedValue.MinValue}-{skillCappedValue.MaxValue})[{values.MinValue}-{values.MaxValue}]";
                    }
                }

                sb.AppendLine(
                    $"‣ {string.Format(Localization.instance.Localize(effectDef.DisplayText), valueDisplay)}");
            }

            sb.Append("</color>");

            return sb.ToString();
        }

        private static List<InventoryItemListElement> GetAugmentCost(ItemDrop.ItemData item, int augmentindex)
        {
            return AugmentHelper.GetAugmentCosts(item, augmentindex)
                .Select(x =>
                {
                    var itemData = x.Key.m_itemData.Clone();
                    itemData.m_dropPrefab = x.Key.gameObject;
                    itemData.m_stack = x.Value;
                    return new InventoryItemListElement() {Item = itemData};
                }).ToList();
        }

        private static GameObject AugmentItem(ItemDrop.ItemData item, int augmentindex)
        {
            // Set as augmented
            var magicItem = item?.GetMagicItem();
            if (magicItem == null)
                return null;

            Enchanting.SuccessfulOperation((int) magicItem.Rarity, Enchanting.OperationType.Augment);

            magicItem.SetEffectAsAugmented(augmentindex);
            item.SaveMagicItem(magicItem);

            var choiceDialog = AugmentHelper.CreateAugmentChoiceDialog(true);
            choiceDialog.transform.SetParent(EnchantingTableUI.instance.transform);

            var rt = (RectTransform) choiceDialog.transform;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, 0);

            if (!EpicLootBase.HasAuga)
            {
                var frame = choiceDialog.transform.Find("Frame");
                if (frame != null)
                {
                    var frameRT = (RectTransform) frame;
                    frameRT.pivot = new Vector2(0.5f, 0.5f);
                    frameRT.anchorMax = new Vector2(0.5f, 0.5f);
                    frameRT.anchorMin = new Vector2(0.5f, 0.5f);
                    frameRT.anchoredPosition = new Vector2(0, 0);
                }
            }

            choiceDialog.Show(item, augmentindex, OnAugmentComplete);
            return choiceDialog.gameObject;
        }

        private static void OnAugmentComplete(ItemDrop.ItemData item, int effectIndex, MagicItemEffect newEffect)
        {
            var magicItem = item?.GetMagicItem();
            if (magicItem == null)
                return;

            if (magicItem.HasEffect(MagicEffectType.Indestructible))
            {
                item.m_shared.m_useDurability =
                    item.m_dropPrefab?.GetComponent<ItemDrop>().m_itemData.m_shared.m_useDurability ?? false;

                if (item.m_shared.m_useDurability)
                {
                    item.m_durability = item.GetMaxDurability();
                }
            }

            var oldEffects = magicItem.GetEffects();
            var oldEffect = (effectIndex >= 0 && effectIndex < oldEffects.Count) ? oldEffects[effectIndex] : null;

            magicItem.ReplaceEffect(effectIndex, newEffect);

            // Don't count this free augment as locking in an augment
            if (oldEffect != null && EnchantCostsHelper.EffectIsDeprecated(oldEffect.EffectType))
            {
                magicItem.AugmentedEffectIndices.Remove(effectIndex);
            }

            if (magicItem.Rarity == ItemRarity.Rare)
            {
                magicItem.DisplayName = MagicItemNames.GetNameForItem(item, magicItem);
            }

            item.SaveMagicItem(magicItem);

            MagicItemEffects.Indestructible.MakeItemIndestructible(item);

            Game.instance.GetPlayerProfile().m_playerStats.m_stats[PlayerStatType.Crafts]++;
            Gogan.LogEvent("Game", "Augmented", item.m_shared.m_name, 1);

            EquipmentEffectCache.Reset(Player.m_localPlayer);
        }

        private static List<InventoryItemListElement> GetDisenchantItems()
        {
            var player = Player.m_localPlayer;
            var inventory = player.GetInventory();
            var boundItems = new List<ItemDrop.ItemData>();
            inventory.GetBoundItems(boundItems);
            return Player.m_localPlayer.GetInventory().GetAllItems()
                .Where(item => !item.m_equipped && (EpicLootBase.ShowEquippedAndHotbarItemsInSacrificeTab.Value ||
                                                    !boundItems.Contains(item)))
                .Where(item => item.IsMagic(out var magicItem) && magicItem.CanBeDisenchanted())
                .Select(item => new InventoryItemListElement() {Item = item})
                .ToList();
        }

        private static List<InventoryItemListElement> GetDisenchantCost(ItemDrop.ItemData item)
        {
            var result = new List<InventoryItemListElement>();
            if (item == null || !item.IsMagic())
                return result;

            var rarity = item.GetRarity();
            List<ItemAmountConfig> costList;
            switch (rarity)
            {
                case ItemRarity.Magic:
                    costList = EnchantCostsHelper.Config.DisenchantCosts.Magic;
                    break;

                case ItemRarity.Rare:
                    costList = EnchantCostsHelper.Config.DisenchantCosts.Rare;
                    break;

                case ItemRarity.Epic:
                    costList = EnchantCostsHelper.Config.DisenchantCosts.Epic;
                    break;

                case ItemRarity.Legendary:
                    costList = EnchantCostsHelper.Config.DisenchantCosts.Legendary;
                    break;

                // TODO: Mythic Hookup
                case ItemRarity.Mythic:
                    return result;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var featureValues =
                EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Disenchant);
            var reducedCost = 0;
            if (!float.IsNaN(featureValues.Item2))
                reducedCost = (int) featureValues.Item2;

            foreach (var itemAmountConfig in costList)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(itemAmountConfig.Item);
                if (prefab == null)
                {
                    EpicLootBase.LogWarning(
                        $"Tried to add unknown item ({itemAmountConfig.Item}) to disenchant cost for item ({item.m_shared.m_name})");
                    continue;
                }

                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    EpicLootBase.LogWarning(
                        $"Tried to add item without ItemDrop ({itemAmountConfig.Item}) to disenchant cost for item ({item.m_shared.m_name})");
                    continue;
                }

                var costItem = itemDrop.m_itemData.Clone();
                costItem.m_stack = itemAmountConfig.Amount - reducedCost;
                result.Add(new InventoryItemListElement() {Item = costItem});
            }

            return result;
        }


        private static List<InventoryItemListElement> DisenchantItem(ItemDrop.ItemData item)
        {
            List<InventoryItemListElement> bonusItems = new List<InventoryItemListElement>();
            if (item.IsMagic(out var magicItem) && magicItem.CanBeDisenchanted())
            {
                var featureValues =
                    EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Disenchant);
                var bonusItemChance = 0;
                if (!float.IsNaN(featureValues.Item1))
                    bonusItemChance = (int) featureValues.Item1;

                Enchanting.SuccessfulOperation((int) magicItem.Rarity, Enchanting.OperationType.Disenchant);

                if (Random.Range(0, 99) < bonusItemChance)
                {
                    EnchantingTableUI.instance.PlayEnchantBonusSFX();

                    bonusItems = GetSacrificeProducts(new List<Tuple<ItemDrop.ItemData, int>>() {new(item, 1)});
                }


                item.Data().Remove<MagicItemComponent>();
            }

            return bonusItems;
        }
    }
}