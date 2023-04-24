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


def relation_to_string(kg_name, relation, kg_data_cache):
    # 提取关系中的 source_id、edge_type 和 target_id
    source_id = relation['source_vertex_id']
    edge_type = relation['edge_type']
    target_id = relation['target_vertex_id']

    # 从实体列表中查找 source_id 和 target_id 对应的 vertex_name
    entities = kg_data_cache[kg_name]["entities"]
    source_vertex_name = None
    target_vertex_name = None

    for entity in entities:
        if entity['vertex_id'] == source_id:
            source_vertex_name = entity['vertex_name']
        if entity['vertex_id'] == target_id:
            target_vertex_name = entity['vertex_name']
        if source_vertex_name is not None and target_vertex_name is not None:
            break

    # 创建关系的字符串表示
    relation_string = f"{source_vertex_name}的{edge_type}是{target_vertex_name}"

    return relation_string


def search_kg_data(kg_name, query, kg_data_cache):
    query = query.strip()
    results = []

    if kg_name in kg_data_cache:
        entities = kg_data_cache[kg_name]["entities"]
        relations = kg_data_cache[kg_name]["relations"]

        for entity in entities:
            vertex_name = entity['vertex_name'].strip()
            indices = [i for i in range(len(query)) if query.startswith(vertex_name, i)]

            for index in indices:
                results.append({
                    'type': 'entity',
                    'name': entity['vertex_name'],
                    'data': entity_to_string(entity),
                    'index': index
                })

        matched_relations = set()
        for relation in relations:
            edge_type = relation['edge_type']
            if edge_type in matched_relations:
                continue
            indices = [i for i in range(len(query)) if query.startswith(edge_type, i)]
            matched_relations.add(edge_type)

            for index in indices:
                results.append({
                    'type': 'relation',
                    'name': relation['edge_type'],
                    'data': relation_to_string(kg_name, relation, kg_data_cache),
                    'index': index
                })

        results.sort(key=lambda x: x['index'])

    return results
