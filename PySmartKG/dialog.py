def entity_to_string(entity):
    # 提取实体中的 vertex_name、vertex_type 和 attributes
    vertex_name = entity['vertex_name']
    vertex_type = entity['vertex_type']
    attributes = entity['attributes']

    # 将属性格式化为 "attribute_key: attribute_value" 格式
    formatted_attributes = [f'{attribute_dict[keys[0]]}: {attribute_dict[keys[1]]}' for attribute_dict in attributes for keys in zip(list(attribute_dict.keys())[::2], list(attribute_dict.keys())[1::2])]

    # 将格式化后的属性连接为一个字符串
    attributes_string = ', '.join(formatted_attributes)

    # 创建实体的字符串表示
    entity_string = f'{vertex_name}是一个{vertex_type}, 相关信息包括{attributes_string}'

    return entity_string


def relation_to_string(relation):
    # 提取关系中的 source_id、edge_type 和 target_id
    source_id = relation['source_vertex_id']
    edge_type = relation['edge_type']
    target_id = relation['target_vertex_id']

    # 创建关系的字符串表示
    relation_string = f'{source_id} {edge_type} {target_id}'

    return relation_string


def search_kg_data(kg_name, query, kg_data_cache):
    query = query.strip()
    results = []

    if kg_name in kg_data_cache:
        entities = kg_data_cache[kg_name]["entities"]
        relations = kg_data_cache[kg_name]["relations"]

        for entity in entities:
            vertex_name = entity['vertex_name'].strip()

            if vertex_name in query or vertex_name == query:
                results.append({
                    'type': 'entity',
                    'name': entity['vertex_name'],
                    'data': entity_to_string(entity),
                    'index': query.index(vertex_name)
                })

        matched_relations = set()
        for relation in relations:
            edge_type = relation['edge_type']
            if edge_type in matched_relations:
                continue
            if edge_type in query or edge_type == query:
                matched_relations.add(edge_type)
                results.append({
                    'type': 'relation',
                    'name': relation['edge_type'],
                    'data': relation_to_string(relation),
                    'index': query.index(edge_type)
                })

        results.sort(key=lambda x: x['index'])

    return results
