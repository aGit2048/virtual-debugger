using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUtility
{
    public static class ExcelReader
    {
        public static async Task<ExcelLoadResult> LoadExcelParallelAsync(string filePath, int sheetIndex = 0, bool hasHeader = true, Action<int> processChanged = null)
        {
            return await Task.Run(() => LoadExcelParallel(filePath, sheetIndex, hasHeader, processChanged));
        }

        private static ExcelLoadResult LoadExcelParallel(string filePath, int sheetIndex = 1, bool hasHeader = true, Action<int> processChanged = null)
        {
            ExcelLoadResult result = new ExcelLoadResult();
            try
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[sheetIndex];
                    int totalRows = worksheet.Dimension.Rows;
                    int totalColumns = worksheet.Dimension.Columns;
                    int startRow = hasHeader ? 2 : 1;
                    int dataRowCount = totalRows - startRow + 1;

                    // 存储结果和列名
                    result.Data = new List<string[]>();
                    result.ColumnNames = new string[totalColumns];

                    // 读取列名
                    if (hasHeader)
                    {
                        for (int col = 1; col <= totalColumns; col++)
                        {
                            result.ColumnNames[col - 1] = worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}";
                        }
                    }

                    // 预分配内存
                    result.Data.Capacity = dataRowCount;
                    var tempResults = new string[dataRowCount][];

                    int processedRows = 0;
                    object progressLock = new object();
                    int lastReportedProgress = -1;

                    // 使用并行处理读取数据行
                    Parallel.For(0, dataRowCount, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    }, index =>
                    {
                        int actualRow = startRow + index;
                        var rowData = new string[totalColumns];

                        for (int col = 1; col <= totalColumns; col++)
                        {
                            var cellValue = worksheet.Cells[actualRow, col].Value;
                            rowData[col - 1] = cellValue?.ToString() ?? string.Empty;
                        }

                        tempResults[index] = rowData;

                        // 线程安全的进度更新（减少更新频率）
                        lock (progressLock)
                        {
                            processedRows++;
                            int progress = (int)((double)processedRows / dataRowCount * 100);

                            // 只在进度变化时报告，且每5%报告一次
                            if (progress != lastReportedProgress && progress % 5 == 0)
                            {
                                lastReportedProgress = progress;
                                processChanged?.Invoke(progress);
                            }
                        }
                    });

                    // 将结果转换为列表
                    result.Data = tempResults.ToList();
                    result.TotalRows = dataRowCount;
                    result.TotalColumns = totalColumns;

                    processChanged?.Invoke(100);
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            return result;
        }
    }
}
