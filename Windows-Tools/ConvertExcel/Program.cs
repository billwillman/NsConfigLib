using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConvertExcel
{
    class Program
    {
        static void Test()
        {
            string excelFileName = "../../../table/test.xlsx";
            bool ret = ExcelConvert.ConvertSheet(excelFileName);
            if (ret)
                Console.WriteLine("导出完成");
            else
                Console.WriteLine("导出失败");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            Test();
            return;

            if (args.Length <= 1)
            {
                Console.WriteLine(string.Format("command paramCount {0} must > 1", args.Length));
                Console.ReadLine();
                return;
            }
            string excelFileName = args[1];
            if (!File.Exists(excelFileName))
            {
                Console.WriteLine(string.Format("Excel File {0} not found~!", excelFileName));
                Console.ReadLine();
                return;
            }
            bool ret = ExcelConvert.ConvertSheet(excelFileName);
            if (ret)
                Console.WriteLine("导出完成");
            else
                Console.WriteLine("导出失败");
            Console.ReadLine();
        }
    }
}
