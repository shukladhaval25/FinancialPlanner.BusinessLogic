using FinancialPlanner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialPlanner.BusinessLogic
{
    public class CommonService
    {
        public string GetString(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;
            if (!System.IO.File.Exists(filePath))
            {
                Logger.LogDebug("File not found :" + filePath);
                return string.Empty;
            }

            return Common.DataConversion.FileConversion.GetStringfromFile(filePath);
        }
    }
}
