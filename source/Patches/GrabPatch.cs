using HarmonyLib;
using UnityEngine;

namespace InjurySystem.Patches;

/// <summary>
/// Patches PhysGrabber to reduce grab strength when arms are injured.
/// With severe arm injuries, the player may randomly drop objects.
/// </summary>
[HarmonyPatch(typeof(PhysGrabber))]
static class GrabPatch
{
    private static float _originalGrabStrength = -1f;
    private static float _dropTimer;

    [HarmonyPostfix, HarmonyPatch("Update")]
    static void Update_Postfix(PhysGrabber __instance)
    {
        if (__instance == null)
            return;

        var avatar = PlayerAvatar.instance;
        if (avatar == null)
            return;

        string playerId = InjuryManager.GetPlayerId(avatar);
        var state = InjuryManager.GetOrCreateState(playerId);

        var armInjury = state.GetWorstArmInjury();
        if (armInjury == Severity.Healthy)
            return;

        // Store original grab strength on first access
        if (_originalGrabStrength < 0f)
            _originalGrabStrength = __instance.grabStrength;

        // Reduce grab strength based on arm injury
        float penalty = armInjury == Severity.Minor
            ? InjurySystem.ArmGripPenaltyMinor.Value
            : InjurySystem.ArmGripPenaltySevere.Value;

        __instance.grabStrength = _originalGrabStrength * (1f - penalty);

        // Severe arm injury: chance to drop objects periodically
        if (armInjury == Severity.Severe && __instance.grabbedObject != null)
        {
            _dropTimer += Time.deltaTime;
            if (_dropTimer >= 3f) // Check every 3 seconds
            {
                _dropTimer = 0f;
                if (Random.value < 0.2f) // 20% chance to drop
                {
                    __instance.grabbedObject = null;
                    InjurySystem.Logger.LogInfo("[InjurySystem] Dropped object due to severe arm injury!");
                }
            }
        }
    }

    /// <summary>
    /// Reset original grab strength when no longer grabbing.
    /// </summary>
    [HarmonyPostfix, HarmonyPatch("Start")]
    static void Start_Postfix(PhysGrabber __instance)
    {
        if (__instance != null)
            _originalGrabStrength = __instance.grabStrength;
    }
}
