using System.Collections.Generic;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatNPC : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultTextUI;
    [SerializeField] private TMP_InputField _promptTextField; // 프롬프트 : AI에 우리 요청사항을 담은 텍스트
    [SerializeField] private Button _sendButton;

    [SerializeField] private ApiKeyConfig _config;

    // 숨기는 방법은 크게
    // - 환경 변수를 이용한 방식
    // - gitignore를 이용한 방식
    // - 깃허브 시크릿 파일을 이용한 방식


    private void Start()
    {
        // 버튼 클릭 이벤트
        _sendButton.onClick.AddListener(Send);
    }

    private async void Send()
    {
        string prompt = _promptTextField.text;
        if (string.IsNullOrEmpty(prompt))
        {
            return;
        }

        // 0. 버튼을 잠근다.
        _sendButton.interactable = false;

        // 1. ChatGPT 사이트에 API_KEY로 로그인한다.
        var api = new OpenAIClient(_config.OpenAIKey);

        // 2. 프롬프트를 작성한다.
        var messages = new List<Message>
        {
            new Message(Role.User, prompt),
        };

        // 3. 모델을 선택하고, 요청을 보낸다. (전송 버튼을 누른다.)
        var chatRequest = new ChatRequest(messages, Model.GPT4oMini);

        // 4. 답변을 비동기로 받는다.
        var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);

        // 5. 답변이 여러개일 수 있으므로 첫번째를 선택한다. (디폴트: 1개) 
        var choice = response.FirstChoice;

        // 결과값을 UI에 출력한다.
        _resultTextUI.text = choice.Message;

        // 초기화
        _promptTextField.text = string.Empty;
        _sendButton.interactable = true;
    }
}