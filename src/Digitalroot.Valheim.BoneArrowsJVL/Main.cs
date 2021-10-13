using BepInEx;
using Digitalroot.Valheim.Common;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Reflection;

namespace Digitalroot.Valheim.BoneArrowsJVL
{
  [BepInPlugin(Guid, Name, Version)]
  [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
  [BepInIncompatibility("com.bepinex.plugins.bonearrows")]
  [BepInDependency(Jotunn.Main.ModGuid)]
  public class Main : BaseUnityPlugin, ITraceableLogging
  {
    public const string Version = "1.0.0";
    public const string Name = "Digitalroot BoneArrowsJVL";
    public const string Guid = "digitalroot.mods.bonearrows.jvl";
    public const string Namespace = "Digitalroot.Valheim" + nameof(BoneArrowsJVL);
    public static Main Instance;

    public Main()
    {
      Instance = this;
#if DEBUG
      EnableTrace = true;
      Log.RegisterSource(Instance);
#else
      EnableTrace = false;
#endif
      Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
    }

    [UsedImplicitly]
    private void Awake()
    {
      try
      {
        Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");
        PrefabManager.OnVanillaPrefabsAvailable += AddClonedItems;
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    private void AddClonedItems()
    {
      AddBoneArrow();

      // You want that to run only once, Jotunn has the item cached for the game session
      PrefabManager.OnVanillaPrefabsAvailable -= AddClonedItems;
    }

    private void AddBoneArrow()
    {
      try
      {
        Log.Trace(Main.Instance, $"{Main.Namespace}.{MethodBase.GetCurrentMethod().DeclaringType?.Name}.{MethodBase.GetCurrentMethod().Name}");

        var customItem = new CustomItem("ArrowBoneJVL"
          , Common.Names.Vanilla.ItemDropNames.ArrowFlint
          , new ItemConfig
          {
            CraftingStation = Common.Names.Vanilla.CraftingStationNames.Workbench
            , MinStationLevel = 1
            , Amount = 20
            , Requirements = new[]
            {
              new RequirementConfig
              {
                Item = Common.Names.Vanilla.ItemDropNames.Wood
                , Amount = 8
              }
              , new RequirementConfig
              {
                Item = Common.Names.Vanilla.ItemDropNames.Feathers
                , Amount = 2
              }
              , new RequirementConfig
              {
                Item = Common.Names.Vanilla.ItemDropNames.BoneFragments
                , Amount = 5
              }
            }
          });

        var prefab = customItem.ItemPrefab;

        if (prefab == null)
        {
          throw new NullReferenceException(nameof(prefab));
        }

        var itemDrop = prefab.GetComponent<ItemDrop>();

        if (itemDrop == null)
        {
          throw new NullReferenceException(nameof(itemDrop));
        }

        itemDrop.m_itemData.m_shared.m_name = "$item_bone_arrow";
        itemDrop.m_itemData.m_shared.m_description = "$item_bone_arrow_description";
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Ammo;
        itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
        itemDrop.m_itemData.m_shared.m_weight = 0.1f;
        itemDrop.m_itemData.m_shared.m_backstabBonus = 4f;
        itemDrop.m_itemData.m_shared.m_damages.m_pierce = 30f;

        ItemManager.Instance.AddItem(customItem);
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    #region Implementation of ITraceableLogging

    /// <inheritdoc />
    public string Source => Namespace;

    /// <inheritdoc />
    public bool EnableTrace { get; }

    #endregion
  }
}
