#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import generateOutputPaths


if __name__ == "__main__":
    targetPath = "..\\..\\COVID19"

    vJsonPath, eJsonPath, intentPath, entityMapPath = generateOutputPaths(targetPath, "COVID19")

    excelPath = "..\\excel\\COVID19\\SmartKG_KGDesc_virsus.xlsx"

    convertFile(excelPath, [], ["COVID19"], vJsonPath, eJsonPath, intentPath, entityMapPath, True)