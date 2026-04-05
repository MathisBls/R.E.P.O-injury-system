using HarmonyLib;
using UnityEngine;

namespace InjurySystem.Patches;

/// <summary>
/// Debug commands for testing injuries in-game.
/// Press F8 to inflict a random injury on yourself.
/// Press F9 to heal all injuries.
/// Only active in debug/testing - remove or disable for release.
/// </summary>
[HarmonyPatch(typeof(PlayerController))]
static class DebugPatch
{
    [HarmonyPostfix, HarmonyPatch("Update")]
    static void Update_Postfix(PlayerController __instance)
    {
        if (__instance == null) return;

        var avatar = PlayerAvatar.instance;
        if (avatar == null) return;

        string playerId = InjuryManager.GetPlayerId(avatar);

        // F8 = Apply random injury for testing
        if (Input.GetKeyDown(KeyCode.F8))
        {
            var part = InjuryManager.ApplyRandomInjury(playerId, 25);
            if (part.HasValue)
            {
                var state = InjuryManager.GetOrCreateState(playerId);
                InjuryNotification.Show(part.Value, state.GetSeverity(part.Value));
                InjurySystem.Logger.LogWarning(
                    $"[DEBUG] Forced injury: {part.Value} -> {state.GetSeverity(part.Value)}");
            }
            else
            {
                InjurySystem.Logger.LogWarning("[DEBUG] No injury applied (already max or bad roll)");
            }
        }

        // F9 = Heal all injuries
        if (Input.GetKeyDown(KeyCode.F9))
        {
            InjuryManager.HealPlayer(playerId);
            InjuryNotification.ShowFullHeal();
            InjurySystem.Logger.LogWarning("[DEBUG] All injuries healed!");
        }
    }
}
