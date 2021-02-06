using OfficeOpenXml;
using SmartKG.Common.Data.KG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartKG.Common.Parser
{

    public class ExcelParser
    {

        public (List<ExcelSheetVertexesRow>, List<ExcelSheetEdgesRow>) ParserExcel(string path)
        {
            var fi = new FileInfo(path);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Dictionary<string, ExcelSheetVertexesRow> vRows = new Dictionary<string, ExcelSheetVertexesRow>();
            Dictionary<string, ExcelSheetEdgesRow> eRows = new Dictionary<string, ExcelSheetEdgesRow>();

            using (ExcelPackage package = new ExcelPackage(fi))
            {
                //get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                
                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                
                for (int row = 2; row <= rowCount; row++)
                {
                    ExcelSheetVertexesRow vRow = new ExcelSheetVertexesRow();
                    VertexProperty p = null;

                    for (int col = 1; col <= colCount; col++)
                    {
                        string cellStr = worksheet.Cells[row, col].Value?.ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(cellStr))
                        {
                            switch (col)
                            {
                                case 1:

                                    vRow.entityId = cellStr;
                                    break;
                                case 2:
                                    vRow.entityName = cellStr;
                                    break;
                                case 3:
                                    vRow.entityType = cellStr;
                                    break;
                                case 4:
                                    vRow.leadingSentence = cellStr;
                                    break;
                                default:
                                    if (col % 2 == 1)
                                    {
                                        p = new VertexProperty();
                                        p.name = cellStr;

                                        col += 1;
                                        cellStr = worksheet.Cells[row, col].Value?.ToString().Trim();

                                        if (!string.IsNullOrWhiteSpace(cellStr))
                                        { 
                                            p.value = cellStr;
                                            vRow.properties.Add(p);
                                        }
                                    }
                                    
                                    break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(vRow.entityId) && !string.IsNullOrWhiteSpace(vRow.entityName) && !string.IsNullOrWhiteSpace(vRow.entityType))
                    {
                        if (!vRows.ContainsKey(vRow.entityId))
                        {
                            vRows.Add(vRow.entityId, vRow);
                        }                       
                    }
                }

                worksheet = package.Workbook.Worksheets[1];

                colCount = worksheet.Dimension.End.Column;  //get Column Count
                rowCount = worksheet.Dimension.End.Row;     //get row count
                for (int row = 2; row <= rowCount; row++)
                {
                    ExcelSheetEdgesRow eRow = new ExcelSheetEdgesRow();

                    for (int col = 1; col <= colCount; col++)
                    {
                        string cellStr = worksheet.Cells[row, col].Value?.ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(cellStr))
                        {
                            switch (col)
                            {
                                case 1:

                                    eRow.relationType = cellStr;
                                    break;
                                case 2:
                                    eRow.sourceEntityId = cellStr;
                                    break;
                                case 3:
                                    eRow.targetEntityId = cellStr;
                                    break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(eRow.relationType) && !string.IsNullOrWhiteSpace(eRow.sourceEntityId) && !string.IsNullOrWhiteSpace(eRow.targetEntityId))
                    {
                        if (vRows.ContainsKey(eRow.sourceEntityId) && vRows.ContainsKey(eRow.targetEntityId))
                        {
                            string key = eRow.relationType + eRow.sourceEntityId + eRow.targetEntityId;

                            if (!eRows.ContainsKey(key))
                            {
                                eRows.Add(key, eRow);
                            }
                        }
                    }                    
                }
            }

            return (vRows.Values.ToList<ExcelSheetVertexesRow>(), eRows.Values.ToList<ExcelSheetEdgesRow>());
        }
    }
}
