namespace Example1;

public class Employee
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DateTime CreationTs { get; set; }
    public Employee(string id, string name, int age)
    {
        Id = id;
        Name = name;
        Age = age;
        CreationTs = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"Id: {Id} Name: {Name} Age: {Age} Create Time: {CreationTs}";
    }
}