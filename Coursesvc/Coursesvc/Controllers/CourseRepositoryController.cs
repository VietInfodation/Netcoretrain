using Coursesvc.Interfaces;
using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Coursesvc.Controllers
{
    [Route("course-repository")]
    public class CourseRepositoryController : Controller
    {
        private readonly ICourseService _courseService; // put it in UoW ???
        public CourseRepositoryController(ICourseService courseService)
        {
            _courseService = courseService;
        }
        //Controller Show all Course
        [HttpGet]
        public IActionResult GetAll()
        {
            var courses = _courseService.GetAll();
            if (courses == null)
            {
                return BadRequest(new Course[0]); // return null
            }
            return Ok(_courseService.GetAll());


        }

        //Find by ID
        //[Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var course = _courseService.GetById(id); // Find the id 
            if (course == null)
            {
                return NotFound(); // Return 404 if not found
            }
            return Ok(course); // Return the course with the given id
        }
        // Create new Course
        // POST: /Contacts
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _courseService.Add(course); //Add the course

                //Message = $"Create data {DateTime.UtcNow.ToLongDateString()}";
                //_logger.LogInformation(Message);

                return CreatedAtAction(nameof(GetById), new { id = course.Id }, course); // Notification
            }
            return BadRequest(ModelState);

        }
        //Update Specific Course
        //[Authorize(Roles = "Admin")]
        [HttpPut]
        public IActionResult Update(int id, Course course)
        {
            return _courseService.Update(id, course);
        }
        //Delete a course
        // DELETE: /Contacts/{id}
        [HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            //var status = _courseService.Remove(id); //remove the course with the id
            return _courseService.Remove(id);
        }

        //Add range 
        [HttpPost("range")]
        public IActionResult AddRange([FromBody] Course[] courses)
        {
            return _courseService.AddRange(courses);
        }

        //Remove range 
        [HttpDelete("range")]
        public IActionResult RemoveRange([FromBody] int[] id)
        {
            //remove the course with the id
            return _courseService.RemoveRange(id);
        }
        //Update range ***********Outdated
        [HttpPatch("range")]
        public IActionResult UpdateRange([FromBody] Course[] courses)
        {
            //Update the course with the id from the list
            return _courseService.UpdateRange(courses);
        }

        /// <summary>
        /// Update range  with specific column 
        /// </summary>
        /// <param name="courses">the request body</param>
        /// <param name="column">columns to update</param>
        /// <returns></returns>
        [HttpPatch("range-rawsql")]
        public IActionResult UpdateRangeSQL([FromBody] Course[] courses, string[] column)
        {
            //Update the Course using raw SQL
            return _courseService.UpdateRangewithSQL(courses, column);
        }
        [HttpPatch("range-rawsplitsql")]
        public IActionResult UpdateRangeSplitSQL([FromBody] Course[] courses, [Required] string[] column, [Required] int row)
        {
            //Update the Course using raw SQL
            return _courseService.UpdateRangewithSplitSQL(courses, column, row);
        }
    }
}
