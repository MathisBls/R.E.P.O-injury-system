using UnityEngine;

namespace InjurySystem;

/// <summary>
/// Minimal injury status display near the health bar area.
/// Styled to match REPO's HUD aesthetic (Teko font, green/yellow/red, glow).
/// Only shows active injuries as small compact indicators.
/// Toggle with J key.
/// </summary>
public class InjuryHUD : MonoBehaviour
{
    private bool _visible = true;
    private static Font? _gameFont;
    private static bool _fontSearched;

    private void Update()
    {
        if (Input.GetKeyDown(InjurySystem.ToggleHudKey.Value))
            _visible = !_visible;
    }

    private void OnGUI()
    {
        if (!_visible) return;

        // Only show in-game (not in lobby/menus)
        if (Cursor.visible) return;

        var avatar = PlayerAvatar.instance;
        if (avatar == null) return;

        // Hide HUD when dead
        if (avatar.playerHealth == null) return;
        if (avatar.playerHealth.health <= 0) return;

        string playerId = InjuryManager.GetPlayerId(avatar);
        var state = InjuryManager.GetOrCreateState(playerId);

        if (!state.HasAnyInjury()) return;

        FindGameFont();

        // Position: well below the health/stamina display
        float x = 30f;
        float y = 250f;

        DrawInjuryIndicators(x, y, state);
    }

    private void DrawInjuryIndicators(float x, float y, InjuryState state)
    {
        var parts = new (string label, Severity sev)[]
        {
            ("HEAD", state.Head),
            ("L.ARM", state.LeftArm),
            ("R.ARM", state.RightArm),
            ("TORSO", state.Torso),
            ("L.LEG", state.LeftLeg),
            ("R.LEG", state.RightLeg)
        };

        var style = new GUIStyle
        {
            font = _gameFont,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        var sevStyle = new GUIStyle(style)
        {
            alignment = TextAnchor.MiddleRight
        };

        foreach (var (label, sev) in parts)
        {
            if (sev == Severity.Healthy) continue;

            Color color;
            string sevText;
            if (sev == Severity.Minor)
            {
                color = new Color(1f, 0.92f, 0.02f, 0.9f); // Yellow
                sevText = "MINOR";
            }
            else
            {
                color = new Color(1f, 0.2f, 0.15f, 0.95f); // Red
                sevText = "SEVERE";
            }

            // Glow behind text
            style.normal.textColor = new Color(color.r, color.g, color.b, 0.15f);
            style.fontSize = 20;
            GUI.Label(new Rect(x - 1, y - 1, 80, 24), label, style);
            style.fontSize = 18;

            // Chromatic aberration
            style.normal.textColor = new Color(1f, 0f, 0f, 0.2f);
            GUI.Label(new Rect(x - 1, y, 80, 22), label, style);
            style.normal.textColor = new Color(0f, 0.3f, 1f, 0.2f);
            GUI.Label(new Rect(x + 1, y, 80, 22), label, style);

            // Shadow
            style.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
            GUI.Label(new Rect(x + 1, y + 1, 80, 22), label, style);

            // Label text
            style.normal.textColor = new Color(0.8f, 0.85f, 0.8f, 0.9f);
            GUI.Label(new Rect(x, y, 80, 22), label, style);

            // Severity indicator with glow
            float sevX = x + 75f;

            // Glow
            sevStyle.normal.textColor = new Color(color.r, color.g, color.b, 0.12f);
            sevStyle.fontSize = 20;
            GUI.Label(new Rect(sevX - 1, y - 1, 70, 24), sevText, sevStyle);
            sevStyle.fontSize = 18;

            // Aberration
            sevStyle.normal.textColor = new Color(1f, 0f, 0f, 0.2f);
            GUI.Label(new Rect(sevX - 1, y, 70, 22), sevText, sevStyle);
            sevStyle.normal.textColor = new Color(0f, 0.3f, 1f, 0.2f);
            GUI.Label(new Rect(sevX + 1, y, 70, 22), sevText, sevStyle);

            // Shadow
            sevStyle.normal.textColor = new Color(0f, 0f, 0f, 0.5f);
            GUI.Label(new Rect(sevX + 1, y + 1, 70, 22), sevText, sevStyle);

            // Main severity text
            sevStyle.normal.textColor = color;
            GUI.Label(new Rect(sevX, y, 70, 22), sevText, sevStyle);

            y += 24f;
        }
    }

    private static void FindGameFont()
    {
        if (_fontSearched) return;
        _fontSearched = true;

        var allFonts = Resources.FindObjectsOfTypeAll<Font>();
        foreach (var f in allFonts)
        {
            string name = f.name.ToLowerInvariant();
            if (name.Contains("teko"))
            {
                _gameFont = f;
                return;
            }
        }
        foreach (var f in allFonts)
        {
            string name = f.name.ToLowerInvariant();
            if (name != "arial" && name != "liberation sans" && !name.Contains("legacy"))
            {
                _gameFont = f;
                return;
            }
        }
    }
}
