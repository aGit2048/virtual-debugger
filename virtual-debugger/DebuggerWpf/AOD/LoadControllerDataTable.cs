using FileUtility;
using System;

namespace DebuggerWpf.AOD
{
    internal static class LoadControllerDataTable
    {
        public static async void LoadExecel(Action<ExcelLoadResult> callback)
        {
            ExcelLoadResult result = await ExcelReader.LoadExcelParallelAsync(@"C:\Users\hh\Desktop\ControllerData_All.xlsx", 1, true);
            callback?.Invoke(result);
        }
    }
}
