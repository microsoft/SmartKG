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

        public (List<Vertex>, List<Edge>) ParserExcel(string path)
        {
            var fi = new FileInfo(path);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Dictionary<string, Vertex> vRows = new Dictionary<string, Vertex>();
            Dictionary<string, Edge> eRows = new Dictionary<string, Edge>();

            using (ExcelPackage package = new ExcelPackage(fi))
            {
                //get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                
                int colCount = worksheet.Dimension.End.Column;  //get Column Count
                int rowCount = worksheet.Dimension.End.Row;     //get row count
                
                for (int row = 2; row <= rowCount; row++)
                {
                    Vertex aVertex = new Vertex();
                    VertexProperty p = null;

                    for (int col = 1; col <= colCount; col++)
                    {
                        string cellStr = worksheet.Cells[row, col].Value?.ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(cellStr))
                        {
                            switch (col)
                            {
                                case 1:

                                    aVertex.id = cellStr;
                                    break;
                                case 2:
                                    aVertex.name = cellStr;
                                    break;
                                case 3:
                                    aVertex.label = cellStr;
                                    break;
                                case 4:
                                    aVertex.leadSentence = cellStr;
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
                                            aVertex.properties.Add(p);
                                        }
                                    }
                                    
                                    break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(aVertex.id) && !string.IsNullOrWhiteSpace(aVertex.name) && !string.IsNullOrWhiteSpace(aVertex.label))
                    {
                        if (!vRows.ContainsKey(aVertex.id))
                        {
                            vRows.Add(aVertex.id, aVertex);
                        }                       
                    }
                }

                worksheet = package.Workbook.Worksheets[1];

                colCount = worksheet.Dimension.End.Column;  //get Column Count
                rowCount = worksheet.Dimension.End.Row;     //get row count
                for (int row = 2; row <= rowCount; row++)
                {
                    Edge aEdge = new Edge();

                    for (int col = 1; col <= colCount; col++)
                    {
                        string cellStr = worksheet.Cells[row, col].Value?.ToString().Trim();

                        if (!string.IsNullOrWhiteSpace(cellStr))
                        {
                            switch (col)
                            {
                                case 1:

                                    aEdge.relationType = cellStr;
                                    break;
                                case 2:
                                    aEdge.headVertexId = cellStr;
                                    break;
                                case 3:
                                    aEdge.tailVertexId = cellStr;
                                    break;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(aEdge.relationType) && !string.IsNullOrWhiteSpace(aEdge.headVertexId) && !string.IsNullOrWhiteSpace(aEdge.tailVertexId))
                    {
                        if (vRows.ContainsKey(aEdge.headVertexId) && vRows.ContainsKey(aEdge.tailVertexId))
                        {
                            string key = aEdge.relationType + aEdge.headVertexId + aEdge.tailVertexId;

                            if (!eRows.ContainsKey(key))
                            {
                                eRows.Add(key, aEdge);
                            }
                        }
                    }                    
                }
            }

            return (vRows.Values.ToList<Vertex>(), eRows.Values.ToList<Edge>());
        }
    }
}
