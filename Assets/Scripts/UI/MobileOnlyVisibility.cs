using UnityEngine;

public class MobileOnlyVisibility : MonoBehaviour
{
    void Awake()
    {
        // Se não estiver em plataforma mobile, desativa o objeto.
        if (!Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
    }
}
