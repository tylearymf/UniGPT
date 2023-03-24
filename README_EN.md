*The following content is translated by ChatGPT.*

---

# UniGPT

**Use ChatGPT and BingChat to generate and execute code within the Unity editor.**

------

![chatgpt](Screenshots/chatgpt.gif)

![binggpt](Screenshots/binggpt.gif)

### Installation Requirements

**Unity version >= 2019.3**

### Usage Tutorial

#### Configure ChatGPT

1. Obtain the api_key for ChatGPT, refer to: https://github.com/acheong08/ChatGPT#v3-official-chat-api
2. Edit this JSON configuration: Assets/IntegrationGPT/openai_config.json

```json
{
  # Replace the api_key below with the one you obtained
  "api_key": "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  # The model used by ChatGPT
  "model": "gpt-3.5-turbo",
  # Proxy settings, delete if not needed. Must be set for users in China to access the openai API.
  "proxy": "http://127.0.0.1:1080",
  # If there are domestic mirror sites, you can fill in the api address here
  # Official API: https://api.openai.com/v1/chat/completions
  "api_url": "",
  # This is the prompt for ChatGPT, you can add or delete prompts as needed
  "prompt": {
    "Chat": "",
	"Generate and execute code": "I want you to implement a requirement in Unity and then reply with the code. You need to write the implementation logic in the Test static method of the TemplateClass, and correctly reference the namespace. I hope I don't need to set anything, and I can get the correct results by manually calling the Test method. I hope you only reply with the code, not any other content, and don't add comments.\nMy first requirement is\n"
  }
}
```

#### Configure Bing Chat

1. First, make sure you have joined Bing Chat, refer to: https://github.com/acheong08/EdgeGPT#checking-access-required

2. Then get the cookies, refer to: https://github.com/acheong08/EdgeGPT#getting-authentication-required

3. Copy the obtained cookies to Assets/IntegrationGPT/bing_cookies.json

4. Edit this JSON configuration: Assets/IntegrationGPT/bing_config.json

   ```json
   {
     # Conversation style
     # Creative: creative, Balanced: balanced, Precise: precise
     "style": "precise",
     # Proxy settings, delete if not needed. Must be set for users in China to access the Bing API.
     "proxy": "http://127.0.0.1:1080",
     # If there are domestic mirror sites, you can fill in the api address here
     # Official API: https://edgeservices.bing.com/edgesvc/turing/conversation/create
     "api_url": "",
     # This is the prompt for Bing, you can add or delete prompts as needed
     "prompts": {
       "Chat": "",
       "Generate and execute code": "I want you to implement a requirement in Unity and then reply with the code. You need to write the implementation logic in the Test static method of the TemplateClass, and correctly reference the namespace. I hope I don't need to set anything, and I can get the correct results by manually calling the Test method. I hope you only reply with the code, not any other content, and don't add comments.\nMy first requirement is\n"
     }
   }
   ```

#### Configure Brad

1. To be added in the future

### If it is not working, please manually install the required package

![image-20230324020733689](Screenshots/image-20230324020733689.png)

```bash
Use the following command to manually install the required packages:

python -m pip install EdgeGPT
python -m pip install revChatGPT==3.3.5
```

![image-20230324021009501](Screenshots/image-20230324021009501.png)

![image-20230324021128716](Screenshots/image-20230324021128716.png)

### References

- [ChatGPT](https://github.com/acheong08/ChatGPT)
- [EdgeGPT](https://github.com/acheong08/EdgeGPT)
