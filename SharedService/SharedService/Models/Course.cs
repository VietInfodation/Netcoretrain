using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Coursesvc.Models;

public partial class Course
{
    [BindNever]
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    [Required]
    public float? Price { get; set; }

    public string? Decription { get; set; }

    //[BindNever]
    //public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
