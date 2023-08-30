using Microsoft.AspNetCore.Mvc;

namespace Coursesvc.Interfaces
{
    public interface ICronJobs
    {
        IActionResult DailyExport();
        IActionResult FireandForgetId();
    }
}
