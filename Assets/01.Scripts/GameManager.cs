using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int ManualDamage = 10;
    public int AutoDamage = 3;
    public int Gold;

    private void Awake()
    {
        Instance = this;
    }
}