using Hangfire.Server;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Coursesvc.Interfaces;
using Coursesvc.Controllers;

namespace Coursesvc.Services
{
    public class CronService : ICronJobs
    {
        private readonly ILogger _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IFile _fileService;
        public CronService(ILogger<CronJobsController> logger, IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager, IFile fileService)
        {
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _fileService = fileService;
        }

        //One time job showing id (for testing)
        public IActionResult FireandForgetId()
        {
            var jobId = _backgroundJobClient.Enqueue(() => FireAndForgetJob(null));
            Thread.Sleep(5000);
            return new OkObjectResult($"Job Id: {jobId} completed...");
        }

        //Recurring Export csv File
        public IActionResult DailyExport()
        {
            var jobId = "DailyExportFile";
            
            //Add or Update a recurring Task
            _recurringJobManager.AddOrUpdate(jobId, () => ExportFileRecurringJob(jobId),
            Cron.Minutely);
            Thread.Sleep(5000);
            return new OkObjectResult($"Executing Job Id: {jobId}...{DateTime.Now}");
        }
        //Recurring Task
        public Task ExportFileRecurringJob(string job_id)
        {
            try 
            {
                _logger.LogInformation($"Executing Job Id: {job_id} at {DateTime.Now}");
                _fileService.WriteEmployeeCSV(); //Export file service
                return Task.CompletedTask;
            } catch (Exception ex) 
            {
                return Task.FromException(ex);
            }
        }

        public Task FireAndForgetJob(PerformContext context)
        {
            var jobId = context.BackgroundJob.Id;
            _logger.LogInformation($"Executing Job Id: {jobId}...");
            return Task.CompletedTask;
        }
    }
}
