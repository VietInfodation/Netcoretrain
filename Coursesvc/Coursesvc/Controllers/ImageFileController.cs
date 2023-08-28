using Coursesvc.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedService.Interfaces;
using SharedService.Models;

namespace Coursesvc.Controllers
{
    [Route("image")]
    [ApiController]

    public class ImageFileController : Controller
    {
        private readonly IImageFile _imageFileService;
        private readonly IUnitofWorks _unitofWorks;
        public ImageFileController(IImageFile imageFileService, IUnitofWorks unitofWorks)
        {
            _imageFileService = imageFileService;
            _unitofWorks = unitofWorks;
        }


        [Route("/")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] ImageFile file)
        {

            return await _imageFileService.Add(file); 
        }

        [Route("/{id}")]
        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {

            return await _imageFileService.Remove(id); 
        }
        [Route("/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {

            return await _imageFileService.GetFile(id); 
        }

        [Route("/csv")]
        [HttpGet]
        public async Task<IActionResult> ImportCsv(string fileName)
        {

            return await _imageFileService.ImportCsv(fileName);
        }

        [Route("/export")]
        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {

            return await _imageFileService.WriteEmployeeCSV();
        }
    }
}
