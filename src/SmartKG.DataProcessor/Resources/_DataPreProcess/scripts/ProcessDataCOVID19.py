#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import generateOutputPaths
from ExcelReader import generateSimilarWordMap

if __name__ == "__main__":
    targetPath = "..\\..\\COVID19"

    vJsonPath, eJsonPath, intentPath, entityMapPath = generateOutputPaths(targetPath, "COVID19")

    #excelPath = "..\\excel\\COVID19\\SmartKG_KGDesc_virsus.xlsx"

    excelPath ="..\\excel\\COVID19\\SmartKG_KGDesc_COVID19_zh.xlsx"

    similarWordMap = generateSimilarWordMap("..\\excel\\COVID19\\similarWords_COVID19_zn.tsv")

    convertFile(excelPath, [], ["COVID19"], similarWordMap, vJsonPath, eJsonPath, intentPath, entityMapPath, True)