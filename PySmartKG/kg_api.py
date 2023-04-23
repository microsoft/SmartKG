from flask import Flask, request, jsonify, render_template
import pandas as pd
import pickle
import os
from data_import import read_entities, read_relations
import shutil
from dialog import search_kg_data

app = Flask(__name__)

# 全局缓存字典
kg_data_cache = {}


def load_kg_data(kg_name):
    if os.path.exists(kg_name):
        # 加载实体数据
        with open(os.path.join(kg_name, 'entities.pkl'), 'rb') as f:
            entities = pickle.load(f)

        # 加载关系数据
        with open(os.path.join(kg_name, 'relations.pkl'), 'rb') as f:
            relations = pickle.load(f)

        # 将数据缓存到字典中
        kg_data_cache[kg_name] = {"entities": entities, "relations": relations}
    else:
        raise FileNotFoundError(f"Knowledge graph '{kg_name}' not found.")


@app.route('/get_all_kg_names', methods=['GET'])
def get_all_kg_names():
    # 使用 glob 查找当前目录下所有文件夹
    folders = [f.name for f in os.scandir('.') if f.is_dir()]

    # 过滤出包含 entities.pkl 和 relations.pkl 的文件夹
    kg_names = []
    for folder in folders:
        if (os.path.exists(os.path.join(folder, 'entities.pkl')) and
                os.path.exists(os.path.join(folder, 'relations.pkl'))):
            kg_names.append(folder)

    return jsonify(kg_names)


@app.route('/upload', methods=['POST'])
def upload_file():
    excel_file = request.files['file']
    kg_name = request.form.get('kg_name')
    print(excel_file, kg_name)
    if excel_file and kg_name:
        # 检查文件夹是否存在，如果不存在则创建
        if not os.path.exists(kg_name):
            os.makedirs(kg_name)
        else:
            return jsonify({"status": "error", "message": "The folder already exists. Please change the kg_name value."})

        # 读取 Excel 文件的实体信息
        entities_sheet = pd.read_excel(excel_file, sheet_name=0, header=1)
        relations_sheet = pd.read_excel(excel_file, sheet_name=1, header=1)

        entities, type_color_mappings = read_entities(kg_name, entities_sheet)
        relations = read_relations(relations_sheet)

        # 将实体和关系数据保存到本地 pkl 文件
        with open(os.path.join(kg_name, 'entities.pkl'), 'wb') as f:
            pickle.dump(entities, f)

        with open(os.path.join(kg_name, 'relations.pkl'), 'wb') as f:
            pickle.dump(relations, f)

        # 保存 type_color_mappings 为 pkl 文件
        with open(os.path.join(kg_name, 'type_color_mappings.pkl'), 'wb') as f:
            pickle.dump(type_color_mappings, f)

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
                load_kg_data(kg_name)
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
                load_kg_data(kg_name)
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
                load_kg_data(kg_name)
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
                load_kg_data(kg_name)
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

    type_color_mappings_path = os.path.join(kg_name, 'type_color_mappings.pkl')

    if not os.path.exists(type_color_mappings_path):
        return jsonify({"message": "type_color_mappings.pkl not found"}), 404

    with open(type_color_mappings_path, 'rb') as f:
        type_color_mappings = pickle.load(f)

    return jsonify(type_color_mappings), 200


@app.route('/update_type_color_mappings', methods=['PUT'])
def update_type_color_mappings():
    kg_name = request.args.get('kg_name')
    type_color_mappings = request.get_json()

    if not kg_name or not type_color_mappings:
        return jsonify({"message": "Please provide both kg_name and type_color_mappings"}), 400

    type_color_mappings_path = os.path.join(kg_name, 'type_color_mappings.pkl')

    if not os.path.exists(type_color_mappings_path):
        return jsonify({"message": "type_color_mappings.pkl not found"}), 404

    with open(type_color_mappings_path, 'wb') as f:
        pickle.dump(type_color_mappings, f)

    return jsonify(message='Type-color mappings updated successfully'), 200


@app.route('/delete_kg', methods=['DELETE'])
def delete_kg():
    kg_name = request.args.get('kg_name', '')
    print(kg_name)
    if not kg_name:
        return jsonify({"message": "Please provide a valid kg_name"}), 400

    kg_folder_path = kg_name

    if not os.path.exists(kg_folder_path):
        return jsonify({"message": f"KG folder '{kg_name}' does not exist"}), 404

    try:
        shutil.rmtree(kg_folder_path)
        return jsonify({"message": f"KG folder '{kg_name}' deleted successfully"}), 200
    except Exception as e:
        return jsonify({"message": f"An error occurred while deleting the KG folder '{kg_name}': {str(e)}"}), 500


@app.route('/dialog', methods=['GET'])
def dialog():
    kg_name = request.args.get('kg_name')
    query = request.args.get('query')

    if not kg_name or not query:
        return jsonify({'error': 'Invalid parameters'}), 400

    if kg_name not in kg_data_cache:
        try:
            load_kg_data(kg_name)
        except FileNotFoundError as e:
            return jsonify({"status": "error", "message": str(e)}), 400

    matched_items = search_kg_data(kg_name, query, kg_data_cache)
    print("matched_items", matched_items)

    if len(matched_items) > 0:
        resp_message = f"KG: {kg_name}. Matched some items"
    else:
        resp_message = f"KG: {kg_name}. Matched 0 items"

    return jsonify({'resp_message': resp_message}), 200


@app.route('/')
def index():
    return render_template('index.html')


@app.route('/upload', methods=['GET'])
def upload_page():
    return render_template('upload.html')


if __name__ == '__main__':
    app.run(debug=False)
