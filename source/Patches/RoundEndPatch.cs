using HarmonyLib;

namespace InjurySystem.Patches;

/// <summary>
/// Resets all injuries when returning to the lobby (round end / extraction).
/// </summary>
[HarmonyPatch(typeof(RoundDirector), nameof(RoundDirector.ExtractionCompleted))]
public static class ExtractionPatch
{
    static void Postfix()
    {
        InjuryManager.ResetAll();
    }
}

[HarmonyPatch(typeof(RoundDirector), nameof(RoundDirector.RoundOver))]
public static class RoundOverPatch
{
    static void Postfix()
    {
        InjuryManager.ResetAll();
    }
}
