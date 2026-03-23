using System.Runtime.InteropServices;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class RuntimeDeviceProfile
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int Furia_IsMobileBrowser();
#endif

    public static bool ShouldShowMobileControls()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return Furia_IsMobileBrowser() == 1;
#elif UNITY_EDITOR
#if ENABLE_INPUT_SYSTEM
        return Touchscreen.current != null;
#else
        return false;
#endif
#else
        return false;
#endif
    }
}
