# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

import json
from xlrd import open_workbook
import os.path
from os import path
import shutil

defaultColorMap = {"red": "#E83344","orange": "#F5A100", "green": "#9DE7B7", "blue": "#009DDC","lightred": "#FD9F7F","bluepurple": "#025CEA","lightpurple": "#78A7FF"}


def getId(rawId, sceanrio):
    newId = str(rawId) + "_" + sceanrio
    return newId


def generateOutputPaths(outputDir, suffix, newlyCreated):
    checkDir(outputDir, newlyCreated)
    checkDir(outputDir + "\\KG\\", newlyCreated)
    checkDir(outputDir + "\\NLU\\", newlyCreated)
    checkDir(outputDir + "\\Visulization\\", newlyCreated)

    vJsonPath = outputDir + "\\KG\\" + "Vertexes_" + suffix + ".json"
    eJsonPath = outputDir + "\\KG\\" + "Edges_" + suffix + ".json"

    intentPath = outputDir + "\\NLU\\intentrules_" + suffix + ".tsv"
    entityMapPath = outputDir + "\\NLU\\entitymap_" + suffix + ".tsv"

    colorJsonPath = outputDir + "\\Visulization\\VisulizationConfig_" + suffix + ".json"

    return vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath

def checkDir(dir, newlyCreated):
    if not path.exists(dir):
        os.mkdir(dir)
        print(dir + " is created to contain KG and NLU files.")
    else:
        if os.path.isdir(dir):
            if newlyCreated:
                print(dir + " exists, clean it to accept newly created files.")
                shutil.rmtree(dir)
                os.mkdir(dir)
            else:
                print(dir + " exists.")
        else:
            os.mkdir(dir)
            print(dir + " is created to contain KG and NLU files.")
    return

def generateSimilarWordMap(sfilePath):
    similarWordMap = {}

    if not sfilePath:
        return similarWordMap

    sfp = open(sfilePath, 'r', encoding="utf-8")
    lines = sfp.readlines()

    for line in lines:
        if not line:
            continue

        line = line.strip()
        tmps = line.split("\t")
        if (len(tmps) != 2):
            print("[Warning] in valid line:", line)
            continue
        stdWord = tmps[0]
        similarWords = tmps[1].split(",")
        similarWordMap[stdWord] = similarWords

    return similarWordMap

def convertFile(excelPath, default_rules, scenarios, similarWordMap, vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath):
    wb = open_workbook(excelPath)
    sheet_vertexes = wb.sheets()[0]
    sheet_edges = wb.sheets()[1]

    edges = []

    nodeNameSet = set()
    propertyNameSet = set()
    relationTypeSet = set()

    headVidSet = set()
    tailVidSet = set()

    scenarioLabelMap = {}
    scenarioReltionMap = {}

    scenario = "_".join(scenarios)

    for row in range(1, sheet_edges.nrows):
        data = {}
        data["scenarios"] = scenarios
        data["relationType"] = sheet_edges.cell_value(row, 0)
        relationTypeSet.add(data["relationType"])

        rawSrcVID = sheet_edges.cell_value(row, 1)
        data["headVertexId"] = getId(rawSrcVID, scenario)
        headVidSet.add(data["headVertexId"])

        rawDstVID = sheet_edges.cell_value(row, 2)
        data["tailVertexId"] = getId(rawDstVID, scenario)
        tailVidSet.add(data["tailVertexId"])

        for scenario in scenarios:
            rSet = set()
            if not (scenario in scenarioReltionMap):
                scenarioReltionMap[scenario] = rSet
            else:
                rSet = scenarioReltionMap[scenario]

            rSet.add(data["relationType"])


        edges.append(data)

    vertexes = []

    for row in range(1, sheet_vertexes.nrows):

        if sheet_vertexes.cell_value(row, 1) is not "":
            data = {}

            data["id"] = getId(sheet_vertexes.cell_value(row, 0),scenario)
            data["name"] = sheet_vertexes.cell_value(row, 1)
            nodeNameSet.add(data["name"])
            data["label"] = sheet_vertexes.cell_value(row, 2)
            data["scenarios"] = scenarios
            data["leadSentence"] = sheet_vertexes.cell_value(row, 3)

            vid = data["id"]

            if vid in headVidSet and vid in tailVidSet:
                data["nodeType"] = "MIDDLE"
            elif vid in headVidSet:
                data["nodeType"] = "ROOT"
            elif vid in tailVidSet:
                data["nodeType"] = "LEAF"
            else:
                data["nodeType"] = "SINGLE"

            for scenario in scenarios:
                vSet = set()
                if not (scenario in scenarioLabelMap):
                    scenarioLabelMap[scenario] = vSet
                else:
                    vSet = scenarioLabelMap[scenario]

                vSet.add(data["label"])

            properties = []

            for col in range(4, sheet_vertexes.ncols - 1, 2):
                property = {}
                if sheet_vertexes.cell_value(row, col) is not "":
                    property["name"] = sheet_vertexes.cell_value(row, col)
                    propertyNameSet.add(property["name"])
                    property["value"] = sheet_vertexes.cell_value(row , col + 1)
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
        if nodeName in similarWordMap:
            similarWords = similarWordMap[nodeName]
        else:
            similarWords = []
        for scenario in scenarios:
            entity_lines += scenario + "\t" + nodeName + "\t" + nodeName + "\t" + "NodeName" + "\n"

            if len(similarWordMap) > 0:
                for sw in similarWords:
                    entity_lines += scenario + "\t" + sw + "\t" + nodeName + "\t" + "NodeName" + "\n"


    for pn in propertyNameSet:
        if pn in similarWordMap:
            similarWords = similarWordMap[pn]
        else:
            similarWords = []
        for scenario in scenarios:
            entity_lines += scenario + "\t" + pn + "\t" + pn + "\t" + "PropertyName" + "\n"

            if len(similarWordMap) > 0:
                for sw in similarWords:
                    entity_lines += scenario + "\t" + sw + "\t" + pn + "\t" + "NodeName" + "\n"

    for rt in relationTypeSet:
        if rt in similarWordMap:
            similarWords = similarWordMap[rt]
        else:
            similarWords = []
        for scenario in scenarios:
            entity_lines += scenario + "\t" + rt + "\t" + rt + "\t" + "RelationType" + "\n"

            if len(similarWordMap) > 0:
                for sw in similarWords:
                    entity_lines += scenario + "\t" + sw + "\t" + rt + "\t" + "NodeName" + "\n"

    entity_lines = entity_lines[:-1]



    print("Generating entities in", entityMapPath)
    with open(entityMapPath, "w", encoding="utf-8") as ef:
        ef.write(entity_lines)

    print("Generating intent rules in", intentPath)
    nodeNameRule = ""
    for nodeName in nodeNameSet:
        nodeNameRule += nodeName + "|"
    nodeNameRule = nodeNameRule[:-1]

    rule_lines = ""

    for rule in default_rules:
        rule_lines += rule + "\n"
    for scenario in scenarios:
        rule_lines += scenario + "\tPOSITIVE\t" + nodeNameRule

    with open(intentPath, "w", encoding="utf-8") as rf:
        rf.write(rule_lines)

    colorObjs = []

    for scenario in scenarios:
        obj = {}
        obj["scenario"] = scenario
        scenarioLabels = scenarioLabelMap[scenario]
        scenarioRelations = scenarioReltionMap[scenario]

        obj["labelsOfVertexes"] = []
        index = 0
        for label in scenarioLabels:
            colors = list(defaultColorMap.values())
            color = colors[-1]
            if index < len(colors) - 1:
                color = colors[index]

            pair = {}
            pair["itemLabel"] = label
            pair["color"] = color

            obj["labelsOfVertexes"].append(pair)
            index += 1

        obj["relationTypesOfEdges"] = []
        index = 0
        for relationType in scenarioRelations:
            colors = list(defaultColorMap.values())
            color = colors[-1]
            if index < len(colors) - 1:
                color = colors[index]

            pair = {}
            pair["itemLabel"] = relationType
            pair["color"] = color

            obj["relationTypesOfEdges"].append(pair)
            index += 1

        colorObjs.append(obj)

    print("Generating Visualization Config file:", colorJsonPath)
    cfp = open(colorJsonPath, 'w', encoding="utf-8")
    json.dump(colorObjs, cfp, ensure_ascii=False, sort_keys=True, indent=2, separators=(',', ': '))

    return
