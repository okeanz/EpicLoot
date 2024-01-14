using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot.BaseEL;

public class Assets
{
    public AssetBundle AssetBundle;
    public Sprite EquippedSprite;
    public Sprite AugaEquippedSprite;
    public Sprite GenericSetItemSprite;
    public Sprite AugaSetItemSprite;
    public Sprite GenericItemBgSprite;
    public Sprite AugaItemBgSprite;
    public GameObject[] MagicItemLootBeamPrefabs = new GameObject[5];
    public readonly Dictionary<string, GameObject[]> CraftingMaterialPrefabs = new Dictionary<string, GameObject[]>();
    public Sprite SmallButtonEnchantOverlay;
    public AudioClip[] MagicItemDropSFX = new AudioClip[5];
    public AudioClip ItemLoopSFX;
    public AudioClip AugmentItemSFX;
    public GameObject MerchantPanel;
    public Sprite MapIconTreasureMap;
    public Sprite MapIconBounty;
    public AudioClip AbandonBountySFX;
    public AudioClip DoubleJumpSFX;
    public GameObject DebugTextPrefab;
    public GameObject AbilityBar;
    public GameObject WelcomMessagePrefab;
}