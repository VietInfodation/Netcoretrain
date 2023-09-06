using Microsoft.AspNetCore.Mvc;
namespace Coursesvc.Interfaces
{
    public interface IFile
    {
        Task<IActionResult> Add(SharedService.Models.File imageFile);
        Task<IActionResult> Remove(int id);
        Task<IActionResult> GetFile(int id);
        Task<IActionResult> GetList(string fileName);
        Task<IActionResult> ImportCsv(string fileName);
        Task<IActionResult> WriteCSV();
    }
}
