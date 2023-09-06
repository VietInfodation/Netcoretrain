using Coursesvc.Interfaces;
using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Coursesvc.Controllers
{
    [Route("enrollment")]
    public class EnrollmentsController : Controller
    {
        private readonly IEnrollment _context;
        private readonly ILogger _logger;
        public EnrollmentsController(IEnrollment context, ILogger<EnrollmentsController> logger)
        {
            _context = context;
            _logger = logger;
            Message = "";
        }
        public string Message { get; set; }

        // Get all enrollment
        [HttpGet]
        public IActionResult GetAll()
        {
            var enrollments = _context.GetAll();
            if (enrollments == null)
            {
                return BadRequest(new Enrollment[0]); // return null
            }
            return Ok(_context.GetAll());
        }

        // Add enrollment
        [HttpPost]
        [Route("add-enrollment")]
        public async Task<IActionResult> AddEnrollment([FromBody] Enrollment enrollment)
        {
            return await _context.Add(enrollment);
        }

        //remove enrollment
        [HttpPut]
        [Route("remove-enrollment")]
        public async Task<IActionResult> RemoveEnrollment(string userId, int CourseId)
        {
            return await _context.Remove(userId, CourseId);
        }


    }
}
