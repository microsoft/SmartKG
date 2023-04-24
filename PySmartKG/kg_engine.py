def find_subgraph(kg_name, entity_name, kg_data_cache):
    entities = kg_data_cache[kg_name]["entities"]
    relations = kg_data_cache[kg_name]["relations"]

    entity_id = None
    subgraph_entities = []
    subgraph_relations = []

    for entity in entities:
        if entity['vertex_name'] == entity_name:
            entity_id = entity['vertex_id']
            subgraph_entities.append(entity)
            break

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
                        subgraph_entities.append(entity)
                        break

    return {'entities': subgraph_entities, 'relations': subgraph_relations}
