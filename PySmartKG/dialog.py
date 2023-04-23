

def search_kg_data(kg_name, query, kg_data_cache):
    query = query.strip()
    results = []

    if kg_name in kg_data_cache:
        entities = kg_data_cache[kg_name]["entities"]
        relations = kg_data_cache[kg_name]["relations"]

        for entity in entities:
            vertex_name = entity['vertex_name'].strip()
            #print(vertex_name, query)
            if vertex_name in query or vertex_name == query:
                results.append({
                    'type': 'entity',
                    'data': entity,
                    'index': query.index(vertex_name)
                })

        for relation in relations:
            edge_type = relation['edge_type']
            if edge_type in query or edge_type == query:
                results.append({
                    'type': 'relation',
                    'data': relation,
                    'index': query.index(edge_type)
                })

        #if len(results) > 0:
            # 按在 query 中出现的顺序对结果进行排序
        results.sort(key=lambda x: x['index'])

    return results
