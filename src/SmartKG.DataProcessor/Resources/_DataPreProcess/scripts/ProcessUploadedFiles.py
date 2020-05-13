import argparse
from ExcelReader import convertFile
from ExcelReader import generateOutputPaths


def processOneExcelFile(srcPath, scenario, destPath, isFirst):
    vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath = generateOutputPaths(destPath, scenario, isFirst)
    convertFile(srcPath, [], [scenario], [], vJsonPath, eJsonPath, intentPath, entityMapPath, colorJsonPath)

    return

def processExcelFiles(srcPaths, scenarios, destPath):
    if len(srcPaths) == 0:
        print("Error: Invalid srcPath. There is at least a srcPath.")
        return

    if len(scenarios) > len(srcPaths):
        print("Warning: useless scenario names. Will be ignored.")

    isFirst = True

    for index in range(0, len(srcPaths)):
        if index > 0:
            isFirst = False;

        srcPath = srcPaths[index]

        if index >= len(scenarios):
            scenario = "DEFAULT"
        else:
            scenario = scenarios[index]

        processOneExcelFile(srcPath, scenario, destPath, isFirst)

    return


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Process excel files for KG.')
    parser.add_argument('--srcPaths', metavar='S', type=str, nargs='+',
                    help='path of source excel file(s)')
    parser.add_argument('--destPath', metavar='D', type=str,
                    help='path of destination dir path')
    parser.add_argument('--scenarios', metavar="N", type=str, nargs='+',
                    help= "scenario name for excel file, default value is 'DEFAULT'")

    args = parser.parse_args()

    processExcelFiles(args.srcPaths, args.scenarios, args.destPath)