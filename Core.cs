using System.Collections;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.UI;
using ModManagerPhoneApp;
#if MONO
using FishNet;
using ScheduleOne.UI;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Stations;
using ScheduleOne.PlayerScripts;
#else
using Il2CppFishNet;
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.UI.Stations;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.UI.Items;
#endif

[assembly: MelonInfo(
    typeof(ItemSlotColorChanger.Core),
    ItemSlotColorChanger.BuildInfo.Name,
    ItemSlotColorChanger.BuildInfo.Version,
    ItemSlotColorChanger.BuildInfo.Author
)]
[assembly: MelonColor(1, 255, 0, 0)]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonOptionalDependencies("ModManager&PhoneApp")]

namespace ItemSlotColorChanger;

public static class BuildInfo
{
    public const string Name = "Item Slot Color Changer";
    public const string Description = "Changes the background and highlight colors of item slots for better visibility.";
    public const string Author = "Delassa";
    public const string Version = "1.0.0";
}

public class Core : MelonMod
{
    private bool _colorsModified = false;
    private bool _modManagerFound = false;
    private bool _mainSceneLoaded = false;
    private static MelonPreferences_Category _category;
    private static MelonPreferences_Entry<Color> _hotbarColorPref;
    private static MelonPreferences_Entry<Color> _hotbarHighlightColorPref;
    private static MelonPreferences_Entry<Color> _storageColorPref;
    private static MelonPreferences_Entry<Color> _machineColorPref;
    
    public override void OnInitializeMelon()
    {
        // Create or load Melon Preferences
        _category = MelonPreferences.CreateCategory("ItemSlotColorChanger_Colors", "Color Settings");
        _hotbarColorPref = _category.CreateEntry<Color>("HotbarColor", new Color(0.392f, 0.392f, 0.392f, 0.392f), "Hotbar Slot Color");
        _hotbarHighlightColorPref = _category.CreateEntry<Color>("HotbarHighlightColor", new Color(1f, 1f, 1f, 0.5f), "Hotbar Slot Highlight Color");
        _storageColorPref = _category.CreateEntry<Color>("StorageColor", new Color(0.392f, 0.392f, 0.392f, 0.392f), "Storage Slot Color");
        _machineColorPref = _category.CreateEntry<Color>("MachineColor", new Color(0.392f, 0.392f, 0.392f, 0.392f), "Machine Slot Color");
        
        // Initial modification flag to apply colors on first update
        _colorsModified = true;
#if MONO
#else
        // Check for Mod Manager & Phone App presence and subscribe to events
        _modManagerFound = MelonBase.RegisteredMelons.Any(mod => mod?.Info.Name == "Mod Manager & Phone App");
        if (_modManagerFound)
        {
            MelonLogger.Msg(System.ConsoleColor.DarkCyan,"Mod Manager found. Attempting event subscription...");
            SubscribeToModManagerEvents();
        }
        else { MelonLogger.Warning("Mod Manager not found. Skipping event subscription."); }
#endif
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        _mainSceneLoaded = sceneName == "Main";
        // Grab some defaults from the game before we start changing colors
    }

    public override void OnUpdate()
    {
        if (!_mainSceneLoaded) return;
        
        // This gets storage containers
        if (Singleton<StorageMenu>.InstanceExists)
        {
            if (Singleton<StorageMenu>.instance.IsOpen)
            {
                foreach (var slotUI in Singleton<StorageMenu>.Instance.SlotsUIs)
                {
                    slotUI.SetNormalColor(_storageColorPref.Value);
                }
            }
        }

            // Player hotbar color change, check for modification so it doesn't constantly override the highlighting of the active item
            if (PlayerSingleton<PlayerInventory>.InstanceExists)
            {
                if (PlayerSingleton<PlayerInventory>.Instance.HotbarEnabled)
                {
                    if (_colorsModified)
                    {

                        foreach (var slotUI in PlayerSingleton<PlayerInventory>.Instance.slotUIs)
                        {
                            slotUI.SetNormalColor(_hotbarColorPref.Value);
                            slotUI.SetHighlightColor(_hotbarHighlightColorPref.Value);
                        }

                        _colorsModified = false;
                    }
                }
            }

        if (Singleton<PackagingStationCanvas>.InstanceExists)
        {
            Singleton<PackagingStationCanvas>.instance.PackagingSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<PackagingStationCanvas>.instance.ProductSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<PackagingStationCanvas>.instance.OutputSlotUI.SetNormalColor(_machineColorPref.Value);
        }

        if (Singleton<MixingStationCanvas>.InstanceExists)
        {
            Singleton<MixingStationCanvas>.instance.ProductSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<MixingStationCanvas>.instance.IngredientSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<MixingStationCanvas>.instance.PreviewSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<MixingStationCanvas>.instance.OutputSlotUI.SetNormalColor(_machineColorPref.Value);
        }

        if (Singleton<CauldronCanvas>.InstanceExists)
        {
            Singleton<CauldronCanvas>.instance.OutputSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<CauldronCanvas>.instance.LiquidSlotUI.SetNormalColor(_machineColorPref.Value);
            foreach (var ingredientSlotUI in Singleton<CauldronCanvas>.instance.IngredientSlotUIs)
                ingredientSlotUI.SetNormalColor(_machineColorPref.Value);
        }

        if (Singleton<DryingRackCanvas>.InstanceExists)
        {
            Singleton<DryingRackCanvas>.instance.InputSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<DryingRackCanvas>.instance.OutputSlotUI.SetNormalColor(_machineColorPref.Value);
        }

        if (Singleton<LabOvenCanvas>.InstanceExists)
        {
            Singleton<LabOvenCanvas>.instance.IngredientSlotUI.SetNormalColor(_machineColorPref.Value);
            Singleton<LabOvenCanvas>.instance.OutputSlotUI.SetNormalColor(_machineColorPref.Value);
        }
        
        if (Singleton<ChemistryStationCanvas>.InstanceExists)
        {
            Singleton<ChemistryStationCanvas>.instance.OutputSlotUI.SetNormalColor(_machineColorPref.Value);
            foreach (var inputSlotUI in Singleton<ChemistryStationCanvas>.instance.InputSlotUIs)
                inputSlotUI.SetNormalColor(_machineColorPref.Value);
        }
    }

    public override void OnDeinitializeMelon()
    {
#if MONO
        return;
#else
        UnsubscribeFromModManagerEvents();
#endif
    }
    
    private void SubscribeToModManagerEvents()
    {
        
        try
        {
            ModSettingsEvents.OnPhonePreferencesSaved += HandleSettingsUpdate;
            MelonLogger.Msg(System.ConsoleColor.DarkCyan,"Subscribed to OnPhonePreferencesSaved event.");
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Failed to subscribe to OnPhonePreferencesSaved: {e}");
        }

        try
        {
            ModSettingsEvents.OnMenuPreferencesSaved += HandleSettingsUpdate;
            MelonLogger.Msg(System.ConsoleColor.DarkCyan,"Subscribed to OnMenuPreferencesSaved event.");            
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Failed to subscribe to OnPhonePreferencesSaved: {e}");
        }
    }

    private void UnsubscribeFromModManagerEvents()
    {
#if MONO
        return;
#else
        try
        {
            ModSettingsEvents.OnPhonePreferencesSaved -= HandleSettingsUpdate;
            MelonLogger.Msg(System.ConsoleColor.DarkCyan,"Unsubscribed from OnPhonePreferencesSaved event.");
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Failed to unsubscribe from OnPhonePreferencesSaved: {e}");
        }

        try
        {
            ModSettingsEvents.OnMenuPreferencesSaved -= HandleSettingsUpdate;
            MelonLogger.Msg(System.ConsoleColor.DarkCyan,"Unsubscribed from OnMenuPreferencesSaved event.");
        }
        catch (Exception e)
        {
            MelonLogger.Error($"Failed to unsubscribe from OnMenuPreferencesSaved: {e}");
        } 
#endif
    }

    private void HandleSettingsUpdate()
    {
        _hotbarColorPref = _category.GetEntry<Color>("HotbarColor");
        _hotbarHighlightColorPref = _category.GetEntry<Color>("HotbarHighlightColor");
        _storageColorPref = _category.GetEntry<Color>("StorageColor");
        _machineColorPref = _category.GetEntry<Color>("MachineColor");
        _colorsModified = true;
    }   
    
}