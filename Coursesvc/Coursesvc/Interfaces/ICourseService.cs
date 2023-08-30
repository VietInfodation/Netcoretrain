using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Coursesvc.Interfaces
{
    public interface ICourseService
    {
        void Add(Course course);
        IQueryable<Course> Find(Expression<Func<Course, bool>> expression);
        IQueryable<Course> GetAll();
        Course GetById(int id);
        IActionResult Remove(int id);
        IActionResult Update(int id, Course course);
        IActionResult RemoveRange(int[] id);
        IActionResult AddRange(Course [] entities);
        IActionResult UpdateRange(Course [] entities);
        IActionResult UpdateRangeWithSQL(Course[] entities, string[]columns);
        IActionResult UpdateRangeWithSplitSQL(Course[] entities, string[]columns,int row);
    }
}
