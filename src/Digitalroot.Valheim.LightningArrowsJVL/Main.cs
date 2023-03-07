using BepInEx;
using BepInEx.Configuration;
using Digitalroot.Valheim.Common;
using Digitalroot.Valheim.Common.Names.Vanilla;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Digitalroot.Valheim.LightningArrowsJVL
{
  [BepInPlugin(Guid, Name, Version)]
  [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
  [BepInIncompatibility("com.bepinex.plugins.lightningarrows")]
  [BepInDependency(Jotunn.Main.ModGuid, "2.10.0")]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public partial class Main : BaseUnityPlugin, ITraceableLogging
  {
    public static Main Instance;
    [UsedImplicitly] public static ConfigEntry<int> NexusId;

    public Main()
    {
      Instance = this;
      #if DEBUG
      EnableTrace = true;
      Log.RegisterSource(Instance);
      #else
      EnableTrace = false;
      #endif
      NexusId = Config.Bind("General", "NexusID", 000, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));
      Log.RegisterSource(Instance);
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
    }

    [UsedImplicitly]
    private void Awake()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
        PrefabManager.OnVanillaPrefabsAvailable += AddClonedItems;
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    private void AddClonedItems()
    {
      AddLightningArrow();

      // You want that to run only once, Jotunn has the item cached for the game session
      PrefabManager.OnVanillaPrefabsAvailable -= AddClonedItems;
    }

    private void AddLightningArrow()
    {
      try
      {
        Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");

        var customItem = new CustomItem("ArrowLightningJVL"
                                        , ItemDropNames.ArrowSilver
                                        , new ItemConfig
                                        {
                                          CraftingStation = CraftingStationNames.Workbench
                                          , MinStationLevel = 1
                                          , Amount = 20
                                          , Requirements = new[]
                                          {
                                            new RequirementConfig
                                            {
                                              Item = ItemDropNames.Wood
                                              , Amount = 8
                                            }
                                            , new RequirementConfig
                                            {
                                              Item = ItemDropNames.Feathers
                                              , Amount = 2
                                            }
                                            , new RequirementConfig
                                            {
                                              Item = ItemDropNames.HardAntler
                                              , Amount = 1
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

        itemDrop.m_itemData.m_shared.m_name = "$item_lightning_arrow";
        itemDrop.m_itemData.m_shared.m_description = "$item_bone_arrow_description";
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Ammo;
        itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
        itemDrop.m_itemData.m_shared.m_weight = 0.1f;
        itemDrop.m_itemData.m_shared.m_backstabBonus = 4f;
        itemDrop.m_itemData.m_shared.m_damages.m_pierce = 20f;
        itemDrop.m_itemData.m_shared.m_damages.m_lightning = 40f;
        itemDrop.m_itemData.m_shared.m_damages.m_spirit = 0f;

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
