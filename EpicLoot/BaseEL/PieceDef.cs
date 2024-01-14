using System.Collections.Generic;
using EpicLoot.BaseEL.Common;

namespace EpicLoot.BaseEL;

public class PieceDef
{
    public string Table;
    public string CraftingStation;
    public string ExtendStation;
    public List<RecipeRequirementConfig> Resources = new List<RecipeRequirementConfig>();
}