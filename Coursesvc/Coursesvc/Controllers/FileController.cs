using Coursesvc.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Coursesvc.Controllers
{
    [Route("image")]
    [ApiController]

    public class FileController : Controller
    {
        private readonly IFile _FileService;
        //private readonly IUnitofWorks _unitofWorks;
        public FileController(IFile imageFileService/*, IUnitofWorks unitofWorks*/)
        {
            _FileService = imageFileService;
            ////_unitofWorks = unitofWorks;
        }


        [Route("/")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] SharedService.Models.File file)
        {
            return await _FileService.Add(file);
        }

        [Route("/{id}")]
        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            return await _FileService.Remove(id);
        }
        [Route("/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            return await _FileService.GetFile(id);
        }

        [Route("/csv")]
        [HttpGet]
        public async Task<IActionResult> ImportCsv(string fileName)
        {
            return await _FileService.ImportCsv(fileName);
        }

        [Route("/export")]
        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            await _FileService.WriteCSV();       
            return Ok();
        }
    }
}
