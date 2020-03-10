#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import generateOutputPaths

default_rules = ["PhonicsGrade7\tPOSITIVE\t声|音调|传播", "MechanicsGrade7\tPOSITIVE\t力|运动|向量"]


if __name__ == "__main__":
    targetPath = "..\\..\\Physics"

    vJsonPath, eJsonPath, intentPath, entityMapPath = generateOutputPaths(targetPath, "PhonicsGrade7")
    excelPath = "..\\excel\\Physics\\知识图谱导入模板-初二物理上声学.xlsx"
    convertFile(excelPath, [], ["PhonicsGrade7"], vJsonPath, eJsonPath, intentPath, entityMapPath, True)

    vJsonPath, eJsonPath, intentPath, entityMapPath = generateOutputPaths(targetPath, "MechanicsGrade7")
    excelPath = "..\\excel\\Physics\\知识图谱导入模板-初二物理第一+七+八章.xlsx"
    convertFile(excelPath, [], ["MechanicsGrade7"], vJsonPath, eJsonPath, intentPath, entityMapPath, False)
