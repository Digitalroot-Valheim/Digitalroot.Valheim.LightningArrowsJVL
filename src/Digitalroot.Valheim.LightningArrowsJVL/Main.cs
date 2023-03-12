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
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Digitalroot.Valheim.LightningArrowsJVL
{
  [BepInPlugin(Guid, Name, Version)]
  [NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
  [BepInIncompatibility("com.bepinex.plugins.lightningarrows")]
  [BepInDependency(Jotunn.Main.ModGuid, "2.10.4")]
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
        itemDrop.m_itemData.m_shared.m_description = "$item_lightning_arrow_description";
        itemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Ammo;
        itemDrop.m_itemData.m_shared.m_maxStackSize = 100;
        itemDrop.m_itemData.m_shared.m_weight = 0.1f;
        itemDrop.m_itemData.m_shared.m_backstabBonus = 4f;
        itemDrop.m_itemData.m_shared.m_damages.m_pierce = 20f;
        itemDrop.m_itemData.m_shared.m_damages.m_lightning = 40f;
        itemDrop.m_itemData.m_shared.m_damages.m_spirit = 0f;
        itemDrop.m_itemData.m_shared.m_icons[0] = LoadResourceIcon("Lightning_arrow");

        ItemManager.Instance.AddItem(customItem);
      }
      catch (Exception e)
      {
        Log.Error(Instance, e);
      }
    }

    private static Sprite LoadResourceIcon(string name)
    {
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      return LoadSpriteFromTexture(LoadTextureRaw(GetResource(Assembly.GetCallingAssembly(), $"Digitalroot.Valheim.LightningArrowsJVL.Assets.{name}.png")));
    }

    private static Texture2D LoadTextureRaw(byte[] file)
    {
      if (file.Any())
      {
        Texture2D texture2D = new Texture2D(2, 2);
        bool flag2 = texture2D.LoadImage(file);
        if (flag2)
        {
          return texture2D;
        }
      }

      return null;
    }

    private static Sprite LoadSpriteFromTexture(Texture2D spriteTexture, float pixelsPerUnit = 100f)
    {
      return spriteTexture ? Sprite.Create(spriteTexture, new Rect(0f, 0f, spriteTexture.width, spriteTexture.height), new Vector2(0f, 0f), pixelsPerUnit) : null;
    }

    private static byte[] GetResource(Assembly asm, string resourceName)
    {
      Log.Trace(Instance, $"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
      var manifestResourceStream = asm.GetManifestResourceStream(resourceName);
      Log.Trace(Instance, $"[{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}] manifestResourceStream == null : {manifestResourceStream == null}");
      if (manifestResourceStream == null)
      {
        throw new Exception($"Unable to load the manifestResourceStream from {asm.FullName} for {resourceName}");
      }
      Log.Trace(Instance, $"[{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}] manifestResourceStream.Length : {manifestResourceStream.Length}");
      var array = new byte[manifestResourceStream.Length];
      var _ = manifestResourceStream.Read(array, 0, (int) manifestResourceStream.Length);
      Log.Trace(Instance, $"[{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}] array.Length : {array.Length}");
      return array;
    }

    #region Implementation of ITraceableLogging

    /// <inheritdoc />
    public string Source => Namespace;

    /// <inheritdoc />
    public bool EnableTrace { get; }

    #endregion
  }
}
