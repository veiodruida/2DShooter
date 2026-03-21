using UnityEngine;

public class KeepUpright : MonoBehaviour
{
    // Guardamos a rotacao inicial (que deve ser "para cima")
    private Quaternion rotationFixa;

    void Start()
    {
        // Define a rotacao como "zero" no mundo (apontando para cima)
        rotationFixa = Quaternion.Euler(0, 0, 0);
    }

    // Usamos LateUpdate para garantir que rodamos DEPOIS da nave girar
    void LateUpdate()
    {
        // Forca o fogo a manter sempre a mesma rotacao global
        transform.rotation = rotationFixa;
    }
}
