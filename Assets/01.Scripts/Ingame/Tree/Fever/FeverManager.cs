using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class FeverManager : MonoBehaviour
{
    public static FeverManager Instance;

    [Header("Fever Settings")]
    [SerializeField] private int _clicksNeededForFever = 75; // 피버에 필요한 클릭 수
    [SerializeField] private float _feverDuration = 12f; // 피버 지속 시간
    [SerializeField] private float _damageMultiplier = 2f; // 피버 시 데미지 배율

    [Header("Fever Status")]
    public int CurrentClicks = 0; // 현재 클릭 카운트
    public bool IsFeverActive = false; // 피버 활성 여부
    private float _feverTimeRemaining = 0f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject _fallingLeavesObject; // 나뭇잎 파티클 오브젝트
    [SerializeField] private ParticleSystem _feverStartEffect; // 피버 시작 이펙트

    [Header("Events")]
    public UnityEvent OnFeverStart; // 피버 시작 이벤트
    public UnityEvent OnFeverEnd; // 피버 종료 이벤트
    public UnityEvent<int, int> OnClickCountChanged; // 클릭 수 변경 (현재, 최대)
    public UnityEvent<float> OnFeverTimeChanged; // 피버 시간 변경

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_fallingLeavesObject != null)
        {
            _fallingLeavesObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsFeverActive)
        {
            _feverTimeRemaining -= Time.deltaTime;
            OnFeverTimeChanged?.Invoke(_feverTimeRemaining);

            if (_feverTimeRemaining <= 0)
            {
                EndFever();
            }
        }
    }

    // 클릭 시 게이지 증가
    public void AddClick()
    {
        if (IsFeverActive) return; // 피버 중에는 게이지 안 참

        CurrentClicks++;
        OnClickCountChanged?.Invoke(CurrentClicks, _clicksNeededForFever);

        // 게이지 꽉 차면 피버 시작
        if (CurrentClicks >= _clicksNeededForFever)
        {
            StartFever();
        }
    }

    // 피버타임 시작
    private void StartFever()
    {
        IsFeverActive = true;
        _feverTimeRemaining = _feverDuration;
        CurrentClicks = 0; // 게이지 리셋

        // FallingLeaves 활성화
        if (_fallingLeavesObject != null)
        {
            _fallingLeavesObject.SetActive(true);
        }

        // 피버 시작 이펙트
        if (_feverStartEffect != null)
        {
            _feverStartEffect.Play();
        }

        // 이벤트 호출
        OnFeverStart?.Invoke();

        Debug.Log("FEVER TIME START!");
    }

    // 피버타임 종료
    private void EndFever()
    {
        IsFeverActive = false;
        _feverTimeRemaining = 0f;

        // FallingLeaves 비활성화
        if (_fallingLeavesObject != null)
        {
            _fallingLeavesObject.SetActive(false);
        }

        // 이벤트 호출
        OnFeverEnd?.Invoke();

        Debug.Log("Fever Time End");
    }

    // 피버 시 데미지 배율 반환
    public double GetDamageMultiplier()
    {
        return IsFeverActive ? _damageMultiplier : 1.0;
    }

    //현재 게이지 진행률 (0~1)
    public float GetFeverProgress()
    {
        return (float)CurrentClicks / _clicksNeededForFever;
    }

    // 피버 남은 시간 진행률 (0~1)
    public float GetFeverTimeProgress()
    {
        return _feverTimeRemaining / _feverDuration;
    }

    public int GetClicksNeeded()
    {
        return _clicksNeededForFever;
    }
}