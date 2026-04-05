using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace InjurySystem;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class InjurySystem : BaseUnityPlugin
{
    public const string PluginGUID = "Asuki.InjurySystem";
    public const string PluginName = "InjurySystem";
    public const string PluginVersion = "1.0.0";

    internal static InjurySystem Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }

    // Config entries
    internal static ConfigEntry<float> LegSpeedPenaltyMinor = null!;
    internal static ConfigEntry<float> LegSpeedPenaltySevere = null!;
    internal static ConfigEntry<float> ArmGripPenaltyMinor = null!;
    internal static ConfigEntry<float> ArmGripPenaltySevere = null!;
    internal static ConfigEntry<float> HeadShakeIntensity = null!;
    internal static ConfigEntry<float> InjuryChanceOnHit = null!;
    internal static ConfigEntry<int> HealKitCost = null!;
    internal static ConfigEntry<KeyCode> ToggleHudKey = null!;

    private InjuryHUD? _hud;
    private HeadInjuryOverlay? _headOverlay;
    private TorsoCameraDamper? _torsoDamper;

    private void Awake()
    {
        Instance = this;

        this.gameObject.transform.parent = null;
        this.gameObject.hideFlags = HideFlags.HideAndDontSave;

        InitConfig();

        InjuryManager.Initialize();

        Patch();

        _hud = gameObject.AddComponent<InjuryHUD>();
        _headOverlay = gameObject.AddComponent<HeadInjuryOverlay>();
        gameObject.AddComponent<InjuryNotification>();

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded!");
    }

    private void Update()
    {
        // Attach torso damper to camera when it becomes available
        if (_torsoDamper == null && Camera.main != null)
        {
            _torsoDamper = Camera.main.gameObject.GetComponent<TorsoCameraDamper>();
            if (_torsoDamper == null)
                _torsoDamper = Camera.main.gameObject.AddComponent<TorsoCameraDamper>();
        }
    }

    private void InitConfig()
    {
        LegSpeedPenaltyMinor = Config.Bind("Legs", "MinorSpeedPenalty", 0.25f,
            "Speed reduction for minor leg injury (0.25 = 25% slower)");
        LegSpeedPenaltySevere = Config.Bind("Legs", "SevereSpeedPenalty", 0.50f,
            "Speed reduction for severe leg injury (0.50 = 50% slower)");

        ArmGripPenaltyMinor = Config.Bind("Arms", "MinorGripPenalty", 0.30f,
            "Grip strength reduction for minor arm injury (0.30 = 30% weaker)");
        ArmGripPenaltySevere = Config.Bind("Arms", "SevereGripPenalty", 0.60f,
            "Grip strength reduction for severe arm injury (0.60 = 60% weaker)");

        HeadShakeIntensity = Config.Bind("Head", "ShakeIntensity", 0.08f,
            "Camera shake intensity for head injury");

        InjuryChanceOnHit = Config.Bind("General", "InjuryChanceOnHit", 0.6f,
            "Chance of getting an injury when taking damage (0.0-1.0)");

        HealKitCost = Config.Bind("Shop", "HealKitCost", 50,
            "Cost of the Medkit in the shop");

        ToggleHudKey = Config.Bind("UI", "ToggleHudKey", KeyCode.J,
            "Key to toggle the injury HUD on/off");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(PluginGUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }
}
