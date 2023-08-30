using Coursesvc.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Coursesvc.Controllers
{
    [Route("cronjobs")]
    [ApiController]
    public class CronJobsController : Controller
    {
        private readonly ICronJobs _cronJob;
        public CronJobsController(ICronJobs cronJobs)
        {
            _cronJob = cronJobs;
        }

        //Api to create a Cron job
        [HttpGet]
        [Route("recursive")]
        public IActionResult DailyEport()
        {
            var job = _cronJob.DailyExport(); //Cron job service
            return job;
        }


    }
}
