using System.Collections;
using UnityEngine;

//drives a one-shot frame-by-frame animation on its own SpriteRenderer -
//used for VFX overlays (the Parry flash, the Stun stars) that are spawned
//and parented to a player at runtime rather than baked into the shared
//Player.prefab/Player2.prefab, since both players use the same VFX
public class EffectFlipbook : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float frameDuration;
    private Coroutine playing;

    public void Initialize(Sprite[] frames, float frameRate)
    {
        this.frames = frames;
        frameDuration = 1f / frameRate;

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 5;

        gameObject.SetActive(false);
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        if (playing != null)
        {
            StopCoroutine(playing);
        }

        gameObject.SetActive(true);
        playing = StartCoroutine(PlayFrames());
    }

    private IEnumerator PlayFrames()
    {
        foreach (Sprite frame in frames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameDuration);
        }

        playing = null;
        gameObject.SetActive(false);
    }
}
