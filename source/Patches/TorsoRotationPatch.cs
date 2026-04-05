using UnityEngine;

namespace InjurySystem;

/// <summary>
/// MonoBehaviour attached to the main camera that smoothly dampens rotation
/// when the player has a torso injury. Uses LateUpdate + Slerp for safe
/// rotation interpolation that can never flip the camera.
/// Minor: slight rotation lag (25% damping).
/// Severe: heavy rotation lag (55%) + brief pain stutters.
/// </summary>
public class TorsoCameraDamper : MonoBehaviour
{
    private Quaternion _prevCamRot;
    private Quaternion _prevParentRot;
    private bool _initialized;
    private float _stutterTimer;

    private void LateUpdate()
    {
        // Don't apply in menus
        if (Cursor.visible)
        {
            _initialized = false;
            return;
        }

        var avatar = PlayerAvatar.instance;
        if (avatar == null)
        {
            _initialized = false;
            return;
        }

        string playerId = InjuryManager.GetPlayerId(avatar);
        var state = InjuryManager.GetOrCreateState(playerId);

        if (state.Torso == Severity.Healthy)
        {
            _initialized = false;
            return;
        }

        bool severe = state.Torso == Severity.Severe;
        float damping = severe ? 0.55f : 0.25f;

        // Pain stutter for severe - briefly increase damping
        if (severe)
        {
            _stutterTimer += Time.deltaTime;
            if (Mathf.Sin(_stutterTimer * 3.5f) > 0.93f)
                damping = 0.8f;
        }

        // First frame: just record current rotations
        if (!_initialized)
        {
            _prevCamRot = transform.localRotation;
            if (transform.parent != null)
                _prevParentRot = transform.parent.localRotation;
            _initialized = true;
            return;
        }

        // Slerp between previous and current rotation
        // damping=0.25 means we take 75% of the new rotation + 25% of the old
        // This creates a sluggish/heavy feel without ever flipping

        float smoothFactor = 1f - damping;

        // Dampen camera pitch
        Quaternion targetCamRot = transform.localRotation;
        transform.localRotation = Quaternion.Slerp(_prevCamRot, targetCamRot, smoothFactor);
        _prevCamRot = transform.localRotation;

        // Dampen player body yaw
        if (transform.parent != null)
        {
            Quaternion targetParentRot = transform.parent.localRotation;
            transform.parent.localRotation = Quaternion.Slerp(_prevParentRot, targetParentRot, smoothFactor);
            _prevParentRot = transform.parent.localRotation;
        }
    }
}
