import pandas as pd
import pickle

import random


def random_color():
    return "#{:06x}".format(random.randint(0, 0xFFFFFF))


def read_entities(kg_name, sheet):
    entities = []
    entity_types = set()

    for _, row in sheet.iterrows():
        vertex_id = row[0]
        vertex_name = row[1]
        vertex_type = row[2]
        entity_types.add(vertex_type)
        attributes = []
        for i in range(4, len(row), 2):
            if pd.isna(row[i]):
                break
            attribute_key = row[i]
            attribute_value = row[i + 1]
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

    return entities, type_color_mappings

def read_relations(sheet):
    relations = []
    for _, row in sheet.iterrows():
        edge_type = row[0]
        source_vertex_id = row[1]
        target_vertex_id = row[2]
        relation = {
            "edge_type": edge_type,
            "source_vertex_id": source_vertex_id,
            "target_vertex_id": target_vertex_id
        }
        relations.append(relation)
    return relations


def main():
    file_path = 'data\\SmartKG_Xiyouji_org.xlsx'
    excel_data = pd.read_excel(file_path, sheet_name=None, engine='openpyxl')

    #entities_sheet = excel_data[list(excel_data.keys())[0]]
    entities_sheet = pd.read_excel(excel_data, sheet_name=0, header=1)

    #relations_sheet = excel_data[list(excel_data.keys())[1]]
    relations_sheet = pd.read_excel(excel_data, sheet_name=1, header=1)

    entities = read_entities(entities_sheet)
    relations = read_relations(relations_sheet)

    with open('entities.pkl', 'wb') as f:
        pickle.dump(entities, f)

    with open('relations.pkl', 'wb') as f:
        pickle.dump(relations, f)


if __name__ == "__main__":
    main()
