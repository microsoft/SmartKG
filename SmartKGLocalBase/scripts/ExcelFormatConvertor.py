# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

import json
from xlrd import open_workbook
import os.path
from os import path
import xlsxwriter

def createDirIfNoExists(destPath):
    tmps = destPath.split("\\")
    dir = "\\".join(tmps[:-1])
    if not os.path.exists(dir):
        os.makedirs(dir)

def convertFormat(srcPath, destPath):

    if os.path.exists(destPath):
        os.remove(destPath)

    createDirIfNoExists(destPath)

    input_wb = open_workbook(srcPath)
    input_sheet = input_wb.sheets()[0]

    output_wb = xlsxwriter.Workbook(destPath)
    output_sheet = output_wb.add_worksheet("Vertexes")

    for row in range(0, input_sheet.nrows):
        for col in range(0, input_sheet.ncols):
            item = input_sheet.cell_value(row, col)
            output_sheet.write(col, row, item)

    input_sheet =  input_wb.sheets()[1]
    output_sheet = output_wb.add_worksheet("Edges")

    for row in range(0, input_sheet.nrows):
        for col in range(0, input_sheet.ncols):
            item = input_sheet.cell_value(row, col)
            output_sheet.write(row, col, item)

    output_wb.close()

if __name__ == "__main__":

    inputPath = "..\\excel\\_OldExcels\\知识图谱导入模板-运动和力.xlsx"
    outputPath = "..\\excel\\Physics2\\SmartKG_KGDesc_MotionAndPower_zh.xlsx"

    convertFormat(inputPath, outputPath)

    print("Finished!")