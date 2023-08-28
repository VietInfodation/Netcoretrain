using Coursesvc.Interfaces;
using Coursesvc.Models;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SharedService.Interfaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace Coursesvc.Services
{
    //Using basic CRUD from repository
    public class CourseService : ICourseService
    {
        private readonly IUnitofWorks _unitofWorks;
        private readonly IGenericRepository<Course> _context;
        private readonly CourseContext _coursecontext;
        private readonly ILogger _logger;

        public CourseService(IUnitofWorks unitofWorks, CourseContext coursecontext, ILogger<CourseService> logger)
        {
            _unitofWorks = unitofWorks;
            _context = _unitofWorks.GetRepository<Course>();
            _coursecontext = coursecontext;
            _logger = logger;
        }

        public void Add(Course course)
        {
            //if(course.Price == null) { course.Price = 0; };
            _context.Add(course); // ~ _unitofWorks.GetRepository<Course>().Add(course)
            _unitofWorks.Commit();
        }
        public IQueryable<Course> Find(Expression<Func<Course, bool>> expression)
        {
            return _context.Find(expression); //~ _unitofWorks.GetRepository<Course>().Find(expression)
        }
        public IQueryable<Course> GetAll()
        {
            _logger.LogInformation("Course GET ALL - this is a nice message to test the logs", DateTime.UtcNow);
            return _context.GetAll(); //~ _unitofWorks.GetRepository<Course>().GetAll()
        }
        public Course GetById(int id)
        {
            return _context.GetById(id); //~ _unitofWorks.GetRepository<Course>().GetById(id)
        }

        //Remove Course
        public IActionResult Remove(int id)
        {
            try
            {
                var coursetoremove = _context.GetById(id); // get the course to remove 
                if (coursetoremove != null)
                {
                    _context.Remove(coursetoremove); // remove the course using method from repository
                    _unitofWorks.Commit(); // commit the change
                    return new OkResult();
                }
                return new NotFoundResult(); // 404 if not found
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while deleting the contact: " + ex);
                return new BadRequestResult(); // 500 if Exception
            }

        }
        //Update course
        public IActionResult Update(int id, Course course)
        {
            try
            {
                var coursetoupdate = _context.GetById(id); // Get course to update
                if (coursetoupdate == null)
                {
                    return new NotFoundResult(); // 404 if not found
                }
                // Update the course
                coursetoupdate.Code = course.Code ?? coursetoupdate.Code;
                coursetoupdate.Decription = course.Decription ?? coursetoupdate.Decription;
                coursetoupdate.Price = course.Price ?? 0;
                _context.Update(coursetoupdate); // update the course using method from repository
                _unitofWorks.Commit();
                return new OkResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while update the contact: " + ex);
                return new BadRequestResult(); // 500 if Exception
            }
        }
        //Get Id by Code

        //Remove multi course
        public IActionResult RemoveRange(int[] id)
        {
            try
            {
                var itemsToRemove = _context.Find(item => id.Contains(item.Id)).ToList();
                _context.RemoveRange(itemsToRemove);
                _unitofWorks.Commit();
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"{ex}");
            }

        }

        //Add multi course
        public IActionResult AddRange(Course[] courses)
        {
            try
            {
                _context.AddRange(courses);
                _unitofWorks.Commit();
                return new OkResult();
            }
            catch
            {
                return new BadRequestResult();
            }

        }
        // UpdateRange for specific Column
        public IActionResult UpdateRange(Course[] courses)
        {
            try
            {
                List<Course> coursetoupdate = new List<Course>();
                for (int i = 0; i < courses.Length; i++)
                {
                    // _logger.LogInformation("Reaching the checking method");

                    // _logger.LogInformation("Pass the checking method" + courses[i].Id);
                    var existingcourse = _context.GetById(courses[i].Id);

                    if (existingcourse != null)
                    {
                        // Detach the tracked entity from the original context
                        _coursecontext.Entry(existingcourse).State = EntityState.Detached;

                        coursetoupdate.Add(_context.mapUpdateOject(courses[i], existingcourse));
                    }
                    //_logger.LogInformation("Fail the checking method id" + courses[i].Id);
                }
                //_logger.LogInformation("End checking" + coursetoupdate[0].Id + coursetoupdate[1].Id);
                _context.UpdateRange(coursetoupdate);
                _coursecontext.SaveChanges();
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Something happen " + ex);
            }
        }
        /// <summary>
        /// Uppdate using raw Sql
        /// </summary>
        /// <param name="courses">Body of the request</param>
        /// <param name="columns">columns to update</param>
        /// <returns></returns>
        public IActionResult UpdateRangewithSQL(Course[] courses, string[] columns)
        {
            try
            {
                // Create a dictionary to store the courses
                Dictionary<int, Dictionary<string, object>> courseDictionary = new Dictionary<int, Dictionary<string, object>>();

                // Iterate through the courses and add them to the dictionary.
                foreach (Course course in courses)
                {
                    Dictionary<string, object> courseData = new Dictionary<string, object>();

                    // Loop through all the columns and add them to the data dictionary.
                    foreach (PropertyInfo propertyInfo in course.GetType().GetProperties())
                    {
                        if (propertyInfo.Name.ToLower() == "id" || propertyInfo.GetValue(course) == null)
                        { continue; }
                        courseData[propertyInfo.Name] = propertyInfo.GetValue(course);
                    }

                    courseDictionary[course.Id] = courseData;
                }
                // Group courses dictionary by property name.
                var groupedCourses = courseDictionary
                    .SelectMany(course => course.Value.Select(property => new { course.Key, PropertyName = property.Key, PropertyValue = property.Value }))
                    .GroupBy(course => course.PropertyName);

                // Create a string builder to store the query.
                StringBuilder queryBuilder = new StringBuilder();

                // Start the UPDATE statement.
                queryBuilder.AppendLine("UPDATE courses");
                queryBuilder.AppendLine("SET");

                // Generate CASE statements for each property group.
                foreach (var group in groupedCourses)
                {
                    var caseStatement = "";
                    if (columns.Contains(group.Key.ToLower())) // Update only the column that require
                    {
                        caseStatement = $"    {group.Key} = CASE";

                        foreach (var course in group)
                        {
                            caseStatement += $" WHEN id = {course.Key} THEN '{(course.PropertyValue)}'";
                        }
                    }
                    else continue;


                    caseStatement += $" ELSE {group.Key} END,";

                    // Add to the query builder.
                    queryBuilder.AppendLine(caseStatement);
                }

                // Remove the trailing comma and newline.
                queryBuilder.Length -= 3;

                // Add the WHERE clause with the list of IDs.
                var ids = string.Join(",", courseDictionary.Keys);
                queryBuilder.AppendLine($" WHERE courses.Id IN ({ids});");


                var parameters = new List<MySqlParameter>();
                // Iterate through each group of properties (columns) to be updated
                foreach (var group in groupedCourses)
                {
                    // Iterate through each course (row) within the current group
                    foreach (var course in group)
                    {
                        // Create a new SqlParameter object for the current property value
                        // Construct the parameter name using format Value_ColumnName_RowId
                        var parameter = new MySqlParameter($"Value_{course.PropertyName}_{course.Key}", course.PropertyValue ?? DBNull.Value);
                        // Add the SqlParameter object to the list of parameters
                        parameters.Add(parameter);
                    }
                }

                _unitofWorks.BeginTransaction();

                //_coursecontext.Database.ExecuteSqlRaw(queryBuilder.ToString(), parameters); // Run the SQL query

                _coursecontext.SaveChanges();

                _coursecontext.Database.CommitTransaction();
                return new OkObjectResult(queryBuilder.ToString()); //Return the sql Query string (for context)
            }
            catch (Exception ex)
            {
                _unitofWorks.Rollback();
                return new BadRequestObjectResult("Something happen: " + ex);
            }
        }

        public IActionResult UpdateRangewithSplitSQL(Course[] courses, string[] columns, int row)
        {
            try
            {
                // Create a dictionary to store the courses
                //Dictionary<int, Dictionary<string, object>> courseDictionary = new Dictionary<int, Dictionary<string, object>>();

                var courseDictionary = courses.ToDictionary(
                    course => course.Id,
                    course => course.GetType()
                        .GetProperties()
                        .Where(propertyInfo =>
                            propertyInfo.Name.ToLower() != "id" &&
                            propertyInfo.GetValue(course) != null &&
                            columns.Contains(propertyInfo.Name.ToLower()))
                        .ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(course))
                );

                // Group courses dictionary by property name.
                var groupedCourses = courseDictionary
                    .SelectMany(course => course.Value.Select(property => new { course.Key, PropertyName = property.Key, PropertyValue = property.Value }))
                    .GroupBy(course => course.PropertyName);



                // Split groupedCourses into smaller groups based on row

                /* var smallerGroups = from group in groupedCourses
                                   let rowIndex = group.Key / row
                   group group by rowIndex into rowGroup
                   select rowGroup.GroupBy(item => item.Value.PropertyName);*/

                /*
                  smallerGroups[index] = {propertyGroups = {columnsGroup[index],columnsGroup[index+1],...}}
                  columnsGroup[index] = {groupIndex, Course(id,propertyName,value)}
                */

                var smallerGroups = groupedCourses
                    .SelectMany(group => group
                        .Select((course, index) => new { GroupIndex = index / row, Course = course }))
                    .GroupBy(item => item.GroupIndex)
                    .Select(group => group.GroupBy(item => item.Course.PropertyName))
                    .ToList();



                // Create a string builder to store the query.
                StringBuilder queryBuilder = new StringBuilder();

                foreach (var smallergroup in smallerGroups)
                {
                    // Start the UPDATE statement.
                    queryBuilder.AppendLine("UPDATE courses");
                    queryBuilder.AppendLine("SET");
                    List<string> ids = new List<string>();
                    // Generate CASE statements for each property group.
                    foreach (var group in smallergroup)
                    {
                        var caseStatement = "";

                        caseStatement = $"    {group.Key} = CASE";

                        foreach (var course in group)
                        {
                            caseStatement += $" WHEN id = {course.Course.Key} THEN '{(course.Course.PropertyValue)}'";
                            // Add the WHERE clause with the list of IDs.
                            ids.Add(course.Course.Key.ToString());
                        }



                        caseStatement += $" ELSE {group.Key} END,";

                        // Add to the query builder.
                        queryBuilder.AppendLine(caseStatement);
                        // Remove the trailing comma and newline.

                    }
                    queryBuilder.Length -= 3;
                    queryBuilder.AppendLine($" WHERE courses.Id IN ({string.Join(",", ids.Distinct())});");
                }


                // Now the queryBuilder contains the splitted UPDATE statements


                //_unitofWorks.BeginTransaction();

                //_coursecontext.Database.ExecuteSqlRaw(queryBuilder.ToString(), parameters); // Run the SQL query

                //_coursecontext.SaveChanges();

                //_coursecontext.Database.CommitTransaction();
                return new OkObjectResult(queryBuilder.ToString()); //Return the sql Query string (for context)
            }
            catch (Exception ex)
            {
                _unitofWorks.Rollback();
                return new BadRequestObjectResult("Something happen: " + ex);
            }
        }
    }
}
