using UnityEngine;

public class SoundFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private AudioSource _audio;

    public void Play(ClickInfo clickInfo)
    {
        if (clickInfo.Type == EClickType.Auto) return;

        _audio.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        _audio.Play();
    }
}