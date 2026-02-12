using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatDiagnostic : MonoBehaviour
{
    [MenuItem("Tools/Diagnose Chat UI")]
    public static void Diagnose()
    {
        Debug.Log("========== CHAT UI DIAGNOSTIC ==========");

        // 1. OpenAIAPI 오브젝트
        var openAIObj = GameObject.Find("OpenAIAPI");
        if (openAIObj == null) { Debug.LogError("❌ OpenAIAPI 오브젝트 없음!"); return; }
        Debug.Log("✅ OpenAIAPI 오브젝트 발견: " + openAIObj.name);

        // 2. OpenAIAPITest 컴포넌트
        var script = openAIObj.GetComponent<OpenAIAPITest>();
        if (script == null) { Debug.LogError("❌ OpenAIAPITest 컴포넌트 없음!"); return; }
        Debug.Log("✅ OpenAIAPITest 컴포넌트 존재");

        // 3. SerializedField 연결 상태 확인
        var so = new SerializedObject(script);

        var resultProp   = so.FindProperty("_resultTextUI");
        var promptProp   = so.FindProperty("_promptTextField");
        var buttonProp   = so.FindProperty("_sendButton");
        var configProp   = so.FindProperty("_config");

        Debug.Log(resultProp.objectReferenceValue != null
            ? "✅ _resultTextUI 연결됨: " + resultProp.objectReferenceValue.name
            : "❌ _resultTextUI 연결 안됨 (NULL)");

        Debug.Log(promptProp.objectReferenceValue != null
            ? "✅ _promptTextField 연결됨: " + promptProp.objectReferenceValue.name
            : "❌ _promptTextField 연결 안됨 (NULL)");

        Debug.Log(buttonProp.objectReferenceValue != null
            ? "✅ _sendButton 연결됨: " + buttonProp.objectReferenceValue.name
            : "❌ _sendButton 연결 안됨 (NULL)");

        Debug.Log(configProp.objectReferenceValue != null
            ? "✅ _config 연결됨: " + configProp.objectReferenceValue.name
            : "❌ _config 연결 안됨 (NULL)");

        // 4. ApiKeyConfig 값 확인
        if (configProp.objectReferenceValue != null)
        {
            var config = configProp.objectReferenceValue as ApiKeyConfig;
            bool hasKey = !string.IsNullOrEmpty(config.OpenAIKey);
            Debug.Log(hasKey
                ? "✅ API Key 존재: " + config.OpenAIKey.Substring(0, 10) + "..."
                : "❌ API Key 비어있음!");
        }

        // 5. 연결이 끊겼으면 자동 재연결
        bool needsRelink = resultProp.objectReferenceValue == null ||
                           promptProp.objectReferenceValue == null ||
                           buttonProp.objectReferenceValue == null ||
                           configProp.objectReferenceValue == null;

        if (needsRelink)
        {
            Debug.LogWarning("⚠️ 연결 끊김 감지 → 자동 재연결 시작...");

            var responseTextObj = GameObject.Find("ResponseText");
            var promptFieldObj  = GameObject.Find("PromptField");
            var sendButtonObj   = GameObject.Find("SendButton");
            var config = AssetDatabase.LoadAssetAtPath<ApiKeyConfig>("Assets/OpenAIScripts/ApiKeyConfig.asset");

            if (responseTextObj) resultProp.objectReferenceValue = responseTextObj.GetComponent<TextMeshProUGUI>();
            if (promptFieldObj)  promptProp.objectReferenceValue  = promptFieldObj.GetComponent<TMP_InputField>();
            if (sendButtonObj)   buttonProp.objectReferenceValue  = sendButtonObj.GetComponent<Button>();
            if (config)          configProp.objectReferenceValue  = config;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(script);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("🔧 재연결 완료! 씬 저장 필요");
        }
        else
        {
            Debug.Log("✅ 모든 참조 정상 연결됨!");
        }

        // 6. SendButton onClick 리스너 수 (런타임에서만 의미있음)
        var btn = sendButtonObj_check();
        if (btn != null)
            Debug.Log("📌 SendButton onClick 퍼시스턴트 리스너 수: " + btn.onClick.GetPersistentEventCount());

        Debug.Log("========================================");
    }

    static Button sendButtonObj_check()
    {
        var obj = GameObject.Find("SendButton");
        return obj ? obj.GetComponent<Button>() : null;
    }
}
