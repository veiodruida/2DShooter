using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private Vector3 offsetTotal = Vector3.zero;

    void Awake()
    {
        instance = this;
    }

    public void Shake(float duracao, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duracao, magnitude));
    }

    IEnumerator ShakeRoutine(float duracao, float magnitude)
    {
        float tempoPassado = 0f;

        while (tempoPassado < duracao)
        {
            // Calcula um desvio aleatorio
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Aplica o desvio mantendo o Z da camara intacto
            // Somamos a posicao que a camara ja tem (onde o jogador esta)
            transform.position += new Vector3(x, y, 0);

            tempoPassado += Time.deltaTime;
            yield return null;

            // Removemos o desvio no frame seguinte para nao "acumular" erro de posicao
            transform.position -= new Vector3(x, y, 0);
        }
    }
}
