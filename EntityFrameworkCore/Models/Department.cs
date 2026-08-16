namespace EntityFrameworkCore.Models
{
    public class Department
    // 1. Department → 2. Teacher → 3. Course → 4. Student → 5. Enrollment
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // One Department → Many Students
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}

/*
 
Why ICollection specifically?

We could write: public List<Student> Students { get; set; }

but EF Core navigation properties are commonly defined as:

public ICollection<Student> Students { get; set; }
    = new List<Student>();

because ICollection<T> describes the capability you need (a collection of related entities) without tying your model to a specific collection implementation.

 */


/*

A navigation property is the general concept.

Both of these are navigation properties:

public Department Department { get; set; }

and:

public ICollection<Student> Students { get; set; }

The difference is the relationship cardinality:

1) One-to-one
Person → Passport
     ↓
Passport

-------------
2) One-to-many
Department → Students
          ↓
     ICollection<Student>
---------------

3) Many-to-one
Student → Department
       ↓
   Department
--------


4) 

Many-to-many
Student → Courses
       ↕
  ICollection<Course>

So don't think:

"Navigation property OR ICollection"

Think:

ICollection<T> is one type of navigation property used when the relationship contains multiple related entities. 
 
*/
