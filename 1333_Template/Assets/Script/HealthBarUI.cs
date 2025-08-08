/// <summary>
/// UI component for displaying health above units/buildings.
/// Key Usage: Instantiated for entities with health; updates based on health changes.
/// </summary>
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Fill image for the health bar (assign in Inspector)")]
    public Image foregroundImage;

    [Header("Display offset (change Y for height above target)")]
    public Vector3 offset = new Vector3(0, 2.0f, 0);

    private Transform target;

    /// <summary>
    /// Sets the target to follow, and allows optional override of offset.
    /// </summary>
    public void SetTarget(Transform followTarget, Vector3? customOffset = null)
    {
        target = followTarget;
        if (customOffset.HasValue)
            offset = customOffset.Value;
    }

    /// <summary>
    /// Updates health fill (0-1).
    /// </summary>
    public void SetHealth(float percent)
    {
        if (foregroundImage != null)
            foregroundImage.fillAmount = percent;
    }

    private void LateUpdate()
    {
        // If the target no longer exists, destroy this health bar
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Follow the target position + offset
        transform.position = target.position + offset;

        // Always face the camera
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }
}
