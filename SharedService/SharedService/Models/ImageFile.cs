using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SharedService.CustomAtribute;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedService.Models
{
    public partial class ImageFile
    {
        [BindNever]
        public int Id { get; set; }

        public string? ImageLink { get; set; }

        [MaxFileSize(1 * 1024 * 1024)]
        [PermittedExtensions(new string[] { ",jpq", ".png", ".gif", ".jpeg", ".xlsx", ".xls",".csv" })]
        [NotMapped]
        public virtual IFormFile ImageUrl { get; set; }

        public string? ImageStorageName { get; set; }
    }
}
