using System.Collections.Generic;
using UnityEngine;

namespace InjurySystem;

/// <summary>
/// Shows injury notifications in the same style as REPO's upgrade popups.
/// Big centered text with glow + chromatic aberration + fade animation.
/// Uses the game's Teko font grabbed at runtime.
/// </summary>
public class InjuryNotification : MonoBehaviour
{
    private struct Notification
    {
        public string Text;
        public string SubText;
        public Color BaseColor;
        public float SpawnTime;
        public float Duration;
    }

    private static readonly List<Notification> ActiveNotifications = new();
    private static Font? _gameFont;
    private static bool _fontSearched;

    private static readonly Dictionary<BodyPart, string> MinorNames = new()
    {
        { BodyPart.Head, "CONCUSSION" },
        { BodyPart.LeftArm, "SPRAINED LEFT ARM" },
        { BodyPart.RightArm, "SPRAINED RIGHT ARM" },
        { BodyPart.LeftLeg, "SPRAINED LEFT LEG" },
        { BodyPart.RightLeg, "SPRAINED RIGHT LEG" },
        { BodyPart.Torso, "BRUISED RIBS" }
    };

    private static readonly Dictionary<BodyPart, string> SevereNames = new()
    {
        { BodyPart.Head, "SKULL FRACTURE" },
        { BodyPart.LeftArm, "BROKEN LEFT ARM" },
        { BodyPart.RightArm, "BROKEN RIGHT ARM" },
        { BodyPart.LeftLeg, "BROKEN LEFT LEG" },
        { BodyPart.RightLeg, "BROKEN RIGHT LEG" },
        { BodyPart.Torso, "CRACKED RIBS" }
    };

    private static readonly Dictionary<BodyPart, string> MinorSub = new()
    {
        { BodyPart.Head, "- VISION" },
        { BodyPart.LeftArm, "- GRIP" },
        { BodyPart.RightArm, "- GRIP" },
        { BodyPart.LeftLeg, "- SPEED" },
        { BodyPart.RightLeg, "- SPEED" },
        { BodyPart.Torso, "- STAMINA" }
    };

    private static readonly Dictionary<BodyPart, string> SevereSub = new()
    {
        { BodyPart.Head, "-- VISION" },
        { BodyPart.LeftArm, "-- GRIP" },
        { BodyPart.RightArm, "-- GRIP" },
        { BodyPart.LeftLeg, "-- SPEED" },
        { BodyPart.RightLeg, "-- SPEED" },
        { BodyPart.Torso, "-- STAMINA" }
    };

    private static readonly Dictionary<BodyPart, string> HealSub = new()
    {
        { BodyPart.Head, "+ VISION" },
        { BodyPart.LeftArm, "+ GRIP" },
        { BodyPart.RightArm, "+ GRIP" },
        { BodyPart.LeftLeg, "+ SPEED" },
        { BodyPart.RightLeg, "+ SPEED" },
        { BodyPart.Torso, "+ STAMINA" }
    };

    private static readonly Dictionary<BodyPart, string> HealNames = new()
    {
        { BodyPart.Head, "HEAD HEALED" },
        { BodyPart.LeftArm, "LEFT ARM HEALED" },
        { BodyPart.RightArm, "RIGHT ARM HEALED" },
        { BodyPart.LeftLeg, "LEFT LEG HEALED" },
        { BodyPart.RightLeg, "RIGHT LEG HEALED" },
        { BodyPart.Torso, "TORSO HEALED" }
    };

    public static void Show(BodyPart part, Severity severity)
    {
        if (severity == Severity.Healthy) return;

        var names = severity == Severity.Minor ? MinorNames : SevereNames;
        var subs = severity == Severity.Minor ? MinorSub : SevereSub;

        string text = names.TryGetValue(part, out var n) ? n : $"{part}";
        string sub = subs.TryGetValue(part, out var s) ? s : "";

        Color color = severity == Severity.Minor
            ? new Color(1f, 0.92f, 0.02f)
            : new Color(1f, 0.2f, 0.15f);

        ActiveNotifications.Add(new Notification
        {
            Text = text,
            SubText = sub,
            BaseColor = color,
            SpawnTime = Time.time,
            Duration = 2.8f
        });
    }

    /// <summary>
    /// Shows a green heal notification when a body part is healed.
    /// </summary>
    public static void ShowHeal(BodyPart part)
    {
        string text = HealNames.TryGetValue(part, out var n) ? n : $"{part} HEALED";
        string sub = HealSub.TryGetValue(part, out var s) ? s : "";

        ActiveNotifications.Add(new Notification
        {
            Text = text,
            SubText = sub,
            BaseColor = new Color(0.2f, 1f, 0.3f), // Green like health
            SpawnTime = Time.time,
            Duration = 2.5f
        });
    }

    /// <summary>
    /// Shows a green notification when all injuries are healed.
    /// </summary>
    public static void ShowFullHeal()
    {
        ActiveNotifications.Add(new Notification
        {
            Text = "FULLY HEALED",
            SubText = "+ ALL STATS RESTORED",
            BaseColor = new Color(0.2f, 1f, 0.3f),
            SpawnTime = Time.time,
            Duration = 2.8f
        });
    }

    private void OnGUI()
    {
        if (ActiveNotifications.Count == 0) return;
        if (Cursor.visible) return;

        FindGameFont();

        float centerX = Screen.width / 2f;
        float centerY = Screen.height * 0.30f;

        for (int i = ActiveNotifications.Count - 1; i >= 0; i--)
        {
            var notif = ActiveNotifications[i];
            float elapsed = Time.time - notif.SpawnTime;

            if (elapsed > notif.Duration)
            {
                ActiveNotifications.RemoveAt(i);
                continue;
            }

            float alpha = CalcAlpha(elapsed, notif.Duration);
            float scale = CalcScale(elapsed);
            float y = centerY + i * 80f;

            DrawUpgradeStyleText(notif.Text, notif.SubText, notif.BaseColor, alpha, scale, centerX, y);
        }
    }

    private static float CalcAlpha(float elapsed, float duration)
    {
        // Fast fade in (0-0.15s), hold, fade out (last 0.6s)
        if (elapsed < 0.15f)
            return elapsed / 0.15f;
        if (elapsed > duration - 0.6f)
            return (duration - elapsed) / 0.6f;
        return 1f;
    }

    private static float CalcScale(float elapsed)
    {
        // Slam in: starts at 1.4x, snaps to 1.0x in 0.15s
        if (elapsed < 0.15f)
        {
            float t = elapsed / 0.15f;
            return 1f + (1f - t) * 0.4f;
        }
        return 1f;
    }

    private void DrawUpgradeStyleText(string text, string subText, Color baseColor, float alpha, float scale, float cx, float cy)
    {
        int mainSize = Mathf.RoundToInt(52 * scale);
        int subSize = Mathf.RoundToInt(30 * scale);

        var mainStyle = MakeStyle(mainSize);
        var subStyle = MakeStyle(subSize);

        var mainContent = new GUIContent(text);
        var mainSizeVec = mainStyle.CalcSize(mainContent);
        float mainX = cx - mainSizeVec.x / 2f;

        // === GLOW (draw enlarged blurred copies behind) ===
        Color glowColor = baseColor;
        glowColor.a = alpha * 0.12f;
        var glowStyle = MakeStyle(mainSize + 4);
        glowStyle.normal.textColor = glowColor;
        for (int g = 3; g >= 1; g--)
        {
            glowStyle.fontSize = mainSize + g * 3;
            glowStyle.normal.textColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha * 0.06f * g);
            var gs = glowStyle.CalcSize(mainContent);
            GUI.Label(new Rect(cx - gs.x / 2f, cy - (gs.y - mainSizeVec.y) / 2f, gs.x + 10, gs.y + 5), text, glowStyle);
        }

        // === CHROMATIC ABERRATION (RGB split) ===
        float aberration = 2f * scale;

        // Red channel offset left
        mainStyle.normal.textColor = new Color(1f, 0f, 0f, alpha * 0.35f);
        GUI.Label(new Rect(mainX - aberration, cy, mainSizeVec.x + 10, mainSizeVec.y + 5), text, mainStyle);

        // Blue channel offset right
        mainStyle.normal.textColor = new Color(0f, 0.3f, 1f, alpha * 0.35f);
        GUI.Label(new Rect(mainX + aberration, cy, mainSizeVec.x + 10, mainSizeVec.y + 5), text, mainStyle);

        // === MAIN TEXT ===
        // Shadow
        mainStyle.normal.textColor = new Color(0f, 0f, 0f, alpha * 0.6f);
        GUI.Label(new Rect(mainX + 2, cy + 2, mainSizeVec.x + 10, mainSizeVec.y + 5), text, mainStyle);

        // Main colored text
        mainStyle.normal.textColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        GUI.Label(new Rect(mainX, cy, mainSizeVec.x + 10, mainSizeVec.y + 5), text, mainStyle);

        // === SUB TEXT (stat affected) ===
        if (!string.IsNullOrEmpty(subText))
        {
            var subContent = new GUIContent(subText);
            var subSizeVec = subStyle.CalcSize(subContent);
            float subX = cx - subSizeVec.x / 2f;
            float subY = cy + mainSizeVec.y - 5f;

            // Sub shadow
            subStyle.normal.textColor = new Color(0f, 0f, 0f, alpha * 0.5f);
            GUI.Label(new Rect(subX + 1, subY + 1, subSizeVec.x + 10, subSizeVec.y + 5), subText, subStyle);

            // Sub text in white
            subStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f, alpha * 0.9f);
            GUI.Label(new Rect(subX, subY, subSizeVec.x + 10, subSizeVec.y + 5), subText, subStyle);
        }
    }

    private GUIStyle MakeStyle(int fontSize)
    {
        return new GUIStyle
        {
            font = _gameFont,
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }

    private static void FindGameFont()
    {
        if (_fontSearched) return;
        _fontSearched = true;

        // Search for the game's font (Teko) in loaded resources
        var allFonts = Resources.FindObjectsOfTypeAll<Font>();
        foreach (var f in allFonts)
        {
            string name = f.name.ToLowerInvariant();
            if (name.Contains("teko"))
            {
                _gameFont = f;
                InjurySystem.Logger.LogInfo($"[InjurySystem] Found game font: {f.name}");
                return;
            }
        }

        // Fallback: try to find any non-default font the game uses
        foreach (var f in allFonts)
        {
            string name = f.name.ToLowerInvariant();
            if (name != "arial" && name != "liberation sans" && !name.Contains("legacy"))
            {
                _gameFont = f;
                InjurySystem.Logger.LogInfo($"[InjurySystem] Using font: {f.name}");
                return;
            }
        }

        InjurySystem.Logger.LogWarning("[InjurySystem] Game font not found, using default.");
    }
}
