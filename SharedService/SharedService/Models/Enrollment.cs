using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Coursesvc.Models;

public partial class Enrollment
{
    [BindNever]
    public int Id { get; set; }

    public int CouresId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime EnrolledDate { get; set; }

    public virtual Course Coures { get; set; } = null!;
}
