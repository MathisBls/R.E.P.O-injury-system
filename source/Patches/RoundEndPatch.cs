using HarmonyLib;

namespace InjurySystem.Patches;

/// <summary>
/// Resets all injuries when extraction is completed (end of round).
/// </summary>
[HarmonyPatch(typeof(RoundDirector), nameof(RoundDirector.ExtractionCompleted))]
public static class ExtractionPatch
{
    static void Postfix()
    {
        InjuryManager.ResetAll();
    }
}
