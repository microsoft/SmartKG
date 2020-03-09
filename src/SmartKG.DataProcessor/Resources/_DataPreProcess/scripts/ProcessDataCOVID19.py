#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import checkDir


if __name__ == "__main__":
    targetPath = "..\\..\\COVID19"

    checkDir(targetPath)
    checkDir(targetPath + "\\KG\\")
    checkDir(targetPath + "\\NLU\\")

    excelPath = "..\\excel\\COVID19\\SmartKG_KGDesc_virsus.xlsx"

    vJsonPath = targetPath + "\\KG\\" + "Vertexes_COVID19.json"
    eJsonPath = targetPath + "\\KG\\" + "Edges_COVID19.json"

    intentPath = targetPath + "\\NLU\\intentrules.tsv"
    entityMapPath = targetPath + "\\NLU\\entitymap.tsv"

    convertFile(excelPath, [], ["COVID19"], vJsonPath, eJsonPath, intentPath, entityMapPath, True)