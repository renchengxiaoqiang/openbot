using BotLib.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace Bot.ChromeNs
{
    public class MyOpenAI
    {
        public static ChatClient ChatClient { get; set; }

        private static string systemPrompt;

        private static ConcurrentDictionary<string, List<ChatMessage>> buyerChatMessages;

        static MyOpenAI()
        {
            var apikey = Params.Robot.GetApiKey();
            var baseUrl = Params.Robot.GetBaseUrl();
            var model = Params.Robot.GetModelName();
            buyerChatMessages = new ConcurrentDictionary<string, List<ChatMessage>>();

            // Check if required parameters are not empty
            if (!string.IsNullOrEmpty(apikey) && !string.IsNullOrEmpty(model))
            {
                ChatClient = new ChatClient(model: model, credential: new System.ClientModel.ApiKeyCredential(apikey)
                                , options: string.IsNullOrEmpty(baseUrl) ? null : new OpenAIClientOptions
                                {
                                    Endpoint = new Uri(baseUrl)
                                });
            }
            systemPrompt = Params.Robot.GetSystemPrompt();
        }


        public static string GetAnswer(string seller, string buyer, string question)
        {
            // Check if ChatClient is initialized
            if (ChatClient == null)
            {
                var apikey = Params.Robot.GetApiKey();
                var model = Params.Robot.GetModelName();

                if (string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(model))
                {
                    return "错误：未配置AI参数，请在设置中配置API密钥和模型名称";
                }

                return "错误：AI客户端未正确初始化";
            }

            var key = string.Format("{0}#{1}", seller, buyer);
            var messages = buyerChatMessages.xTryGetValue(key);
            if (messages == null || messages.Count < 1)
            {
                messages = new List<ChatMessage>() {
                    ChatMessage.CreateSystemMessage(systemPrompt),
                    ChatMessage.CreateUserMessage(question),
                };
            }
            else
            {
                messages.Add(ChatMessage.CreateUserMessage(question));
            }
            var completion = ChatClient.CompleteChat(messages);
            var completionContent = completion.GetRawResponse().Content.ToString();
            var answer = JObject.Parse(completionContent)["choices"][0]["message"]["content"].ToString();
            messages.Add(ChatMessage.CreateAssistantMessage(answer));
            buyerChatMessages.AddOrUpdate(key, id => messages, (k, v) => messages);
            return answer;
        }
    }
}