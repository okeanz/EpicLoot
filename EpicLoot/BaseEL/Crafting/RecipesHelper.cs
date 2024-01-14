﻿using System;
using EpicLoot.BaseEL.Common;

namespace EpicLoot.BaseEL.Crafting
{
    public static class RecipesHelper
    {
        public static RecipesConfig Config;

        public static void Initialize(RecipesConfig config)
        {
            Config = config;

            if (EpicLootBase.IsObjectDBReady())
            {
                SetupRecipes();
            }
        }

        public static void SetupRecipes()
        {
            PrefabCreator.Reset();
            foreach (var recipe in Config.recipes)
            {
                if (!String.IsNullOrEmpty(recipe.craftingStation))
                {
                    PrefabCreator.AddNewRecipe(recipe.name, recipe.item, recipe);                    
                }
            }
        }

        public static bool HaveRequirements(Player player, Piece.Requirement[] requirements, int qualityLevel)
        {
            foreach (var resource in requirements)
            {
                if (resource.m_resItem)
                {
                    var amount = resource.GetAmount(qualityLevel);
                    var num = player.m_inventory.CountItems(resource.m_resItem.m_itemData.m_shared.m_name);
                    if (num < amount)
                        return false;
                }
            }
            return true;
        }
    }
}
