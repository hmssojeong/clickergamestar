// ApiKeyConfig.cs — ScriptableObject로 키 분리
using UnityEngine;

[CreateAssetMenu(fileName = "ApiKeyConfig", menuName = "Config/API Key")]
public class ApiKeyConfig : ScriptableObject
{
    [SerializeField] private string _openAIKey;
    public string OpenAIKey => _openAIKey;
}