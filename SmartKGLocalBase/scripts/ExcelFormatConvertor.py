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

    inputPath = "..\\..\\Resources\\Data\\Excel\\input\\XYJ\\SmartKG_Xiyouji_relations.xlsx"
    outputPath = "..\\temp\\test"

    convertFormat(inputPath, outputPath)

    print("Finished!")