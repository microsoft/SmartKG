# 项目名称

本项目演示了如何将大型语言模型（LLM）集成到知识图谱查看器中。LLM负责根据知识图谱提供的上下文生成对用户查询的响应。

## 开始使用

1. 克隆此代码库。
2. 安装Python

Python的版本需要 >= 3.8, 建议 3.10

3. 设置虚拟环境（可选但推荐）并安装所需的软件包：

python -m venv venv
source venv/bin/activate # 在Windows上使用 venv\Scripts\activate
pip install -r requirements.txt


4. 运行应用程序：

python kg_api.py


5. 打开浏览器并导航至 `http://localhost:5000` 以使用该应用程序。

## LLM 集成

要启用 LLM 集成，您需要提供自己的 OpenAI API 密钥。API 密钥应存储在名为 `openai_key.txt` 的文本文件中，该文件位于 `data` 目录下。

**注意**：如果 `data` 目录下没有 `openai_key.txt` 文件，即使在应用程序中启用 LLM 集成，它也将被禁用。

要获取 API 密钥，请在 OpenAI 或 Microsoft Azure 上注册帐户。

### 示例目录结构

your-project/
├── data/
│ ├── openai_key.txt
├── app.py
├── templates/
└── README.md


## 许可

本项目采用 [MIT 许可证](LICENSE) 开源。

## 贡献

欢迎提供贡献！请随时提交问题或拉取请求以改进此项目。