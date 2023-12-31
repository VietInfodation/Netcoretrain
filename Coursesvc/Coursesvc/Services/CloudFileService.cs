﻿using Coursesvc.Interfaces;
using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using SharedService.Interfaces;
using SharedService.Models;
using System.Data;
using System.Globalization;

namespace Coursesvc.Services
{
    public class CloudFileService : IFile
    {
        private readonly IUnitofWorks _unitofWorks;
        private readonly ICloudStorage _cloudStorage;
        private readonly CourseContext _coursecontext;
        private readonly IEnrollment _enrollmentservice;
        private readonly IGenericRepository<SharedService.Models.File> _context;
        private readonly HttpClient _httpClient;
        private readonly GoogleWorkingFileDictionaryOptions _options;
        private readonly ICsvService _csvservice;

        public CloudFileService(ICloudStorage cloudStorage, IUnitofWorks unitofWorks, CourseContext coursecontext, HttpClient httpClient, IEnrollment enrollmentservice, IOptions<GoogleWorkingFileDictionaryOptions> options, ICsvService csvservice)
        {
            _cloudStorage = cloudStorage;
            _unitofWorks = unitofWorks;
            _coursecontext = coursecontext;
            _context = _unitofWorks.GetRepository<SharedService.Models.File>();
            _httpClient = httpClient;
            _enrollmentservice = enrollmentservice;
            _options = options.Value;
            _csvservice = csvservice;
        }
        /// <summary>
        /// Add to the DataBase the File Name and the File link + Upload load file to the Google Cloud
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(SharedService.Models.File file)
        {
            try
            {
                if (file.FileUrl != null)
                {
                    await UploadFile(file); // Upload the File to GG Cloud
                }
                _context.Add(file); // Add the info to Database
                await _coursecontext.SaveChangesAsync();
                return new OkResult();
            }
            catch (Exception ex) { Console.WriteLine(ex); return new BadRequestResult(); }
        }

        /// <summary>
        /// Remove data from the Databae + File from the GG Cloud
        /// </summary>
        /// <param name="id">Id to delete</param>
        /// <returns></returns>
        public async Task<IActionResult> Remove(int id)
        {
            var file = _context.GetById(id); // find the row
            try
            {
                if (file == null)
                {
                    return new NotFoundResult();
                }
                if (file.FileStorageName != null)
                {
                    await _cloudStorage.DeleteFileAsync(file.FileStorageName); // delete the file from GG Cloud
                }
                _context.Remove(file);
                _unitofWorks.Commit();
                return new OkResult();
            }
            catch (Exception ex) { return new BadRequestObjectResult(ex); }
        }

        /// <summary>
        /// Download the file from GG Cloud
        /// </summary>
        /// <param name="id">Id for download</param>
        /// <returns></returns>
        public async Task<IActionResult> GetFile(int id)
        {
            var file = _context.GetById(id); // find the row
            try
            {
                if (file == null)
                {
                    return new NotFoundResult();
                }
                if (file.FileStorageName != null)
                {
                    await _cloudStorage.DownLoadFileAsync(file.FileStorageName, "~\\data"); // Download the file from GG Cloud
                }
                return new OkResult();
            }
            catch (Exception ex) { return new BadRequestObjectResult(ex); }
        }

        /// <summary>
        /// Called the service for Upload file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task UploadFile(SharedService.Models.File file)
        {
            //fileNameForStorage is the name of the file to be stored
            string fileNameForStorage = FormFileName(file.Id.ToString(), file.FileUrl.FileName);

            //the public link of the GG cloud file
            file.FileLink = await _cloudStorage.UploadFileAsync(file.FileUrl, fileNameForStorage);

            //Set the model ImageStorageName with the fileNameForStorage that we create above
            file.FileStorageName = fileNameForStorage;

        }

        //Export CSV and XlSX
        public async Task<IActionResult> WriteCSV()
        {
            try
            {
                IQueryable<Enrollment> listEnroll = _unitofWorks.GetRepository<Enrollment>().GetAll(); //Get the Enrollments
                IQueryable<Course> listCourse = _unitofWorks.GetRepository<Course>().GetAll(); //Get the Courses

                //Get a IQuery with the format of the Import file
                var list = from enroll in listEnroll
                           join course in listCourse on enroll.CouresId equals course.Id
                           where enroll.EnrolledDate >= DateTime.Now.AddMonths(-3) // EnrollDate within the last 3 months
                           select new
                           {
                               CourseCode = course.Code,
                               UserId = enroll.UserId ?? "",
                               IsEnroll = true,
                               EnrollDate = enroll.EnrolledDate,
                           };

                string saveLocationcsv = _csvservice.WriteCSV<dynamic>(list); //get the exported .csv location
                string saveLocationxlsx = _csvservice.WriteXlsx<dynamic>(list); //get the exported .xlsx location

                //Begin Upload two files
                using (var csvStream = new FileStream(saveLocationcsv, FileMode.Open))
                using (var excelStream = new FileStream(saveLocationxlsx, FileMode.Open))
                {
                    IFormFile csvFormFile = new FormFile(csvStream, 0, csvStream.Length, null, Path.GetFileName(saveLocationcsv));
                    IFormFile excelFormFile = new FormFile(excelStream, 0, excelStream.Length, null, Path.GetFileName(saveLocationxlsx));

                    // Upload CSV file
                    await _cloudStorage.UploadFileAsync(csvFormFile, $"Course/export/csv/{saveLocationcsv}");

                    // Upload Excel file
                    await _cloudStorage.UploadFileAsync(excelFormFile, $"Course/export/excel/{saveLocationxlsx}");
                }

                System.IO.File.Delete(saveLocationcsv);
                System.IO.File.Delete(saveLocationxlsx);

                return new OkResult();
            }
            catch (Exception ex) { return new BadRequestObjectResult(ex); }
        }

        //Import .csv file
        public async Task<IActionResult> ImportCsv(string urlFile)
        {
            string fileName = Path.GetFileName(urlFile); //Get file name from url
            // Download the file from GG Cloud
            //"Assume" there are file in /new
            await _cloudStorage.DownLoadFileAsync(fileName, "data");
            var checkrule = await RulingImportCsv(fileName); // Checking rule
            if (!checkrule)
            {
                await _cloudStorage.MoveFileInGCS($"{_options.New}/{fileName}", $"{_options.Failed}/{fileName}");
                return new BadRequestObjectResult("Moved to failed");
            }
            await _cloudStorage.MoveFileInGCS($"{_options.New}/{fileName}", $"{_options.Completed}/{fileName}");
            return new OkObjectResult("Move to completed");
        }

        //Import and Check the rule for import
        public async Task<dynamic> RulingImportCsv(string fileName)
        {
            List<string> listCol = new List<string> { "CourseCode", "UserId", "IsEnroll", "EnrollDate" };
            //string fileName = "RegisterCourse_code1_2023_08_23.csv";
            List<string> duplicateCheck = new List<string>(); //Dictionary for checking duplicate key

            var checkFormat = await CheckFileFormat(fileName); // Check File Format

            var table = await GetDataTabletFromCSVFile($"data\\{fileName}"); //Import the .csv to a DataTable for read

            if (table == null) { return false; }

            if (!checkFormat || table.Columns.Count != 4) { return false; } //+ Nếu file sai, thiếu, thừa headers => reject file

            // Check if all DataTable columns exist in the listCol
            //What if the order wrong? How can we check follow order if there are a lot of column?
            bool allColumnsExist = table.Columns
                .Cast<DataColumn>()
                .All(column => listCol.Contains(column.ColumnName));

            if (!allColumnsExist) { return false; }

            //Get the Id Course
            string[] parts = fileName.Split('_');
            string courseCode = parts[1];
            int idCoures = _coursecontext.Courses
                   .Where(entry => entry.Code == parts[1])
                   .Select(entry => (int)entry.Id)
                   .FirstOrDefault();
            if (idCoures.ToString() == null) { return false; } //Check if the Id Code exist in DB

            foreach (DataRow row in table.Rows)
            {
                // Access each column value using the column name or index
                if (row[0].ToString() != courseCode) { continue; } // + Nếu row nào có CourseCode khác với Course_code in file name
                if (row[2].ToString() == null || row[2].ToString() == "" || row[0].ToString() == null || row[1].ToString() == null) { continue; } //If null => reject
                if (duplicateCheck.Any(pair => pair == row[1].ToString())) { continue; } //Nếu userId duplicate in file => reject
                duplicateCheck.Add(row[1].ToString());
                //Run the Api to check if the user exist and return the User's Balance
                var responsecheckuser = await _httpClient.GetAsync($"https://localhost:7226/api/Authenticate/checkuser?id={row[1]}"); //send request to API address

                if (!responsecheckuser.IsSuccessStatusCode) //If APi failed (no User)
                {
                    continue;
                }
                if (row[2].ToString() == "false") { await _enrollmentservice.Remove(row[1].ToString(), idCoures); continue; } // + Nếu IsEnroll = false => user hủy khóa học

                //var checkExist = _unitofWorks.GetRepository<Enrollment>().Find(e => e.CouresId == idCoures && e.UserId == row[1].ToString()).FirstOrDefault();

                //+ Nếu IsEnroll = true and EnrollDate = null => EnrollDate default = today
                DateTime date = DateTime.Now;
                if (row[3].ToString() == "" || row[3].ToString() == null)
                    date = DateTime.Now;
                else
                    date = DateTime.Parse(row[3].ToString());

                //I have fix the Add method so that if the User already subcribe then it only update the Date and Balance (not add new row)
                //The problem is that it will ignore the Buz about checking if the User already subcribe in the old Topic (will fix later)
                await _enrollmentservice.Add(
                    new Enrollment
                    {
                        CouresId = idCoures,
                        UserId = row[1].ToString(),
                        EnrolledDate = date
                    });
            }
            return true; //All rule for file is pass
        }

        /// <summary>
        /// Create file name format
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string FormFileName(string title, string fileName)
        {
            //!!!Should update for upload .csv file
            var fileExtension = Path.GetExtension(fileName);
            var fileNameForStorage = $"{title}-{DateTime.Now.ToString("yyyyMMddHHmmss")}{fileExtension}";
            return fileNameForStorage;
        }

        //For testing CheckDuplicate, you can ignore this
        public async Task<IActionResult> GetList(string fileName)
        {
            var check = await _cloudStorage.CheckDuplicate(fileName);
            return new OkObjectResult(check);
        }

        private async Task<bool> CheckFileFormat(string fileName)
        {
            //+ Nếu file name khác format => move file course/failed
            if (Path.GetExtension(fileName) != ".csv") { return false; }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            // Split the filename using underscores
            string[] parts = fileNameWithoutExtension.Split('_');

            if (parts.Length != 5) { return false; }

            if (parts.Any(value => value.Equals(""))) { return false; }
            //---------------------------------------------------------------

            if (await _cloudStorage.CheckDuplicate(fileName)) { return false; } //+ Nếu file name duplicate với 1 file trước đó

            //Check date format, it the other name format is true then there will be enough part
            //If not There are chance to get out of index exception
            string dateString = $"{parts[2]}_{parts[3]}_{parts[4]}";
            string format = "yyyy_MM_dd";
            DateTime result;

            if (!DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return false;
            }

            return true;
        }

        private static async Task<DataTable> GetDataTabletFromCSVFile(string csv_file_path)
        {
            //Shoudld we check the column and row format here ?
            //Read the csv to a DataTable 
            DataTable csvData = new DataTable();
            //List<string> columns = new List<string>();
            try
            {
                //!!!Null check ?
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { ";" });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                        if (csvData.Rows.Count != 4) { throw new Exception("there are some line that doesn't have ; delimiter"); } //if there a line with "," => throw error
                    }
                }
            }
            catch
            {
                return null;
            }
            return csvData;
        }
    }
}
