using UnityEngine;

public class KeepUpright : MonoBehaviour
{
    // Guardamos a rotação inicial (que deve ser "para cima")
    private Quaternion rotationFixa;

    void Start()
    {
        // Define a rotação como "zero" no mundo (apontando para cima)
        rotationFixa = Quaternion.Euler(0, 0, 0);
    }

    // Usamos LateUpdate para garantir que rodamos DEPOIS da nave girar
    void LateUpdate()
    {
        // Força o fogo a manter sempre a mesma rotação global
        transform.rotation = rotationFixa;
    }
}