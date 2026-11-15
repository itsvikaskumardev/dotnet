namespace EntityFrameworkCore.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        public decimal Marks { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
