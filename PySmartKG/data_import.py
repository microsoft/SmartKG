import pandas as pd
import pickle
import os
import random
import shutil

root_path = "data_store"


def random_color():
    return "#{:06x}".format(random.randint(0, 0xFFFFFF))


def read_entities(kg_name, sheet):
    entities = []
    entity_types = set()
    entity_id_set = set()

    for _, row in sheet.iterrows():
        if pd.isna(row[0]) or pd.isna(row[1]) or pd.isna(row[2]):
            print("Warning: The entity id, entity name and entity type must not be none.", row[0], row[1], row[2])
            continue

        vertex_id = row[0]

        if vertex_id in entity_id_set:
            print("Warning: The entity id has existed.", row[0])
            continue
        else:
            entity_id_set.add(vertex_id)

        vertex_name = row[1]
        vertex_type = row[2]
        entity_types.add(vertex_type)
        attributes = []

        for i in range(4, len(row), 2):
            if pd.isna(row[i]):
                break
            elif pd.isna(row[i+1]):
                continue

            attribute_key = row[i]
            attribute_value = row[i + 1]

            if (not attribute_key) or (not attribute_value):
                continue

            attributes.append({"attribute_key": attribute_key, "attribute_value": attribute_value})
        entity = {
            "vertex_id": vertex_id,
            "vertex_name": vertex_name,
            "vertex_type": vertex_type,
            "attributes": attributes
        }

        entities.append(entity)

    type_color_mappings = []
    for entity_type in entity_types:
        type_color_mapping = {
            "kg_name": kg_name,
            "entity_type": entity_type,
            "color": random_color()
        }
        type_color_mappings.append(type_color_mapping)

    return entities, type_color_mappings, entity_id_set


def read_relations(sheet, entity_id_set):
    relations = []
    for _, row in sheet.iterrows():
        if pd.isna(row[0]) or pd.isna(row[1]) or pd.isna(row[2]):
            print("Warning: The relation type, source id, and target id must not be none.", row[0], row[1], row[2])
            continue

        edge_type = row[0]
        source_vertex_id = row[1]
        target_vertex_id = row[2]

        if not(source_vertex_id in entity_id_set):
            print("Warning: The source id (2nd data) is invalid.", row[0], row[1], row[2])
            continue

        if not(target_vertex_id in entity_id_set):
            print("Warning: The target id (3rd data) is invalid.", row[0], row[1], row[2])
            continue

        relation = {
            "edge_type": edge_type,
            "source_vertex_id": source_vertex_id,
            "target_vertex_id": target_vertex_id
        }
        relations.append(relation)
    return relations


def read_aliases(entities, relations):
    aliases = []

    entity_names = set()
    for entity in entities:
        entity_name = entity["vertex_name"]
        if not (entity_name in entity_names):
            aliases.append({"category": "entity", "name": entity_name, "aliases": [], "id": entity["vertex_id"]})
            entity_names.add(entity_name)

    relation_types = set()
    for relation in relations:
        relation_type = relation["edge_type"]
        if not (relation_type in relation_types):
            aliases.append({"category": "relation_type", "name": relation["edge_type"], "aliases": [], "id": "NA"})
            relation_types.add(relation_type)

    return aliases


def load_kg_data(kg_name):
    if os.path.exists(os.path.join(root_path, kg_name)):
        # 加载实体数据
        with open(os.path.join(root_path, kg_name, 'entities.pkl'), 'rb') as f:
            entities = pickle.load(f)

        # 加载关系数据
        with open(os.path.join(root_path, kg_name, 'relations.pkl'), 'rb') as f:
            relations = pickle.load(f)

        with open(os.path.join(root_path, kg_name, 'aliases.pkl'), 'rb') as f:
            aliases = pickle.load(f)

        with open(os.path.join(root_path, kg_name, 'type_color_mappings.pkl'), 'rb') as f:
            type_color_mappings = pickle.load(f)

        # 将数据缓存到字典中
        return {"entities": entities, "relations": relations, "aliases": aliases, "type_color_mappings": type_color_mappings}
    else:
        raise FileNotFoundError(f"Knowledge graph '{kg_name}' not found.")


def save_kg_data(kg_name, entities, relations, type_color_mappings, aliases):
    if not os.path.exists(os.path.join(root_path, kg_name)):
        os.makedirs(os.path.join(root_path, kg_name))
    else:
        return False

    # 将实体和关系数据保存到本地 pkl 文件
    with open(os.path.join(root_path, kg_name, 'entities.pkl'), 'wb') as f:
        pickle.dump(entities, f)

    with open(os.path.join(root_path, kg_name, 'relations.pkl'), 'wb') as f:
        pickle.dump(relations, f)

    save_color_mapping(kg_name, type_color_mappings)
    save_aliases(kg_name, aliases)

    return True


def save_color_mapping(kg_name, type_color_mappings):
    type_color_mappings_path = os.path.join(root_path, kg_name, 'type_color_mappings.pkl')

    with open(type_color_mappings_path, 'wb') as f:
        pickle.dump(type_color_mappings, f)

    return


def save_aliases(kg_name, aliases):
    aliases_file_path = os.path.join(root_path, kg_name, 'aliases.pkl')

    with open(aliases_file_path, 'wb') as f:
        pickle.dump(aliases, f)

    return


def load_kg_names():
    # 使用 glob 查找当前目录下所有文件夹
    folders = [f.name for f in os.scandir(root_path) if f.is_dir()]

    # 过滤出包含 entities.pkl 和 relations.pkl 的文件夹
    kg_names = []
    for folder in folders:
        if (os.path.exists(os.path.join(root_path, folder, 'entities.pkl')) and
                os.path.exists(os.path.join(root_path, folder, 'relations.pkl'))):
            kg_names.append(folder)
    return kg_names


def delete_kg_data(kg_name):
    kg_folder_path = os.path.join(root_path, kg_name)

    if not os.path.exists(kg_folder_path):
        return False

    shutil.rmtree(kg_folder_path)
    return True