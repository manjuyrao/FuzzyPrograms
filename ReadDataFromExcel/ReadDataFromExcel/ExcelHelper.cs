using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;
using System.Linq;

namespace ReadDataFromExcel
{
    public class ExcelHelper
    {
        // These numbers are the format ids that correspond to OA dates in Excel/OOXML Spreadsheets
        // Add more formats as required
        private int[] dateNumberFormats = new int[] { 14, 15, 16, 17, 22, 165 };

        private string GetCellValue(Cell cell)
        {
            if (cell == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(cell.DataType))
            {
                var dateString = string.Empty;

                if (TryParseDate(cell, out dateString))
                {
                    return dateString;
                }

                return cell.InnerText;
            }

            switch (cell.DataType.Value)
            {
                case CellValues.SharedString:
                    {
                        var worksheet = FindParentWorksheet(cell);
                        var sharedStringTablePart = FindSharedStringTablePart(worksheet);

                        if (sharedStringTablePart != null &&
                            sharedStringTablePart.SharedStringTable != null)
                        {
                            return sharedStringTablePart.SharedStringTable.ElementAt(int.Parse(cell.InnerText)).InnerText;
                        }
                        break;
                    }
                case CellValues.Boolean:
                    {
                        return cell.InnerText == "0" ?
                            bool.FalseString : bool.TrueString;
                    }
            }

            return cell.InnerText;
        }

        private SharedStringTablePart FindSharedStringTablePart(Worksheet worksheet)
        {
            var document = worksheet.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;

            return document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
        }

        private Worksheet FindParentWorksheet(Cell cell)
        {
            var parent = cell.Parent;

            while (parent.Parent != null &&
                    parent.Parent != parent &&
                    !parent.LocalName.Equals("worksheet", StringComparison.InvariantCultureIgnoreCase))
            {
                parent = parent.Parent;
            }

            if (!parent.LocalName.Equals("worksheet", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Worksheet invalid");
            }

            return parent as Worksheet;
        }

        private bool TryParseDate(Cell cell, out string dateString)
        {
            dateString = null;

            if (cell.CellValue == null || cell.StyleIndex == null ||
                !cell.StyleIndex.HasValue)
            {
                return false;
            }

            var worksheet = FindParentWorksheet(cell);
            var document = worksheet.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;
            var styleSheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
            var cellStyle = styleSheet.CellFormats.ChildElements[(int)cell.StyleIndex.Value];
            var formatId = (cellStyle as CellFormat).NumberFormatId;

            //If the exel format is present in the provided array, add the new format number
            if (dateNumberFormats.Contains((int)formatId.Value))
            {
                dateString = DateTime.FromOADate(double.Parse(cell.CellValue.InnerText)).ToString("dd-MM-yyyy HH:mm:ss");
                return true;
            }

            return false;
        }

        public DataSet GetDataBySheetName(string filePath, string sheetName)
        {
            DataSet dataSet = new DataSet();

            using (var spreadsheetDocument = SpreadsheetDocument.Open(filePath, false))
            {
                var workBookPart = spreadsheetDocument.WorkbookPart;

                foreach (var sheet in workBookPart.Workbook.Descendants<Sheet>())
                {
                    var worksheetPart = workBookPart.GetPartById(sheet.Id) as WorksheetPart;

                    if (worksheetPart == null)
                    {
                        // the part was supposed to be here, but wasn't found :/
                        continue;
                    }

                    if (sheet.Name.HasValue && sheet.Name.Value.Equals(sheetName))
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.TableName = sheetName;

                        foreach (var row in worksheetPart.Worksheet.Descendants<Row>())
                        {
                            DataRow dataRow = dataTable.NewRow();
                            int colIndex = 0;
                            int presentColCount = dataTable.Columns.Count;
                            int requiredColCount = row.Elements<Cell>().Count();
                            if (requiredColCount > presentColCount)
                            {
                                int columnsToAdd = requiredColCount - presentColCount;
                                for (int i = 0; i < columnsToAdd; i++)
                                {
                                    dataTable.Columns.Add("", typeof(string));
                                }
                            }

                            foreach (Cell c in row.Elements<Cell>())
                            {
                                var value = GetCellValue(c);

                                dataRow[colIndex++] = value;
                            }
                            dataTable.Rows.Add(dataRow);
                        }

                        dataSet.Tables.Add(dataTable);
                        //Required sheet found. 
                        break;
                    }
                }
            }

            return dataSet;
        }
    }
}