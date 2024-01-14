using System.Collections.Generic;
using System.Linq;
using BepInEx;
using EpicLoot.BaseEL.GatedItemType;
using UnityEngine;
using Random = System.Random;

namespace EpicLoot.BaseEL.Adventure.Feature
{
    public class GambleAdventureFeature : AdventureFeature
    {
        public override AdventureFeatureType Type => AdventureFeatureType.Gamble;
        public override int RefreshInterval => AdventureDataManager.Config.Gamble.RefreshInterval;
        public static bool DebugRandom;

        public List<SecretStashItemInfo> GetGambleItems()
        {
            var player = Player.m_localPlayer;
            if (player == null || AdventureDataManager.Config == null)
            {
                return new List<SecretStashItemInfo>();
            }

            var random = DebugRandom ? new Random() : GetRandom();
            var results = new List<SecretStashItemInfo>();

            var availableGambles = GetAvailableGambles();
            RollOnListNTimes(random, availableGambles.ToList(), AdventureDataManager.Config.Gamble.GamblesCount,
                results);


            if (IsBlockedByGlobalKey(1)) return results;

            results.AddRange(Gamble(random, new Currencies {ForestTokens = 2}, ItemRarity.Magic));

            if (IsBlockedByGlobalKey(2)) return results;

            results.AddRange(Gamble(random, new Currencies {ForestTokens = 4, Coins = 2}, ItemRarity.Rare));

            if (IsBlockedByGlobalKey(3)) return results;

            results.AddRange(Gamble(random, new Currencies {IronBountyTokens = 2, ForestTokens = 1, Coins = 3},
                ItemRarity.Epic));

            if (IsBlockedByGlobalKey(4)) return results;

            results.AddRange(Gamble(random,
                new Currencies {ForestTokens = 2, IronBountyTokens = 2, GoldBountyTokens = 2, Coins = 4},
                ItemRarity.Legendary));

            results.RemoveAll(result => !result.IsKeyRequirementFulfilled());

            return results;
        }

        private bool IsBlockedByGlobalKey(int gateNumber)
        {
            if (!AdventureDataManager.Config.Gamble.GambleRarityGlobalKeys[gateNumber].IsNullOrWhiteSpace() &&
                !ZoneSystem.instance.CheckKey(AdventureDataManager.Config.Gamble.GambleRarityGlobalKeys[gateNumber]))
                return true;
            return false;
        }

        private List<SecretStashItemInfo> Gamble(Random random, Currencies costMultiplier, ItemRarity targetRarity)
        {
            var availableGambles = GetAvailableGambles();
            var resultGambles = new List<SecretStashItemInfo>();
            var tempGambles = new List<SecretStashItemInfo>();
            RollOnListNTimes(random, availableGambles.ToList(),
                AdventureDataManager.Config.Gamble.GamblesCount, tempGambles);
            foreach (var gamble in tempGambles)
            {
                var costIndex =
                    AdventureDataManager.Config.Gamble.GambleCosts.FindIndex(x => x.Item == gamble.ItemID);
                var cost = costIndex == -1 ? 1000 : AdventureDataManager.Config.Gamble.GambleCosts[costIndex].CoinsCost;

                gamble.Cost.Coins = cost * costMultiplier.Coins;
                gamble.Cost.ForestTokens = random.Next(
                    AdventureDataManager.Config.Gamble.ForestTokenGambleCostMin,
                    AdventureDataManager.Config.Gamble.ForestTokenGambleCostMax + 1) * costMultiplier.ForestTokens;
                gamble.Cost.IronBountyTokens = gamble.Cost.ForestTokens * costMultiplier.IronBountyTokens;
                gamble.Cost.GoldBountyTokens = gamble.Cost.IronBountyTokens * costMultiplier.GoldBountyTokens;
                gamble.GuaranteedRarity = true;
                gamble.Rarity = targetRarity;
                resultGambles.Add(gamble);
            }

            return resultGambles;
        }

        private List<SecretStashItemInfo> GetAvailableGambles()
        {
            var availableGambles = new List<SecretStashItemInfo>();
            foreach (var itemConfig in AdventureDataManager.Config.Gamble.Gambles)
            {
                if (string.IsNullOrEmpty(itemConfig))
                {
                    EpicLootBase.LogWarning($"Found empty itemConfig.. skipping.");
                    continue;
                }

                var gatingMode = EpicLootBase.GetGatedItemTypeMode();
                if (gatingMode == GatedItemTypeMode.Unlimited)
                {
                    gatingMode = GatedItemTypeMode.PlayerMustKnowRecipe;
                }

                var itemId = GatedItemTypeHelper.GetItemFromCategory(itemConfig, gatingMode);
                Debug.Log($"GetItemFromCategory result: {itemId}");
                if (string.IsNullOrEmpty(itemId))
                {
                    EpicLootBase.LogWarning(
                        $"[AdventureData] Could not find item id from Category (orig={itemConfig})!");
                    continue;
                }

                var itemDrop = CreateItemDrop(itemId);
                if (itemDrop == null)
                {
                    EpicLootBase.LogWarning(
                        $"[AdventureData] Could not find item type (gated={itemId} orig={itemConfig}) in ObjectDB!");
                    continue;
                }

                var itemData = itemDrop.m_itemData;
                var cost = GetGambleCost(itemId);
                availableGambles.Add(new SecretStashItemInfo(itemId, itemData, cost, true));
                ZNetScene.instance.Destroy(itemDrop.gameObject);
            }

            return availableGambles;
        }

        public Currencies GetGambleCost(string itemId)
        {
            var costConfig = AdventureDataManager.Config.Gamble.GambleCosts.Find(x => x.Item == itemId);
            return new Currencies()
            {
                Coins = costConfig?.CoinsCost ?? 1,
                ForestTokens = costConfig?.ForestTokenCost ?? 0,
                IronBountyTokens = costConfig?.IronBountyTokenCost ?? 0,
                GoldBountyTokens = costConfig?.GoldBountyTokenCost ?? 0
            };
        }

        public ItemDrop.ItemData GenerateGambleItem(SecretStashItemInfo itemInfo)
        {
            var gambleRarity = AdventureDataManager.Config.Gamble.GambleRarityChance;
            if (itemInfo.GuaranteedRarity)
            {
                gambleRarity = AdventureDataManager.Config.Gamble.GambleRarityChanceByRarity[(int) itemInfo.Rarity];
            }

            var gambleRarityGlobalKeys = AdventureDataManager.Config.Gamble.GambleRarityGlobalKeys;

            var nonMagicWeight = gambleRarity.Length > 0 ? gambleRarity[0] : 1;

            var random = new Random();
            var totalWeight = gambleRarity.Sum();
            var nonMagic = (random.NextDouble() * totalWeight) < nonMagicWeight;
            if (nonMagic)
            {
                return itemInfo.Item.Clone();
            }

            var rarityTable = new[]
            {
                gambleRarity.Length > 1 ? gambleRarity[1] : 1,
                gambleRarity.Length > 2 ? gambleRarity[2] : 1,
                gambleRarity.Length > 3 ? gambleRarity[3] : 1,
                gambleRarity.Length > 4 ? gambleRarity[4] : 1,
                gambleRarity.Length > 5 ? gambleRarity[5] : 1
            };

            for (var i = 0; i < gambleRarityGlobalKeys.Length; i++)
            {
                var key = gambleRarityGlobalKeys[i];
                if (key.IsNullOrWhiteSpace()) continue;
                if (!ZoneSystem.instance.CheckKey(key))
                    rarityTable[i] = 0;
            }


            var lootTable = new LootTable()
            {
                Object = "Console",
                LeveledLoot = new List<LeveledLootDef>()
                {
                    new LeveledLootDef()
                    {
                        Level = 1,
                        Drops = new[] {new float[] {1, 1}},
                        Loot = new[]
                        {
                            new LootDrop()
                            {
                                Item = itemInfo.ItemID,
                                Rarity = rarityTable,
                                Weight = 1
                            }
                        }
                    }
                }
            };

            var previousDisabledState = LootRoller.CheatDisableGating;
            LootRoller.CheatDisableGating = true;
            LootRoller.CheatRollingItem = true;
            var loot = LootRoller.RollLootTable(lootTable, 1, "Gamble", Player.m_localPlayer.transform.position);
            LootRoller.CheatRollingItem = false;
            LootRoller.CheatDisableGating = previousDisabledState;
            return loot.Count > 0 ? loot[0] : null;
        }
    }
}