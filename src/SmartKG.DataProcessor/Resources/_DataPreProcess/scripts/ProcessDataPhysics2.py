# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.
#-*- coding: utf-8 -*-

from ExcelReader import convertFile
from ExcelReader import generateOutputPaths

default_rules = []


if __name__ == "__main__":
    vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath = generateOutputPaths("..\\..\\Physics2", "MotionAndPower")
    excelPath = "..\\excel\\Physics2\\SmartKG_KGDesc_MotionAndPower_zh.xlsx"
    convertFile(excelPath, [], ["MotionAndPower"], [], vJsonPath, eJsonPath, intentPath, entityMapPath, True, colorJsonPath)