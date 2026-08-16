namespace EntityFrameworkCore.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        /*
        public int DepartmentId { get; set; }

        public Department Department { get; set; } 
        
        This navigation property means:
         
         This student belongs to one Department.
        That's why it's a normal object:

         */

        // One Student → One Department
        public Department Department { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    }
}

/*
2. But what about Department?

A department can have many students.

So you need: public ICollection<Student> Students { get; set; }

This means: This department has a collection of Students
 
 */
