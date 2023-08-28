using Microsoft.AspNetCore.Mvc;
using SharedService.Models;
namespace Coursesvc.Interfaces
{
    public interface IImageFile
    {
        Task<IActionResult> Add(ImageFile imageFile);
        Task<IActionResult> Remove(int id);
        Task<IActionResult> GetFile(int id);
        Task<IActionResult> GetList(string fileName);
        Task<IActionResult> ImportCsv(string fileName);
        Task<IActionResult> WriteEmployeeCSV();
    }
}
