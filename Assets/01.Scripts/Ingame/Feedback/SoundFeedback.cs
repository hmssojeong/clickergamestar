using UnityEngine;

public class SoundFeedback : MonoBehaviour, IFeedback
{
    [SerializeField] private AudioSource _audio;
    public void Play()
    {
        _audio.pitch = Random.Range(0.8f, 1.2f);
        _audio.Play();
    }
}
