using UnityEngine;

public class MobileOnlyVisibility : MonoBehaviour
{
    void Awake()
    {
        if (!RuntimeDeviceProfile.ShouldShowMobileControls())
        {
            gameObject.SetActive(false);
        }
    }
}
