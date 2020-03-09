#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import checkDir

default_rules = ["PhonicsGrade7\tPOSITIVE\t声|音调|传播", "MechanicsGrade7\tPOSITIVE\t力|运动|向量"]


if __name__ == "__main__":
    targetPath = "..\\..\\Physics"

    checkDir(targetPath)
    checkDir(targetPath + "\\KG\\")
    checkDir(targetPath + "\\NLU\\")

    excelPath = "..\\excel\\Physics\\知识图谱导入模板-初二物理上声学.xlsx"

    vJsonPath = targetPath + "\\KG\\" + "Vertexes_PhonicsGrade7.json"
    eJsonPath = targetPath + "\\KG\\" + "Edges_PhonicsGrade7.json"

    intentPath = targetPath + "\\NLU\\intentrules.tsv"
    entityMapPath = targetPath + "\\NLU\\entitymap.tsv"

    convertFile(excelPath, [], ["PhonicsGrade7"], vJsonPath, eJsonPath, intentPath, entityMapPath, True)

    excelPath = "..\\excel\\Physics\\知识图谱导入模板-初二物理第一+七+八章.xlsx"

    vJsonPath = targetPath + "\\KG\\" + "Vertexes_MechanicsGrade7.json"
    eJsonPath = targetPath + "\\KG\\" + "Edges_MechanicsGrade7.json"

    convertFile(excelPath, [], ["MechanicsGrade7"], vJsonPath, eJsonPath, intentPath, entityMapPath, False)
