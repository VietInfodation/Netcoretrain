using SharedService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Coursesvc.Models;

namespace Coursesvc.Controllers
{
    //This controller handle service by itself not using repository so you should check CourseRepository instead
    [ApiController]
    [Route("course")]
    public class CoursesController : Controller
    {
        private readonly CourseContext _context;
        private readonly ILogger _logger;

        public CoursesController(CourseContext context, ILogger<CoursesController> logger)
        {
            _context = context;
            _logger = logger;
            Message = "";
        }


        public string Message { get; set; }


        //[Authorize(Roles = "Admin,User")]
        [HttpGet]
        public IEnumerable<Course> GetAll()
        {
            try
            {
                var course = _context.Courses.ToList<Course>(); // get the contact list
                Message = $"View data list at {DateTime.UtcNow.ToLongDateString()}";
                _logger.LogInformation(Message);
                return course;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception during providing products, maybe DB is not fully initialized yet? " +
                                  $"Try again in a few minutes and if it doesn't help, check your docker-compose configuration.\n{e}");

                return new Course[0]; // return null
            }
        }

        // GET: /Contacts/{id}
        //[Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var course = _context.Courses.FirstOrDefault(c => c.Id == id); // get the first contact with the given id
                if (course == null)
                {
                    return NotFound(); // Return 404 if not found
                }
                //Message = $"View data {id} at {DateTime.UtcNow.ToLongDateString()}";
                //_logger.LogInformation(Message);
                return Ok(course); // Return the contact with the given id
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while retrieving the contact: " + e);
                return StatusCode(500);
            }
        }

        // POST: /Contacts
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Courses.Add(course); // Add contact
                    _context.SaveChanges(); // Save the changes

                    //Message = $"Create data {DateTime.UtcNow.ToLongDateString()}";
                    //_logger.LogInformation(Message);

                    return CreatedAtAction(nameof(GetById), new { id = course.Id }, course); // Notification
                }
                return BadRequest(ModelState);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while creating the contact: " + e);
                return StatusCode(500);
            }
        }

        // PUT: /Contacts/{id}
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Update(Course updatedCourse)
        {
            try
            {
                var existingCourse = _context.Courses.FirstOrDefault(c => c.Id == updatedCourse.Id); // get the first contact with the given id
                if (existingCourse == null)
                {
                    return NotFound(); // Return 404 if not found
                }
                // Update exist Contact with the given updatedContact
                existingCourse.Code = updatedCourse.Code;
                existingCourse.Price = updatedCourse.Price;
                existingCourse.Decription = updatedCourse.Decription;
                //existingCourse.Enrollments = updatedCourse.Enrollments;

                _context.SaveChanges(); // Save change
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while updating the contact: " + e);
                return StatusCode(500);
            }
        }

        // DELETE: /Contacts/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                var courseToDelete = _context.Courses.FirstOrDefault(c => c.Id == id); // get the first contact with the given id
                if (courseToDelete == null)
                {
                    return NotFound(); // Return 404 if not found
                }

                _context.Courses.Remove(courseToDelete); // Delete the contact
                _context.SaveChanges(); // Save Change
                return NoContent();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while deleting the course: " + e);
                return StatusCode(500);
            }
        }

    }
}
