from flask import Flask, request, jsonify, render_template
import pandas as pd
import os
from data_import import read_entities, read_relations, load_kg_data, load_kg_names, \
    read_aliases, save_color_mapping, delete_kg_data, save_kg_data, save_aliases

from kg_engine import search_kg_data, process_matched_items, generate_response_message, find_subgraph
from llm_dialog import get_response_from_llm

app = Flask(__name__)

# 全局缓存字典
kg_data_cache = {}


@app.route('/get_all_kg_names', methods=['GET'])
def get_all_kg_names():
    kg_names = load_kg_names()

    return jsonify(kg_names)


@app.route('/upload', methods=['POST'])
def upload_file():
    excel_file = request.files['file']
    kg_name = request.form.get('kg_name')
    print(excel_file, kg_name)
    if excel_file and kg_name:
        # 读取 Excel 文件的实体信息
        entities_sheet = pd.read_excel(excel_file, sheet_name=0, header=1)
        relations_sheet = pd.read_excel(excel_file, sheet_name=1, header=1)

        entities, type_color_mappings = read_entities(kg_name, entities_sheet)
        relations = read_relations(relations_sheet)
        aliases = read_aliases(entities, relations)

        result = save_kg_data(kg_name, entities, relations, type_color_mappings, aliases)

        if not result:
            return jsonify({"status": "error", "message": "The folder already exists. Please change the kg_name value."})

        return jsonify({"status": "success", "message": "Entities and relations saved successfully."})
    else:
        return jsonify({"status": "error", "message": "No file received."})


@app.route('/get_entities', methods=['GET'])
def get_entities():
    kg_name = request.args.get('kg_name')

    if kg_name:
        # 加载实体数据
        if kg_name not in kg_data_cache:
            try:
                kg_data_cache[kg_name] = load_kg_data(kg_name)
            except FileNotFoundError as e:
                return jsonify({"status": "error", "message": str(e)})

        entities = kg_data_cache[kg_name]["entities"]
        return jsonify(entities)

    else:
        return jsonify({"status": "error", "message": "The kg_name parameter is required."})


@app.route('/get_relations', methods=['GET'])
def get_relations():
    kg_name = request.args.get('kg_name')

    if kg_name:
        # 加载实体数据
        if kg_name not in kg_data_cache:
            try:
                kg_data_cache[kg_name] = load_kg_data(kg_name)
            except FileNotFoundError as e:
                return jsonify({"status": "error", "message": str(e)})

        relations = kg_data_cache[kg_name]["relations"]
        return jsonify(relations)

    else:
        return jsonify({"status": "error", "message": "The kg_name parameter is required."})


@app.route('/get_kg_data', methods=['GET'])
def get_kg_data():
    kg_name = request.args.get('kg_name')

    if kg_name:
        if kg_name not in kg_data_cache:
            try:
                kg_data_cache[kg_name] = load_kg_data(kg_name)
            except FileNotFoundError as e:
                return jsonify({"status": "error", "message": str(e)})

        kg_data = kg_data_cache[kg_name]
        return jsonify(kg_data)
    else:
        return jsonify({"status": "error", "message": "The kg_name parameter is required."})


@app.route('/get_entity', methods=['GET'])
def get_entity():
    kg_name = request.args.get('kg_name')
    entity_name = request.args.get('entity_name')

    if kg_name and entity_name:
        if kg_name not in kg_data_cache:
            try:
                kg_data_cache[kg_name] = load_kg_data(kg_name)
            except FileNotFoundError as e:
                return jsonify({"status": "error", "message": str(e)})

        entities = kg_data_cache[kg_name]["entities"]

        # 查找指定的实体
        for entity in entities:
            if entity['vertex_name'] == entity_name:
                return jsonify(entity)

        # 如果找不到实体，则返回错误消息
        return jsonify({"status": "error", "message": f"Entity with name '{entity_name}' not found."})
    else:
        return jsonify({"status": "error", "message": "Both kg_name and entity_name parameters are required."})


@app.route('/get_type_color_mappings', methods=['GET'])
def get_type_color_mappings():
    kg_name = request.args.get('kg_name', '')
    if not kg_name:
        return jsonify({"message": "Please provide a valid kg_name"}), 400

    if kg_name not in kg_data_cache:
        try:
            kg_data_cache[kg_name] = load_kg_data(kg_name)
        except FileNotFoundError as e:
            return jsonify({"status": "error", "message": str(e)})

    type_color_mappings = kg_data_cache[kg_name]["type_color_mappings"]

    if type_color_mappings is None:
        return jsonify({"message": "type_color_mappings.pkl not found"}), 404

    return jsonify(type_color_mappings), 200


@app.route('/update_type_color_mappings', methods=['PUT'])
def update_type_color_mappings():
    kg_name = request.args.get('kg_name')
    type_color_mappings = request.get_json()

    if not kg_name or not type_color_mappings:
        return jsonify({"message": "Please provide both kg_name and type_color_mappings"}), 400

    save_color_mapping(kg_name, type_color_mappings)

    kg_data_cache[kg_name]["type_color_mappings"] = type_color_mappings

    return jsonify(message='Type-color mappings updated successfully'), 200


@app.route('/delete_kg', methods=['DELETE'])
def delete_kg():
    kg_name = request.args.get('kg_name', '')

    if not kg_name:
        return jsonify({"message": "Please provide a valid kg_name"}), 400

    try:
        result = delete_kg_data(kg_name)
        if not result:
            return jsonify({"message": f"KG folder '{kg_name}' does not exist"}), 404

        return jsonify({"message": f"KG folder '{kg_name}' deleted successfully"}), 200
    except Exception as e:
        return jsonify({"message": f"An error occurred while deleting the KG folder '{kg_name}': {str(e)}"}), 500


@app.route('/dialog', methods=['GET'])
def dialog():
    kg_name = request.args.get('kg_name')
    query = request.args.get('query')
    llm = request.args.get('is_llm_enabled')

    is_llm_integrated = True
    if llm == 'false':
        is_llm_integrated = False
    print("is_llm_integrated", is_llm_integrated)

    if not kg_name or not query:
        return jsonify({'error': 'Invalid parameters'}), 400

    if kg_name not in kg_data_cache:
        try:
            kg_data_cache[kg_name] = load_kg_data(kg_name)
        except FileNotFoundError as e:
            return jsonify({"status": "error", "message": str(e)}), 400

    matched_items = search_kg_data(kg_name, query, kg_data_cache)
    final_entities, tracing = process_matched_items(kg_name, matched_items, kg_data_cache)

    if is_llm_integrated:
        resp_message = get_response_from_llm(query, final_entities, tracing)
    else:
        resp_message = generate_response_message(final_entities, tracing)

    return jsonify({'resp_message': resp_message}), 200


@app.route('/search', methods=['GET'])
def search():
    kg_name = request.args.get('kg_name')
    entity_name = request.args.get('entity_name')

    # 检查 kg_name 和 entity_name 是否为空
    if not kg_name or not entity_name:
        return jsonify({"error": "Both kg_name and entity_name must be provided"}), 400

    # 检查 kg_data_cache 是否已加载，如果没有则加载
    if kg_name not in kg_data_cache:
        try:
            kg_data_cache[kg_name] = load_kg_data(kg_name)
        except FileNotFoundError as e:
            return jsonify({"status": "error", "message": str(e)}), 400

    matched_items = search_kg_data(kg_name, entity_name, kg_data_cache)

    if len(matched_items) > 0 and matched_items[0]["category"] == "entity":
        entity_id = matched_items[0]["id"]
        subgraph = find_subgraph(kg_name, entity_id, kg_data_cache)

    return jsonify(subgraph)


@app.route('/get_aliases', methods=['GET'])
def get_aliases():
    kg_name = request.args.get('kg_name')

    if not kg_name:
        return jsonify({"message": "Please provide a valid kg_name"}), 400

    if kg_name not in kg_data_cache:
        try:
            kg_data_cache[kg_name] = load_kg_data(kg_name)
        except FileNotFoundError as e:
            return jsonify({"status": "error", "message": str(e)})

    aliases = kg_data_cache[kg_name]["aliases"]

    return jsonify(aliases), 200


@app.route('/update_aliases', methods=['PUT'])
def update_aliases():
    kg_name = request.args.get('kg_name')
    aliases = request.get_json()

    if not kg_name or not aliases:
        return jsonify({"message": "Please provide both kg_name and aliases"}), 400

    save_aliases(kg_name, aliases)
    kg_data_cache[kg_name]["aliases"] = aliases

    return jsonify(message='Aliases updated successfully'), 200


@app.route('/check_openai_key', methods=['GET'])
def check_openai_key():
    key_file_path = "data/openai_key.txt"
    key_exists = os.path.isfile(key_file_path)
    return jsonify({"key_exists": key_exists})


@app.route('/')
def index():
    return render_template('index.html')


@app.route('/upload', methods=['GET'])
def upload_page():
    return render_template('upload.html')


if __name__ == '__main__':
    app.run(debug=False)
