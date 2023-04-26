import openai


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

    #print(text)
    return text