using System.Collections.Generic;
using UnityEngine;

// 효과음 종류를 정의하는 Enum
public enum ESfx
{
    AutoClickerAttack,
    Click,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("SFX Settings")]
    [SerializeField] private AudioClip[] _playerSfxs;

    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 0.8f;

    [Header("AudioSource Pool Settings")]
    [SerializeField] private int _sfxPoolSize = 10;

    private List<AudioSource> _sfxSourcePool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
    }

    private void InitializeAudioSources()
    {
        _sfxSourcePool.Clear();

        for (int i = 0; i < _sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFX_Source_{i}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            _sfxSourcePool.Add(source);
        }
    }

    // 효과음을 재생
    public void PlaySFX(ESfx sfxType, float volumeMultiplier = 1f)
    {
        int index = (int)sfxType; // Enum을 정수 인덱스로 변환

        if (index < 0 || index >= _playerSfxs.Length)
        {
            return;
        }

        AudioClip clip = _playerSfxs[index];
        if (clip == null) return;

        AudioSource availableSource = GetAvailableSource();
        if (availableSource != null)
        {
            // 자연스러운 사운드를 위해 피치를 살짝 랜덤하게 조절
            availableSource.pitch = Random.Range(0.95f, 1.05f);
            availableSource.PlayOneShot(clip, _sfxVolume * volumeMultiplier);
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in _sfxSourcePool)
        {
            if (!source.isPlaying) return source;
        }
        // 모든 소스가 사용 중이면 첫 번째 소스 재사용
        return _sfxSourcePool[0];
    }
}