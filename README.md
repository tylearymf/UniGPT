# UniGPT	[English Doc](https://github.com/tylearymf/UniGPT/blob/main/README_EN.md)

**在Unity编辑器中使用ChatGPT、NewBing、GoogleBard来生成并执行代码。**

**输出文本支持流式传输**

---

![chatgpt](Screenshots/chatgpt.gif)

![binggpt](Screenshots/binggpt.gif)



### 安装要求

**Unity版本 >= 2019.3**

### 初始化环境

1. 进入Unity工程，等待Python环境初始化完成，然后 Edit -> Project Settings -> Python Scripting -> 

   Launch Terminal (打开命令行工具)

   ![image-20230324020733689](Screenshots/image-20230324020733689.png)

2. 按顺序执行以下命令

   ```bash
   python -m pip install --upgrade pip
   python -m pip install revChatGPT
   python -m pip install EdgeGPT
   python -m pip install GoogleBard
   ```

3. 按照下面的教程配置AI

### 使用教程

#### 配置 ChatGPT

1. 获取 ChatGPT 的 api_key，具体参考：https://platform.openai.com/account/api-keys
2. 编辑该Json配置：Assets/IntegrationGPT/Config~/chat_gpt_config.json
3. 国内的可以参考这里部署个腾讯云函数：[openai-api-proxy](https://github.com/easychen/openai-api-proxy/blob/master/FUNC.md)，然后替换掉配置中的 api_url 即可

```json
{
  # 将获取到的 api_key 替换掉下面的
  "api_key": "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  # chatgpt使用的模型, 如果你是Plus用户, 可以修改模型为 gpt-4
  "model": "gpt-3.5-turbo",
  # 代理设置，如果不需要则删除，在国内的必须要设置，否则无法访问openai的api
  "proxy": "http://127.0.0.1:1080",
  # 如果有国内镜像的，可以将api地址填写在这里
  # 官方API：https://api.openai.com/v1/chat/completions
  "api_url": "",
  # 这个是ChatGPT的提示语，可以根据需要增删
  "prompt": {
    "聊天": "",
	"生成并执行代码": "我想让你在Unity里面实现一个需求, 然后你给我回复代码, 你需要将实现逻辑写在TemplateClass中的Test静态方法里面, 我希望我不需要设置任何东西, 只手动调用Test方法后就能得到正确的结果, 我希望你只回复代码, 而不是其他任何内容, 不要注释.\n我的第一个需求是\n"
  }
}
```

#### 配置 New Bing

1. 首先确保你已经加入了 New Bing，具体参考：https://github.com/acheong08/EdgeGPT#checking-access-required

2. 然后获取 cookies，具体参考：https://github.com/acheong08/EdgeGPT#getting-authentication-required

3. 将获取的 cookies 拷贝到 Assets/IntegrationGPT/Config~/new_bing_cookies.json 中

4. 编辑该Json配置：Assets/IntegrationGPT/Config~/new_bing_config.json

   ```json
   {
     # 对话样式
     # 更具创造力:creative 平衡:balanced 精确:precise
     "style": "precise",
     # 代理设置，如果不需要则删除，在国内的必须要设置，否则无法访问bing的api
     "proxy": "http://127.0.0.1:1080",
     # 如果有国内镜像的，可以将api地址填写在这里
     # 官方API：https://edgeservices.bing.com/edgesvc/turing/conversation/create
     "api_url": "",
     # 这个是Bing的提示语，可以根据需要增删
     "prompts": {
       "聊天": "",
       "生成并执行代码": "我想让你在Unity里面实现一个需求, 然后你给我回复代码, 你需要将实现逻辑写在TemplateClass中的Test静态方法里面, 并正确引用命名空间, 我希望我不需要设置任何东西, 只手动调用Test方法后就能得到正确的结果,我希望你只回复代码, 而不是其他任何内容, 不要注释.\n我的第一个需求是\n"
     }
   }
   ```

#### 配置 Google Bard

1. 首先确保你已经加入了 Google Bard

2. 然后获取session，具体参考：https://github.com/acheong08/Bard#authentication

3. 编辑该Json配置：Assets/IntegrationGPT/Config~/google_bard_config.json

   ```json
   {
       # google bard 的 session
       "session": "",
        # 代理设置，如果不需要则删除，在国内的必须要设置，否则无法访问google的api
       "proxy": "",
       # 这个是Google Bard的提示语，可以根据需要增删
       "prompts":{
           "Chat": ""
       }
   }
   ```

   



### 引用
- [ChatGPT](https://github.com/acheong08/ChatGPT)
- [EdgeGPT](https://github.com/acheong08/EdgeGPT)
- [GoogleBard](https://github.com/acheong08/Bard)
