namespace EntityFrameworkCore.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
