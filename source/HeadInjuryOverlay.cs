using UnityEngine;

namespace InjurySystem;

/// <summary>
/// Applies camera blur for head injuries using OnRenderImage.
/// Minor: subtle blur.
/// Severe: strong blur + red flashes.
/// No GUI rectangles - blur only.
/// </summary>
public class HeadInjuryOverlay : MonoBehaviour
{
    private HeadInjuryCameraEffect? _cameraEffect;

    private void Update()
    {
        var avatar = PlayerAvatar.instance;
        if (avatar == null || Cursor.visible)
        {
            if (_cameraEffect != null)
                _cameraEffect.enabled = false;
            return;
        }

        // Disable when dead
        bool isDead = avatar.playerHealth == null || avatar.playerHealth.health <= 0;
        if (isDead)
        {
            if (_cameraEffect != null)
                _cameraEffect.enabled = false;
            return;
        }

        string playerId = InjuryManager.GetPlayerId(avatar);
        var state = InjuryManager.GetOrCreateState(playerId);

        if (state.Head != Severity.Healthy)
        {
            EnsureCameraEffect();
            if (_cameraEffect != null)
            {
                _cameraEffect.severity = state.Head;
                _cameraEffect.enabled = true;
            }
        }
        else if (_cameraEffect != null)
        {
            _cameraEffect.enabled = false;
        }
    }

    private void EnsureCameraEffect()
    {
        if (_cameraEffect != null) return;

        var cam = Camera.main;
        if (cam == null) return;

        _cameraEffect = cam.gameObject.GetComponent<HeadInjuryCameraEffect>();
        if (_cameraEffect == null)
            _cameraEffect = cam.gameObject.AddComponent<HeadInjuryCameraEffect>();
    }
}

/// <summary>
/// Camera component that applies a real blur effect via OnRenderImage.
/// Disabled automatically in menus.
/// </summary>
public class HeadInjuryCameraEffect : MonoBehaviour
{
    public Severity severity = Severity.Healthy;

    private Material? _blurMaterial;
    private float _pulseTimer;

    private void OnEnable()
    {
        if (_blurMaterial == null)
        {
            var shader = Shader.Find("Hidden/BlitCopy");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("UI/Default");

            if (shader != null)
            {
                _blurMaterial = new Material(shader);
                _blurMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Skip if no injury, no material, or cursor is visible (in menu)
        if (_blurMaterial == null || severity == Severity.Healthy || Cursor.visible)
        {
            Graphics.Blit(source, destination);
            return;
        }

        _pulseTimer += Time.deltaTime;

        int downscale = severity == Severity.Severe ? 4 : 2;
        float pulse = Mathf.Sin(_pulseTimer * 2.5f) * 0.5f;
        downscale += Mathf.RoundToInt(pulse);
        downscale = Mathf.Clamp(downscale, 2, 5);

        int width = source.width / downscale;
        int height = source.height / downscale;

        RenderTexture temp = RenderTexture.GetTemporary(width, height, 0, source.format);
        temp.filterMode = FilterMode.Bilinear;

        Graphics.Blit(source, temp, _blurMaterial);

        if (severity == Severity.Severe)
        {
            RenderTexture temp2 = RenderTexture.GetTemporary(width / 2, height / 2, 0, source.format);
            temp2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(temp, temp2, _blurMaterial);
            Graphics.Blit(temp2, temp, _blurMaterial);
            RenderTexture.ReleaseTemporary(temp2);
        }

        Graphics.Blit(temp, destination, _blurMaterial);
        RenderTexture.ReleaseTemporary(temp);
    }
}
