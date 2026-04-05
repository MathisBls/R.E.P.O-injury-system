using HarmonyLib;

namespace InjurySystem.Patches;

/// <summary>
/// Intercepts PlayerHealth.Hurt() to apply a random injury when the player takes damage.
/// Also intercepts PlayerHealth.Heal() to clear torso injuries on full heal.
/// </summary>
[HarmonyPatch(typeof(PlayerHealth))]
static class DamagePatch
{
    [HarmonyPostfix, HarmonyPatch("Hurt")]
    static void Hurt_Postfix(PlayerHealth __instance, int damage)
    {
        if (__instance == null || __instance.playerAvatar == null)
            return;

        // Use the player's steam ID or instance ID as unique identifier
        string playerId = InjuryManager.GetPlayerId(__instance.playerAvatar);

        var injuredPart = InjuryManager.ApplyRandomInjury(playerId, damage);

        if (injuredPart.HasValue)
        {
            var state = InjuryManager.GetOrCreateState(playerId);
            var severity = state.GetSeverity(injuredPart.Value);

            // Show on-screen notification
            InjuryNotification.Show(injuredPart.Value, severity);

            InjurySystem.Logger.LogInfo(
                $"[InjurySystem] {severity} injury to {injuredPart.Value}!");
        }
    }
}
