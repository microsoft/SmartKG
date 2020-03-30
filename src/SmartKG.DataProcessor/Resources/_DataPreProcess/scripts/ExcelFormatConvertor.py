# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

import json
from xlrd import open_workbook
import os.path
from os import path
import xlsxwriter

def convertFormat(srcPath, destPath):

    if os.path.exists(destPath):
        os.remove(destPath)

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
    #inputPath = "..\\excel\\COVID19\\SmartKG_KGDesc_virsus.xlsx"
    #outputPath = "..\\excel\\COVID19\\SmartKG_KGDesc_COVID19_zh.xlsx"

    #inputPath = "..\\excel\\COVID19\\smartKG_virus_en.xlsx"
    #outputPath = "..\\excel\\COVID19\\SmartKG_KGDesc_COVID19_en.xlsx"

    #inputPath = "..\\excel\\Physics\\知识图谱导入模板-初二物理上声学.xlsx"
    #outputPath = "..\\excel\\Physics\\SmartKG_KGDesc_PhonicsGrade7_zh.xlsx"

    #inputPath = "..\\excel\\Physics\\知识图谱导入模板-初二物理第一+七+八章.xlsx"
    #outputPath = "..\\excel\\Physics\\SmartKG_KGDesc_MechanicsGrade7_zh.xlsx"

    #inputPath = "..\\..\\_Template\\SmartKG_KGDesc_Template.xlsx"
    #outputPath = "..\\..\\_Template\\SmartKG_KGDesc_Template_new.xlsx"

    #convertFormat(inputPath, outputPath)

    print("Finished!")