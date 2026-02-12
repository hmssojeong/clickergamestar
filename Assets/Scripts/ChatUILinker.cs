using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatUILinker : MonoBehaviour
{
    [MenuItem("Tools/Link Chat UI References")]
    public static void LinkReferences()
    {
        Debug.Log("========== LINK START ==========");

        var openAIObj = GameObject.Find("OpenAIAPI");
        if (openAIObj == null) { Debug.LogError("❌ OpenAIAPI 오브젝트 없음!"); return; }

        var script = openAIObj.GetComponent<OpenAIAPITest>();
        if (script == null) { Debug.LogError("❌ OpenAIAPITest 컴포넌트 없음!"); return; }

        var responseTextObj = GameObject.Find("ResponseText");
        var promptFieldObj  = GameObject.Find("PromptField");
        var sendButtonObj   = GameObject.Find("SendButton");

        // ✅ 올바른 경로로 ApiKeyConfig 로드
        var config = AssetDatabase.LoadAssetAtPath<ApiKeyConfig>("Assets/OpenAIAPI/ApiKeyConfig.asset");
        if (config == null)
            config = AssetDatabase.LoadAssetAtPath<ApiKeyConfig>("Assets/OpenAIScripts/ApiKeyConfig.asset");

        var so = new SerializedObject(script);

        if (responseTextObj != null)
        {
            so.FindProperty("_resultTextUI").objectReferenceValue = responseTextObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("✅ _resultTextUI → " + responseTextObj.name);
        }
        else Debug.LogError("❌ ResponseText 오브젝트 없음!");

        if (promptFieldObj != null)
        {
            so.FindProperty("_promptTextField").objectReferenceValue = promptFieldObj.GetComponent<TMP_InputField>();
            Debug.Log("✅ _promptTextField → " + promptFieldObj.name);
        }
        else Debug.LogError("❌ PromptField 오브젝트 없음!");

        if (sendButtonObj != null)
        {
            so.FindProperty("_sendButton").objectReferenceValue = sendButtonObj.GetComponent<Button>();
            Debug.Log("✅ _sendButton → " + sendButtonObj.name);
        }
        else Debug.LogError("❌ SendButton 오브젝트 없음!");

        if (config != null)
        {
            so.FindProperty("_config").objectReferenceValue = config;
            bool hasKey = !string.IsNullOrEmpty(config.OpenAIKey);
            Debug.Log("✅ _config → " + config.name + " | API Key 있음: " + hasKey);
        }
        else Debug.LogError("❌ ApiKeyConfig.asset 없음!");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(script);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("🎉 연결 완료! Ctrl+S로 씬 저장하세요.");
        Debug.Log("========== LINK END ==========");
    }
}
