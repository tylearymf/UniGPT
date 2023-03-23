import os
import sys
import asyncio
import json
from enum import IntEnum

import UnityEditor as editor
import EdgeGPT as bing
import revChatGPT.V3 as openai_v3

class AIType(IntEnum):
    OpenAI = 0
    Bing = 1
    Bard = 2

gpt_path = sys.path[1]
bing_config_path = gpt_path + "/bing_config.json"
bing_cookie_path = gpt_path + "/bing_cookies.json"
openai_config_path = gpt_path + "/openai_config.json"

def recv(type : AIType, question, msg):
    editor.GPT.GPTWindow.recv(int(type), question, msg)

def _common_filter(type, question):
    # 特殊命令
    return False

async def _ask_openai(question):
    if _common_filter(AIType.OpenAI, question):
        return
    
    with open(openai_config_path, encoding="utf8") as f:
        config = json.load(f)

    prompt = config['prompt'] + question
    api_key = config['api_key']
    model = config['model']

    if 'proxy' in config:
        proxy = config['proxy']
    else:
        proxy = None

    chatbot = openai_v3.ChatbotCLI(api_key=api_key,
                                   engine=model,
                                   proxy=proxy)
    recv(AIType.OpenAI, question, chatbot.ask(prompt, "user"))

def ask_openai(question):
    asyncio.run(_ask_openai(question))

async def _ask_bing(question):
    if _common_filter(AIType.Bing, question):
        return
    
    with open(bing_config_path, encoding="utf8") as f:
        config = json.load(f)

    prompt = config['prompt'] + question
    style = config['style']
    if 'proxy' in config:
        proxy = config['proxy']
    else:
        proxy = None
        
    bot = bing.Chatbot(cookiePath=bing_cookie_path,
                       proxy=proxy)
    recv(AIType.Bing, question, (await bot.ask(prompt=prompt, conversation_style=style))["item"]["messages"][1]["adaptiveCards"][0]["body"][0]["text"])
    await bot.close()

def ask_bing(question):
    asyncio.run(_ask_bing(question))

async def _ask_bard(question):
    recv(AIType.Bard, question, "not implemented")

def ask_bard(question):
    asyncio.run(_ask_bard(question))
