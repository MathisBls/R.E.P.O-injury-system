using HarmonyLib;
using UnityEngine;

namespace InjurySystem.Patches;

/// <summary>
/// Patches PlayerController to reduce movement speed when legs are injured.
/// Also applies stamina drain penalty for torso injuries.
/// </summary>
[HarmonyPatch(typeof(PlayerController))]
static class MovementPatch
{
    [HarmonyPostfix, HarmonyPatch("FixedUpdate")]
    static void FixedUpdate_Postfix(PlayerController __instance)
    {
        if (__instance == null)
            return;

        var avatar = PlayerAvatar.instance;
        if (avatar == null || avatar.playerHealth == null)
            return;
        if (avatar.playerHealth.health <= 0)
            return;

        string playerId = InjuryManager.GetPlayerId(avatar);
        var state = InjuryManager.GetOrCreateState(playerId);

        ApplyLegPenalty(__instance, state);
        ApplyTorsoPenalty(__instance, state);
    }

    private static void ApplyLegPenalty(PlayerController controller, InjuryState state)
    {
        var legInjury = state.GetWorstLegInjury();
        if (legInjury == Severity.Healthy)
            return;

        float penalty = legInjury == Severity.Minor
            ? InjurySystem.LegSpeedPenaltyMinor.Value
            : InjurySystem.LegSpeedPenaltySevere.Value;

        // Reduce current velocity directly as a speed modifier
        var rb = controller.GetComponent<Rigidbody>();
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            Vector3 vel = rb.velocity;
            // Only reduce horizontal velocity, not vertical (falling/jumping)
            vel.x *= (1f - penalty);
            vel.z *= (1f - penalty);
            rb.velocity = vel;
        }

        // Prevent sprinting with severe leg injury
        if (legInjury == Severity.Severe && controller.sprinting)
        {
            controller.sprinting = false;
        }
    }

    private static void ApplyTorsoPenalty(PlayerController controller, InjuryState state)
    {
        if (state.Torso == Severity.Healthy)
            return;

        if (controller.EnergyCurrent <= 0)
            return;

        // Only drain faster when sprinting
        if (controller.sprinting)
        {
            float sprintDrain = state.Torso == Severity.Minor ? 4f : 9f;
            controller.EnergyCurrent -= Time.fixedDeltaTime * sprintDrain;
        }

        controller.EnergyCurrent = Mathf.Max(0f, controller.EnergyCurrent);
    }

}
