using System;
using System.Collections.Generic;
using UnityEngine;

namespace InjurySystem;

public static class InjuryManager
{
    private static readonly Dictionary<string, InjuryState> PlayerInjuries = new();
    private static readonly System.Random Rng = new();

    /// <summary>
    /// Gets a stable player ID that persists between levels.
    /// Uses the player's name instead of InstanceID which changes on scene reload.
    /// </summary>
    public static string GetPlayerId(PlayerAvatar avatar)
    {
        if (avatar == null) return "unknown";

        // Try to get the player's Steam name or display name
        try
        {
            if (!string.IsNullOrEmpty(avatar.playerName))
                return avatar.playerName;
        }
        catch { }

        // Fallback to instance ID
        return avatar.GetInstanceID().ToString();
    }

    // Body parts weighted by likelihood of being hit
    private static readonly (BodyPart part, int weight)[] BodyPartWeights =
    {
        (BodyPart.Torso, 30),
        (BodyPart.LeftLeg, 20),
        (BodyPart.RightLeg, 20),
        (BodyPart.LeftArm, 12),
        (BodyPart.RightArm, 12),
        (BodyPart.Head, 6)
    };

    private static int _totalWeight;

    public static void Initialize()
    {
        PlayerInjuries.Clear();
        _totalWeight = 0;
        foreach (var (_, weight) in BodyPartWeights)
            _totalWeight += weight;

        InjurySystem.Logger.LogInfo("InjuryManager initialized.");
    }

    public static InjuryState GetOrCreateState(string playerId)
    {
        if (!PlayerInjuries.TryGetValue(playerId, out var state))
        {
            state = new InjuryState();
            PlayerInjuries[playerId] = state;
        }
        return state;
    }

    /// <summary>
    /// Called when a player takes damage. Rolls for a random injury.
    /// </summary>
    public static BodyPart? ApplyRandomInjury(string playerId, int damage)
    {
        float chance = InjurySystem.InjuryChanceOnHit.Value;

        // Higher damage = higher chance of injury
        float damageBonus = Math.Min(damage * 0.02f, 0.3f);
        float finalChance = Math.Min(chance + damageBonus, 0.95f);

        if (Rng.NextDouble() > finalChance)
            return null;

        var state = GetOrCreateState(playerId);

        // Roll a body part, re-roll up to 5 times if already at max severity
        BodyPart? availablePart = null;
        for (int attempt = 0; attempt < 5; attempt++)
        {
            var rolled = RollBodyPart();
            if (state.GetSeverity(rolled) < Severity.Severe)
            {
                availablePart = rolled;
                break;
            }
        }

        // If all rolls landed on maxed parts, find any non-maxed part
        if (!availablePart.HasValue)
        {
            foreach (BodyPart bp in Enum.GetValues(typeof(BodyPart)))
            {
                if (state.GetSeverity(bp) < Severity.Severe)
                {
                    availablePart = bp;
                    break;
                }
            }
        }

        // All body parts are Severe - nothing left to injure
        if (!availablePart.HasValue)
            return null;

        var part = availablePart.Value;
        var current = state.GetSeverity(part);
        var newSeverity = current + 1;
        state.SetSeverity(part, (Severity)newSeverity);

        InjurySystem.Logger.LogInfo(
            $"Player {playerId} injured: {part} -> {(Severity)newSeverity} (damage={damage})");

        return part;
    }

    public static void HealPlayer(string playerId)
    {
        var state = GetOrCreateState(playerId);
        state.HealAll();
        InjurySystem.Logger.LogInfo($"Player {playerId} fully healed.");
    }

    public static void HealPart(string playerId, BodyPart part)
    {
        var state = GetOrCreateState(playerId);
        state.HealPart(part);
        InjurySystem.Logger.LogInfo($"Player {playerId} healed: {part}");
    }

    public static void ResetAll()
    {
        PlayerInjuries.Clear();
        InjurySystem.Logger.LogInfo("All injuries reset.");
    }

    private static BodyPart RollBodyPart()
    {
        int roll = Rng.Next(_totalWeight);
        int cumulative = 0;
        foreach (var (part, weight) in BodyPartWeights)
        {
            cumulative += weight;
            if (roll < cumulative)
                return part;
        }
        return BodyPart.Torso;
    }
}
