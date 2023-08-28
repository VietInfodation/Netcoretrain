using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Coursesvc.Interfaces
{
    public interface IEnrollment
    {
        IEnumerable<Enrollment> GetAll();
        Task<IActionResult> Add(Enrollment enrollment);
        Task<IActionResult> Remove(string userId, int CourseId);
        Task<IActionResult> Update(Enrollment enrollment);
    }
}
