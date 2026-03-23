using UnityEngine;
using UnityEngine.InputSystem;

public class MobileOnlyVisibility : MonoBehaviour
{
    void Awake()
    {
        if (!ShouldShowMobileUI())
        {
            gameObject.SetActive(false);
        }
    }

    private bool ShouldShowMobileUI()
    {
        if (Application.isMobilePlatform) return true;
        if (SystemInfo.deviceType == DeviceType.Handheld) return true;
        if (Input.touchSupported) return true;
        if (Touchscreen.current != null) return true;
        return false;
    }
}
