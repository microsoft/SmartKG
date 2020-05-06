# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.
#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import generateOutputPaths

default_rules = []


if __name__ == "__main__":
    targetPath = "..\\..\\Physics2"

    scenario = "MotionAndPower"

    vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath = generateOutputPaths(targetPath, scenario, True)
    excelPath = "..\\excel\\Physics2\\SmartKG_KGDesc_MotionAndPower_zh.xlsx"
    convertFile(excelPath, [], [scenario], [], vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath)

    scenario = "Phonics"

    vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath = generateOutputPaths(targetPath, scenario, False)
    excelPath = "..\\excel\\Physics\\SmartKG_KGDesc_PhonicsGrade7_zh.xlsx"
    convertFile(excelPath, [], [scenario], [], vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath)