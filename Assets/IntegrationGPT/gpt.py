import os
import asyncio
import json
import shutil
import enum
import socket

import EdgeGPT as bing
import revChatGPT.V3 as openai_v3

class AIType(enum.IntEnum):
    OpenAI = 0
    Bing = 1
    Bard = 2

gpt_path = os.environ['GPT_PATH']
bing_config_path = gpt_path + "/bing_config.json"
bing_cookie_path = gpt_path + "/bing_cookies.json"
openai_config_path = gpt_path + "/openai_config.json"
last_prompt = ''

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 10086)
client_socket.connect(server_address)

def close_socket():
    client_socket.close()

def send_text(msg):
    client_socket.sendall(msg.encode())

def _common_filter(type, question):
    # 特殊命令
    return False

def set_prompt(prompt):
    global last_prompt
    last_prompt = prompt

async def _ask_openai(question):
    type = AIType.OpenAI
    if _common_filter(type, question):
        return
    
    with open(openai_config_path, encoding="utf8") as f:
        config = json.load(f)

    prompt = last_prompt + question
    api_key = config['api_key']
    model = config['model']

    if 'proxy' in config:
        proxy = config['proxy']
    else:
        proxy = None

    chatbot = openai_v3.ChatbotCLI(api_key=api_key, engine=model, proxy=proxy)
    for query in chatbot.ask_stream(prompt):
        send_text(query)
    close_socket()

def ask_openai(question):
    asyncio.run(_ask_openai(question))

async def _ask_bing(question):
    type = AIType.Bing
    if _common_filter(type, question):
        return
    
    with open(bing_config_path, encoding="utf8") as f:
        config = json.load(f)

    prompt = last_prompt + question
    style = config['style']
    if 'proxy' in config:
        proxy = config['proxy']
    else:
        proxy = None
    
    bot = bing.Chatbot(cookiePath=bing_cookie_path, proxy=proxy)
    wrote = 0
    async for final, response in bot.ask_stream(prompt=prompt, conversation_style=style):
        if not final:
            send_text(response[wrote:])
            wrote = len(response)
    close_socket()
    await bot.close()

def ask_bing(question):
    asyncio.run(_ask_bing(question))

async def _ask_bard(question):
    type = AIType.Bard

    send_text("not implemented")
    close_socket()

def ask_bard(question):
    asyncio.run(_ask_bard(question))