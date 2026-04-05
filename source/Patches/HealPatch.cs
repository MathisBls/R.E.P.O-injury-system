using HarmonyLib;

namespace InjurySystem.Patches;

/// <summary>
/// Patches PlayerHealth.Heal() to heal injuries when the player heals HP.
/// Teammate heal (1 HP) = heal one injury one level.
/// Big heal (25+ HP) = heal up to 3 injuries.
/// </summary>
[HarmonyPatch(typeof(PlayerHealth))]
static class HealPatch
{
    [HarmonyPostfix, HarmonyPatch("Heal")]
    static void Heal_Postfix(PlayerHealth __instance, int healAmount)
    {
        if (__instance == null || __instance.playerAvatar == null)
            return;

        string playerId = InjuryManager.GetPlayerId(__instance.playerAvatar);
        var state = InjuryManager.GetOrCreateState(playerId);

        if (!state.HasAnyInjury())
            return;

        // Big heal (health pack / shop medkit) = heal up to 3 injuries
        if (healAmount >= 25)
        {
            for (int i = 0; i < 3; i++)
            {
                var worst = FindWorstInjury(state);
                if (!worst.HasValue) break;

                var sev = state.GetSeverity(worst.Value);
                var newSev = (Severity)((int)sev - 1);
                state.SetSeverity(worst.Value, newSev);
                InjuryNotification.ShowHeal(worst.Value);
            }

            if (!state.HasAnyInjury())
                InjuryNotification.ShowFullHeal();

            InjurySystem.Logger.LogInfo("[InjurySystem] Big heal - healed up to 3 injuries!");
        }
        // Small heal (teammate heal = 1 HP) = heal the worst injury by one level
        else if (healAmount >= 1)
        {
            var worstPart = FindWorstInjury(state);

            if (worstPart.HasValue)
            {
                var worstSev = state.GetSeverity(worstPart.Value);
                var newSev = (Severity)((int)worstSev - 1);
                state.SetSeverity(worstPart.Value, newSev);
                InjuryNotification.ShowHeal(worstPart.Value);
                InjurySystem.Logger.LogInfo(
                    $"[InjurySystem] Partial heal: {worstPart.Value} {worstSev} -> {newSev}");
            }
        }
    }

    private static BodyPart? FindWorstInjury(InjuryState state)
    {
        BodyPart? worstPart = null;
        Severity worstSev = Severity.Healthy;

        foreach (BodyPart part in System.Enum.GetValues(typeof(BodyPart)))
        {
            var sev = state.GetSeverity(part);
            if (sev > worstSev)
            {
                worstSev = sev;
                worstPart = part;
            }
        }

        return worstPart;
    }
}
