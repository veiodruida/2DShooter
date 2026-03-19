using UnityEngine;

public static class Audio2DManager
{
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Criamos um objeto vazio apenas para tocar o som
        GameObject go = new GameObject("TempAudio2D_" + clip.name);
        AudioSource source = go.AddComponent<AudioSource>();

        // CONFIGURAÇÃO CRUCIAL PARA SOM NÃO-ESPACIAL:
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f; // 100% 2D
        source.playOnAwake = false;
        source.ignoreListenerPause = true; // Opcional: toca mesmo se o jogo pausar

        source.Play();

        // Destrói o objeto assim que o som acabar
        Object.Destroy(go, clip.length);
    }
}
