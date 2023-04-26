import openai


def generate_prompt(query, trace_logs):
    prompt_template_path = "data\\dialog_prompt.txt"

    # 从文件中读取 prompt 模板
    with open(prompt_template_path, 'r') as file:
        prompt_template = file.read()
        # 将 trace_logs 列表转换为一个长字符串
        trace_logs_str = '\n'.join(trace_logs)
        prompt = prompt_template.replace('{related_information}', trace_logs_str).replace('{query_str}', query)
        return prompt

    return query


def gpt_process(prompt):
    with open('data\\openai_key.txt', 'r') as file:
        openai.api_key = file.read().strip()

    openai.api_type = 'azure'
    openai.api_base = "https://avidemo0.openai.azure.com/"
    openai.api_version = "2023-03-15-preview"
    response = openai.ChatCompletion.create(
            engine="gpt40314",
            # replace this value with the deployment name you chose when you deployed the associated model.
            messages=[{"role": "user","content": prompt}],
            temperature=0,
            max_tokens=1000,
            top_p=0.95,
            frequency_penalty=0,
            presence_penalty=0,
            stop=None)

    text = response.choices[0].message.content.strip()

    return text


def get_response_from_llm(query, trace_logs):

    try:
        prompt = generate_prompt(query, trace_logs)
        resp_text = gpt_process(prompt)
        resp_message = f"{resp_text}"
    except Exception as e:
        resp_message = f"在调用大模型的过程中出错"

    return resp_message