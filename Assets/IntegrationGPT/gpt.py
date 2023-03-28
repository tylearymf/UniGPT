import os
import sys
import asyncio
import json
import enum
import socket

import EdgeGPT as bing
import revChatGPT.V3 as chatgpt
import Bard as googleBard


class AIType(enum.IntEnum):
    ChatGPT = 0
    Bing = 1
    Bard = 2


gpt_path = os.environ["GPT_PATH"]
config_prompt = ""
config_path = None
config_data = None

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ("localhost", 10086)
client_socket.connect(server_address)


def close_socket():
    client_socket.close()


def send_text(msg):
    client_socket.sendall(msg.encode())


def send_exception(e: Exception):
    exc_type, exc_value, exc_traceback = sys.exc_info()
    fname = os.path.split(exc_traceback.tb_frame.f_code.co_filename)[1]
    send_text(f"An error occurred: {repr(e)}.\n:::{fname}:{exc_traceback.tb_lineno}")


def _common_filter(type, question):
    # 特殊命令
    return False


def set_prompt(prompt):
    global config_prompt
    config_prompt = prompt


def set_config(path):
    global config_path
    global config_data

    config_path = path
    with open(path, encoding="utf8") as f:
        config_data = json.load(f)


async def _ask_chat_gpt(question):
    try:
        type = AIType.ChatGPT
        if _common_filter(type, question):
            return

        prompt = config_prompt + question
        api_key = config_data["api_key"]
        model = config_data["model"]

        if "proxy" in config_data:
            proxy = config_data["proxy"]
        else:
            proxy = None

        chatbot = chatgpt.ChatbotCLI(api_key=api_key, engine=model, proxy=proxy)
        for query in chatbot.ask_stream(prompt):
            send_text(query)
    except Exception as e:
        send_exception(e)
    finally:
        close_socket()


def ask_chat_gpt(question):
    asyncio.run(_ask_chat_gpt(question))


async def _ask_bing(question):
    try:
        type = AIType.Bing
        if _common_filter(type, question):
            return

        prompt = config_prompt + question
        style = config_data["style"]
        if "proxy" in config_data:
            proxy = config_data["proxy"]
        else:
            proxy = None

        dir_path, filename = os.path.split(config_path)
        cookie_path = os.path.join(dir_path, config_data["cookie_path"])
        chatbot = bing.Chatbot(cookiePath=cookie_path, proxy=proxy)

        wrote = 0
        async for final, response in chatbot.ask_stream(
            prompt=prompt, conversation_style=style
        ):
            if not final:
                send_text(response[wrote:])
                wrote = len(response)
        await chatbot.close()
    except Exception as e:
        send_exception(e)
    finally:
        close_socket()


def ask_bing(question):
    asyncio.run(_ask_bing(question))


async def _ask_bard(question):
    try:
        type = AIType.Bard
        if _common_filter(type, question):
            return

        prompt = config_prompt + question
        session = config_data["session"]
        if "proxy" in config_data:
            proxy = config_data["proxy"]
        else:
            proxy = None

        chatbot = googleBard.Chatbot(session) # proxy=proxy
        send_text(chatbot.ask(prompt)["content"])
    except Exception as e:
        send_exception(e)
    finally:
        close_socket()


def ask_bard(question):
    asyncio.run(_ask_bard(question))
