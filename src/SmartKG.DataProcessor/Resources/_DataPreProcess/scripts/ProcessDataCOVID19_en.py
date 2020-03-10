from ExcelReader import convertFile
from ExcelReader import generateOutputPaths


if __name__ == "__main__":
    targetPath = "..\\..\\COVID19_en"

    vJsonPath, eJsonPath, intentPath, entityMapPath = generateOutputPaths(targetPath, "COVID19_en")

    excelPath = "..\\excel\\COVID19\\SmartKG_virus_en.xlsx"

    convertFile(excelPath, [], ["COVID19_en"], vJsonPath, eJsonPath, intentPath, entityMapPath, True)