using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
// 引用Microsoft.Office.Interop.Excel.dll
using Microsoft.Office.Interop;
// 引用Office.dll
using Microsoft.Office.Core;

namespace ConvertExcel
{
    public class ExcelConvert: IDisposable
    {
        private string m_FileName = string.Empty;
        private Microsoft.Office.Interop.Excel.Application m_App = null;
        private Microsoft.Office.Interop.Excel.Workbooks m_Books = null;
        private Microsoft.Office.Interop.Excel.Workbook m_Book = null;
        private void Close()
        {
            m_FileName = string.Empty;
            if (m_Book != null)
            {
                m_Book.Close(Type.Missing, Type.Missing, Type.Missing);
                m_Book = null;
            }
            if (m_Books != null)
            {
                m_Books.Close();
                m_Books = null;
            }
            if (m_App != null)
            {
                m_App.Quit();
                m_App = null;
            }
            GC.Collect();
        }

        public string FileName
        {
            get
            {
                return m_FileName;
            }
        }

        public bool ConvertSheet(Microsoft.Office.Interop.Excel.Worksheet sheet, string exportDir)
        {
            if (sheet == null)
                return false;
            
            sheet.Activate();

            if (sheet.Cells == null || string.IsNullOrEmpty(exportDir) || sheet.UsedRange.Rows.Count < 3 || sheet.UsedRange.Columns.Count <= 0)
                return false;
           
            // 转换
            if (!ConvertSheetToCs(sheet, exportDir))
                return false;

            if (!ConvertSheetToJson(sheet, exportDir))
                return false;

            

            return true;
        }

        // 表第一个字段为KEY
        private bool ConvertSheetToJson(Microsoft.Office.Interop.Excel.Worksheet sheet, string exportDir)
        {
           
            return true;
        }

        private void WriteCsNameSpaceBegin(Stream stream)
        {
            string usingStr = "using System;\r\n" +
                              "using System.Collections.Generic;\r\n\r\n";
            string nameSpaceBegin = "namespace NsLib.Config.Table \r\n" +
                                     "{\r\n";

            byte[] buf = System.Text.Encoding.UTF8.GetBytes(usingStr);
            stream.Write(buf, 0, buf.Length);
            buf = System.Text.Encoding.UTF8.GetBytes(nameSpaceBegin);
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }

        private void WriteCsNameSpaceEnd(Stream stream)
        {
            string nameSpaceEnd = "\r\n}";
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(nameSpaceEnd);
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }

        private string GetExcelTypeStr(string varType)
        {
            if (string.IsNullOrEmpty(varType))
                return string.Empty;
            if (string.Compare(varType, "string", true) == 0)
                return "string";
            if (string.Compare(varType, "int", true) == 0)
                return "int";
            if (string.Compare(varType, "uint", true) == 0)
                return "uint";
            if (string.Compare(varType, "short", true) == 0)
                return "short";
            if (string.Compare(varType, "byte", true) == 0)
                return "byte";
            if (string.Compare(varType, "ushort", true) == 0)
                return "ushort";
            if (string.Compare(varType, "int16", true) == 0)
                return "short";
            if (string.Compare(varType, "uint16", true) == 0)
                return "ushort";
            if (string.Compare(varType, "int32", true) == 0)
                return "int";
            if (string.Compare(varType, "uint32", true) == 0)
                return "uint";
            if (string.Compare(varType, "sbyte", true) == 0)
                return "sbyte";
            if (string.Compare(varType, "int8", true) == 0)
                return "sbyte";
            if (string.Compare(varType, "uint8", true) == 0)
                return "byte";
            if (string.Compare(varType, "long", true) == 0)
                return "long";
            if (string.Compare(varType, "ulong", true) == 0)
                return "ulong";
            if (string.Compare(varType, "int64", true) == 0)
                return "long";
            if (string.Compare(varType, "uint64", true) == 0)
                return "ulong";
            if (string.Compare(varType, "float", true) == 0)
                return "float";
            if (string.Compare(varType, "float32", true) == 0)
                return "float";
            if (string.Compare(varType, "double32", true) == 0)
                return "float";
            if (string.Compare(varType, "double", true) == 0)
                return "double";
            if (string.Compare(varType, "float64", true) == 0)
                return "double";
            if (string.Compare(varType, "double64", true) == 0)
                return "double";

            return string.Empty;
        }

        private void WriteCsTableClass(Stream stream, string className, string[] varNames, string[] varDescs, string[] varTypes)
        {
            string classBegin = "  public class " + className +  "\r\n  {\r\n";
            byte[] buf = System.Text.Encoding.UTF8.GetBytes(classBegin);
            stream.Write(buf, 0, buf.Length);

            for (int i = 0; i < varNames.Length; ++i)
            {
                string varType = GetExcelTypeStr(varTypes[i]);
                if (string.IsNullOrEmpty(varType))
                {
                    string err = string.Format("Name {0} type {1} is error~!", varNames[i], varTypes[i]);
                    continue;
                }
                string itemStr = string.Format("    //{0}\r\n    [ConfigId({1})]\r\n    public {2} {3}\r\n    {{get; set;}}\r\n\r\n", varDescs[i], i, varType, varNames[i]);
                buf = System.Text.Encoding.UTF8.GetBytes(itemStr);
                stream.Write(buf, 0, buf.Length);
            }

            buf = System.Text.Encoding.UTF8.GetBytes("  }");
            stream.Write(buf, 0, buf.Length);
            stream.Flush();
        }

        private bool ConvertSheetToCs(Microsoft.Office.Interop.Excel.Worksheet sheet, string exportDir)
        {
            int colNum = sheet.UsedRange.Columns.Count;
            string[] varNames = new string[colNum];
            for (int i = 1; i <= colNum; ++i)
            {
                var cell = sheet.Cells[1, i];
                
                varNames[i - 1] = cell.Value2.ToString();
            }

            string[] varDescs = new string[colNum];
            for (int i = 1; i <= colNum; ++i)
            {
                var cell = sheet.Cells[2, i];
                varDescs[i - 1] = cell.Value2.ToString();
            }

            string[] varTypes = new string[colNum];
            for (int i = 1; i <= colNum; ++i)
            {
                var cell = sheet.Cells[3, i];
                varTypes[i - 1] = cell.Value2.ToString();
            }

            if (varNames.Length != varDescs.Length || varDescs.Length != varTypes.Length)
                return false;

            string csFileName = string.Format("{0}/{1}.cs", exportDir, sheet.Name);
            FileStream outputStream = new FileStream(csFileName, FileMode.Create, FileAccess.Write);
            try
            {
                WriteCsNameSpaceBegin(outputStream);
                WriteCsTableClass(outputStream, sheet.Name, varNames, varDescs, varTypes);
                WriteCsNameSpaceEnd(outputStream);
            } finally
            {
                outputStream.Close();
                outputStream.Dispose();
            }

            return true;
        }

        private Microsoft.Office.Interop.Excel.Sheets GetSheets()
        {
            if (m_Book == null)
                return null;

            return m_Book.Worksheets;
        }


        // 获得当前的表
        private Microsoft.Office.Interop.Excel.Worksheet GetSheet(string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName) || m_Book == null || m_Book.Worksheets == null || m_Book.Worksheets.Count <= 0)
                return null;
            
            try
            {
                Microsoft.Office.Interop.Excel.Worksheet ret = m_Book.Worksheets[sheetName] as Microsoft.Office.Interop.Excel.Worksheet;
                return ret;
            } catch
            {
                return null;
            }
        }

        public static bool ConvertSheet(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return false;
            fileName = Path.GetFullPath(fileName);
            ExcelConvert convert = new ExcelConvert();
            bool ret = convert.OpenExcel(fileName);
            if (!ret)
            {
                convert.Dispose();
                return ret;
            }
            try
            {
                string dir = Path.GetDirectoryName(fileName);
                var sheets = convert.GetSheets();
                if (sheets == null)
                {
                    return false;
                }
                
                var iter = sheets.GetEnumerator();
                while (iter.MoveNext())
                {
                    var sheet = iter.Current as Microsoft.Office.Interop.Excel.Worksheet;
                    if (sheet != null)
                    {
                        bool isOk = convert.ConvertSheet(sheet, dir);
                        if (!isOk)
                        {
                            string err = string.Format("【Config:{0}】sheet {1} convert error~!", fileName, sheet.Name);
                            Console.WriteLine(err);
                        }
                    }
                }

                return true;
            } finally
            {
                convert.Dispose();
            }
        
        }

        // 打开EXCEL文件
        public bool OpenExcel(string fileName)
        {
            Close();

            if (string.IsNullOrEmpty(fileName))
                return false;

            fileName = Path.GetFullPath(fileName);
            m_App = new Microsoft.Office.Interop.Excel.Application();
            m_App.Visible = false;
            m_Book = m_App.Workbooks.Open(fileName);

            m_FileName = fileName;

            return true;
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
