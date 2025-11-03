using FileUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ExcelLoadResult result = ExcelReader.LoadExcelParallelAsync(@"C:\Users\hh\Desktop\ControllerData_All.xlsx", 1, true).GetAwaiter().GetResult();

          

            for (int i = 0; i < result.Data.Count; i++)
            {
                string row = string.Empty;
                for (int j = 0; j < result.Data[i].Length; j++)
                {
                    row += $"{result.Data[i][j]}    ";
                }
                Console.WriteLine(row);
            }

            Console.ReadKey();
        }
    }
}
