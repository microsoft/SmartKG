#-*- coding: utf-8 -*-

import json
import uuid

from xlrd import open_workbook

defualt_rules = ["PhonicsGrade7\tPOSITIVE\t声|音调|传播", "MechanicsGrade7\tPOSITIVE\t力|运动|向量"]

def genID(rawId, label):
    id = str(uuid.uuid5(uuid.NAMESPACE_DNS, label + rawId))

    return rawId

def convertFile(path, label, cleanNLU):
    vJsonPath = "..\\..\\KG\\" + "Vertexes_" + label + ".json"
    eJsonPath = "..\\..\\KG\\" + "Edges_" + label + ".json"

    intentPath = "..\\..\\NLU\\intentrules.tsv"
    entityMapPath = "..\\..\\NLU\\entitymap.tsv"

    wb = open_workbook(path)
    sheet_vertexes = wb.sheets()[0]
    sheet_edges = wb.sheets()[1]

    edges = []

    nodeNameSet = set()
    propertyNameSet = set()
    relationTypeSet = set()

    headVidSet = set()
    tailVidSet = set()

    for row in range(1, sheet_edges.nrows):
        data = {}
        data["scenarios"] = [label]
        data["relationType"] = sheet_edges.cell_value(row, 1)
        relationTypeSet.add(data["relationType"])

        rawSrcVId = sheet_edges.cell_value(row, 2)
        data["headVertexId"] = genID(rawSrcVId, label)
        headVidSet.add(data["headVertexId"])

        rawDstVID = sheet_edges.cell_value(row, 3)
        data["tailVertexId"] = genID(rawDstVID, label)
        tailVidSet.add(data["tailVertexId"])

        edges.append(data)

    vertexes = []

    for col in range(1, sheet_vertexes.ncols):

        if sheet_vertexes.cell_value(1, col) is not "":
            data = {}
            rawId = sheet_vertexes.cell_value(0, col)
            vid = genID(rawId, label)
            data["id"] = vid
            data["name"] = sheet_vertexes.cell_value(1, col)
            nodeNameSet.add(data["name"])
            data["label"] = "物理概念"
            data["scenarios"] = [label]
            data["leadSentence"] = sheet_vertexes.cell_value(3, col)

            if vid in headVidSet and vid in tailVidSet:
                data["nodeType"] = "MIDDLE"
            elif vid in headVidSet:
                data["nodeType"] = "ROOT"
            elif vid in tailVidSet:
                data["nodeType"] = "LEAF"
            else:
                data["nodeType"] = "SINGLE"

            properties = []

            for row in range(4, sheet_vertexes.nrows - 1, 2):
                property = {}
                if sheet_vertexes.cell_value(row, col) is not "":
                    property["name"] = sheet_vertexes.cell_value(row, col)
                    propertyNameSet.add(property["name"])
                    property["value"] = sheet_vertexes.cell_value(row + 1, col)
                    properties.append(property)

            data["properties"] = properties

            vertexes.append(data)

    print("Generating vertex file:", vJsonPath)
    vfp = open(vJsonPath, 'w', encoding="utf-8")
    json.dump(vertexes, vfp, ensure_ascii=False, sort_keys=True, indent=2, separators=(',', ': '))

    print("Generating edge file:", eJsonPath)
    efp = open(eJsonPath, 'w', encoding="utf-8")
    json.dump(edges, efp, ensure_ascii=False, sort_keys=True, indent=2, separators=(',', ': '))

    entity_lines = ""
    for nodeName in nodeNameSet:
        entity_lines += label + "\t" + nodeName + "\t" + nodeName + "\t" + "NodeName" + "\n"

    for pn in propertyNameSet:
        entity_lines += label + "\t" + pn + "\t" + pn + "\t" + "PropertyName" + "\n"

    for rt in relationTypeSet:
        entity_lines += label + "\t" + rt + "\t" + rt + "\t" + "RelationType" + "\n"

    entity_lines = entity_lines[:-1]

    type = 'a'
    if cleanNLU:
        type = 'w'
    else:
        entity_lines = "\n" + entity_lines

    print("Generating entities in", entityMapPath)
    with open(entityMapPath, type, encoding="utf-8") as ef:
        ef.write(entity_lines)

    print("Generating intent rules in", intentPath)
    nodeNameRule = ""
    for nodeName in nodeNameSet:
        nodeNameRule += nodeName + "|"
    nodeNameRule = nodeNameRule[:-1]

    rule_lines = ""
    if cleanNLU:
        for rule in defualt_rules:
            rule_lines += rule + "\n"
        rule_lines += label + "\tPOSITIVE\t" + nodeNameRule
    else:
        rule_lines += "\n" + label + "\tPOSITIVE\t" + nodeNameRule

    with open(intentPath, type, encoding="utf-8") as rf:
        rf.write(rule_lines)
    return

if __name__ == "__main__":

    srcPath = "..\\excel\\"
    filePath = srcPath + "知识图谱导入模板-初二物理上声学.xlsx"
    convertFile(filePath, "PhonicsGrade7", True)

    filePath = srcPath + "知识图谱导入模板-初二物理第一+七+八章.xlsx"
    convertFile(filePath, "MechanicsGrade7", False)