using UnityEngine;

[CreateAssetMenu(fileName = "ApiKeyConfig", menuName = "Scriptable Objects/ApiKeyConfig")]
public class ApiKeyConfig : ScriptableObject
{
    [SerializeField] private string _openAIKey;
    public string OpenAIKey => _openAIKey;
}
