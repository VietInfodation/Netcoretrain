using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Excel;
using SharedService.Interfaces;
using System.Globalization;

namespace SharedService.Services
{
    public class CsvService : ICsvService
    {
        public string WriteCSV<dynamic>(IQueryable<dynamic> records)
        {
            //Format name
            string fileName = $"Export_Backup_Course_{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Second}.csv";

            using (var writer = new StreamWriter(Path.Combine("data", "export", fileName)))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
            return Path.Combine("data", "export", fileName); //return the file location
        }
        public string WriteXlsx<dynamic>(IQueryable<dynamic> records)
        {
            //Format name
            string fileNamexl = $"Export_Backup_Course_{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Second}.xlsx";

            //using (var writer = new StreamWriter(Path.Combine("data", "export", fileName)))
            using (var xlsx = new ExcelWriter(Path.Combine("data", "export", fileNamexl)))
            {
                xlsx.WriteRecords(records);
            }

            using (XLWorkbook workbook = new XLWorkbook(Path.Combine("data", "export", fileNamexl)))
            {
                IXLWorksheet worksheet = workbook.Worksheet(1);
                worksheet.RangeUsed().SetAutoFilter();
                workbook.Save();
            }

            return Path.Combine("data", "export", fileNamexl); //return the file location
        }
    }
}
