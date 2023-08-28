using Coursesvc.Interfaces;
using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;
using SharedService.Interfaces;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Coursesvc.Services
{
    public class EnrollmentService : IEnrollment
    {
        private readonly IUnitofWorks _unitofWorks;
        private readonly IGenericRepository<Enrollment> _enrollmentcontext;
        private readonly IGenericRepository<Course> _coursecontext;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EnrollmentService> _logger;
        public EnrollmentService(IUnitofWorks unitofWorks, HttpClient httpClient, ILogger<EnrollmentService> logger)
        {
            _unitofWorks = unitofWorks;
            _enrollmentcontext = _unitofWorks.GetRepository<Enrollment>();
            _coursecontext = _unitofWorks.GetRepository<Course>();
            _httpClient = httpClient;
            _logger = logger;   
        }

        public async Task<IActionResult> Add(Enrollment enrollment)
        {
            double balance; // for saving the balance info from API
            bool isUpdate = false; //for import csv

            //Run the Api to check if the user exist and return the User's Balance
            var responsecheckuser = await _httpClient.GetAsync($"https://localhost:7226/api/Authenticate/checkuser?id={enrollment.UserId}"); //send request to API address
            
            if (responsecheckuser.IsSuccessStatusCode) //If APi success
            {
                balance = Double.Parse(await responsecheckuser.Content.ReadAsStringAsync()); // Save the balance from the API
            }
            else
                return new BadRequestObjectResult("Khong ton tai user");
            
            if(_coursecontext.GetById(enrollment.CouresId) == null) // check existing course
            {
                //_logger.LogInformation($"Khong co khoa hoc");
                return new BadRequestObjectResult("Khong ton tai khoa hoc");
            }
            if(enrollment.EnrolledDate == null)
            {
                enrollment.EnrolledDate = DateTime.Now;
            }
            if(_coursecontext.GetById(enrollment.CouresId).Price > balance)// check the balance is enough
            {
                //_logger.LogInformation($"So tien cua cousre {enrollment.Coures.Price}");
                return new BadRequestObjectResult("Khong du tien");
            }

            // if all logic above correct check if the user already have that course
            var exists = _enrollmentcontext.Find(e => e.CouresId == enrollment.CouresId && e.UserId == enrollment.UserId).FirstOrDefault(); 
            if (exists != null)
            {
                _logger.LogInformation($"Existed dk: {exists}");
                exists.EnrolledDate = DateTime.Now;
                isUpdate = true;
                _unitofWorks.Commit();

            }

            //object to save data for the API
            var balanceData = new
            {
                Balance = -_coursecontext.GetById(enrollment.CouresId).Price, // The change amount
                UserId = enrollment.UserId // The user to be changed
            };
            var content = new StringContent(JsonSerializer.Serialize(balanceData), Encoding.UTF8, "application/json"); // body content

            //Call the APi to change the balance
            var responsecahngebalance = await _httpClient.PutAsync($"https://localhost:7226/api/Authenticate/changebalance", content); //send request to API address
         
            if (responsecahngebalance.IsSuccessStatusCode) // if success
            {
                if(isUpdate) { return new OkObjectResult("Update Date Success"); }
                _logger.LogInformation($"Is Success");
                _enrollmentcontext.Add(enrollment); // Add the enrollment
                _unitofWorks.Commit(); // Commit
                return new OkResult();
            }
            else
            {
                return new BadRequestObjectResult($"Da xay ra loi gi do");
            }
        }

        public async Task<IActionResult> Remove(string userId, int CourseId)
        {
            try 
            {
                //find if there a an enrollment with the specific userId 
                var exists = _enrollmentcontext.Find(e => e.UserId == userId && e.CouresId == CourseId).FirstOrDefault();
                if (exists == null) // if not found
                {
                    return new BadRequestObjectResult("Not found the specific data"); //should have return NotFoundResult but use this for some log info :v
                }

                //check if the course exist again just for sure :v (yes it not necessary)
                var balancecheck = _coursecontext.GetById(CourseId);
                if(balancecheck == null) return new BadRequestObjectResult("Not found the specific course");

                //Object for API
                var balanceData = new
                {
                    Balance = balancecheck.Price,
                    UserId = userId
                };
                var content = new StringContent(JsonSerializer.Serialize(balanceData), Encoding.UTF8, "application/json"); // body content
                //API to change the balance of a specific User
                var responsecahngebalance = await _httpClient.PutAsync($"https://localhost:7226/api/Authenticate/changebalance", content); //send request to API address

                if (responsecahngebalance.IsSuccessStatusCode) // if success
                {
                    _enrollmentcontext.Remove(exists); // remove the enrollment
                    _unitofWorks.Commit(); // save the change
                    return new OkResult();
                }
                return new BadRequestObjectResult("Loi khi goi API ?"); // Shouldnt reach this far (no hope here) ?
   
            } 
            catch(Exception ex) 
            {
                return new BadRequestObjectResult($"Co loi xay ra:{ex}");
            }

            
        }

        public IEnumerable<Enrollment> GetAll()
        {
            return _enrollmentcontext.GetAll();
        }

        public Task<IActionResult> Update(Enrollment enrollment)
        {
            throw new NotImplementedException();
        }
    }  
}
