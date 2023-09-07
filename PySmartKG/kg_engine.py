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
    entity_string = f'[Entity]: {vertex_name}是一个{vertex_type}, 相关信息包括{attributes_string}'

    return entity_string


def relation_type_to_string(relation_type):
    return f'[Relation_type]: {relation_type}'


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
        aliases = kg_data_cache[kg_name]["aliases"]

        for item in aliases:
            if len(item["aliases"]) > 0:
                aliases_set = set(item["aliases"])
                aliases_set.add(item["name"])
            else:
                aliases_set = set([item["name"]])

            for a_item in aliases_set:
                indices = [i for i in range(len(a_item)) if a_item.startswith(query, i)]#query.startswith(a_item, i)]
                for index in indices:
                    result = {
                        'category': item['category'],
                        'name': item['name'],
                        'id': item['id'],
                        'index': index
                    }
                    results.append(result)

        results.sort(key=lambda x: x['index'])

    return results


def is_contained_entity(entity_group, entity_id):
    dict = {entity["vertex_id"]: entity for entity in entity_group}

    if entity_id in dict.keys():
        return True
    else:
        return False


def search_entity(kg_name, entity_id, kg_data_cache):
    if not entity_id:
        return None

    # 查找实体ID对应的实体
    for entity in kg_data_cache[kg_name]['entities']:
        if entity['vertex_id'] == entity_id:
            return entity

    return None


def search_target_entities(kg_name, entity_id, relation_type, kg_data_cache):
    source_entity = None
    target_entities = []

    if (not entity_id) or (not relation_type):
        return None, None

    # 查找实体ID对应的实体
    for entity in kg_data_cache[kg_name]['entities']:
        if entity['vertex_id'] == entity_id:
            source_entity = entity
            break

    if source_entity is None:
        return None, None

    # 查找实体作为源，关系类型为relation_type的目标实体
    for relation in kg_data_cache[kg_name]['relations']:
        if relation['source_vertex_id'] == entity_id and relation['edge_type'] == relation_type:
            target_id = relation['target_vertex_id']
            for entity in kg_data_cache[kg_name]['entities']:
                if entity['vertex_id'] == target_id:
                    target_entities.append(entity)
                    break

    return source_entity, target_entities


def search_source_entities(kg_name, relation_type, entity_id, kg_data_cache):
    source_entities = []
    target_entity = None

    if (not relation_type) or (not entity_id):
        return None, None

    # 查找实体ID对应的实体
    for entity in kg_data_cache[kg_name]['entities']:
        if entity['vertex_id'] == entity_id:
            target_entity = entity
            break

    if target_entity is None:
        return None, None

    # 查找实体作为源，关系类型为relation_type的目标实体
    for relation in kg_data_cache[kg_name]['relations']:
        if relation['target_vertex_id'] == entity_id and relation['edge_type'] == relation_type:
            source_id = relation['source_vertex_id']
            for entity in kg_data_cache[kg_name]['entities']:
                if entity['vertex_id'] == source_id:
                    source_entities.append(entity)
                    break

    return source_entities, target_entity


def process_single_segment(kg_name, matched_segment, kg_data_cache):
    #print("Segment:", matched_segment)
    processing_trace = []
    before_relations = []
    start_entities = []
    index = 0
    while index < len(matched_segment):# item in matched_segment:
        item = matched_segment[index]
        #print("Segment item:", item)
        index += 1
        if item["category"] == "entity":
            entity_id = item["id"]
            #print("entity_id:", entity_id)
            if len(before_relations) > 0:
                relation = before_relations[-1] # got the last before entity relation
                relation_type = relation["name"]
                processing_trace.append(relation_type_to_string(relation_type))
                start_entities = search_source_entities(kg_name, relation_type, entity_id, kg_data_cache)
            else:
                start_entities.append(search_entity(kg_name, entity_id, kg_data_cache))
            break
        else:
            before_relations.append(item)

    r_index = index

    if r_index == len(matched_segment):
        processing_trace.extend([entity_to_string(entity) for entity in start_entities])

    while r_index < len(matched_segment) and len(start_entities) > 0:
        current_entities = []
        for start_entity in start_entities:
            processing_trace.append(entity_to_string(start_entity))
            relation_type = matched_segment[r_index]["name"]
            processing_trace.append(relation_type_to_string(relation_type))
            _, target_entities = search_target_entities(kg_name, start_entity["vertex_id"], relation_type, kg_data_cache)
            if (not (target_entities is None)) and (len(target_entities) > 0):
                current_entities.extend(target_entities)
                if r_index == len(matched_segment) - 1:
                    processing_trace.extend([entity_to_string(entity) for entity in target_entities])

        if len(current_entities) == 0:
            return None, processing_trace
        else:
            start_entities = current_entities

        r_index += 1

    #print("Trace:", processing_trace)

    if len(start_entities) == 0:
        return None, processing_trace
    else:
        return start_entities, processing_trace


def split_list_by_category(matched_items):
    segments = []
    current_segment = []

    target_category_count = 0

    for item in matched_items:
        if item["category"] == "entity":
            target_category_count += 1
            if target_category_count > 0 and target_category_count % 2 == 0:
                segments.append(current_segment)
                current_segment = []
        current_segment.append(item)

    # 添加最后一个分段（如果有的话）
    if current_segment:
        segments.append(current_segment)

    return segments


def process_matched_items(kg_name, matched_items, kg_data_cache):
    segments = split_list_by_category(matched_items)
    entities = None
    tracing = []
    index = 0
    while index < len(segments):
        entities, processing_trace = process_single_segment(kg_name, segments[index], kg_data_cache)
        tracing.extend(processing_trace)
        if entities is None or len(entities) == 0:
            return None, tracing
        elif index < len(segments) - 1:
            next_entity_id = segments[index+1][0]["id"]
            if not is_contained_entity(entities, next_entity_id):
                tracing.append("前面的信息与当下这个实体不匹配——" + entity_to_string(search_entity(kg_name, next_entity_id, kg_data_cache)))
                return None, tracing

        index += 1

    return entities, tracing


def generate_response_message(final_entities, tracing):
    print("Tracing...", tracing)

    resp_message = f"不好意思，无法基于知识库回答您的问题。"
    if (final_entities is not None) and (len(final_entities) > 0):
        resp_text = ""
        for entity in final_entities:
            resp_text += entity_to_string(entity).replace("[Entity]: ", "") + "\n"

        resp_message = f"{resp_text}"
    else:
        if len(tracing) > 0 and "前面的信息与当下这个实体不匹配——" in tracing[-1]:
            resp_message = f"您的问题与知识库中记载情况不相符。"

    return resp_message


def find_subgraph(kg_name, entity_id, kg_data_cache):
    entities = kg_data_cache[kg_name]["entities"]
    relations = kg_data_cache[kg_name]["relations"]

    subgraph_entities = []
    subgraph_relations = []
    entity_id_set = set()

    for entity in entities:
        if entity['vertex_id'] == entity_id:
            subgraph_entities.append(entity)
            entity_id_set.add(entity_id)
            break

    if len(subgraph_entities) == 0:
        return {}

    if entity_id is not None:
        for relation in relations:
            if relation['source_vertex_id'] == entity_id or relation['target_vertex_id'] == entity_id:
                subgraph_relations.append(relation)

                related_entity_id = (
                    relation['source_vertex_id']
                    if relation['target_vertex_id'] == entity_id
                    else relation['target_vertex_id']
                )

                for entity in entities:
                    if entity['vertex_id'] == related_entity_id:
                        if not entity['vertex_id'] in entity_id_set:
                            subgraph_entities.append(entity)
                            entity_id_set.add(entity['vertex_id'])
                        break

    subgroup = {'entities': subgraph_entities, 'relations': subgraph_relations}
    #print(subgroup)
    return subgroup