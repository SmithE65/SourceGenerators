namespace SG3.Mapping.Models;

public class MyModel
{
    public User? User { get; set; }

    public int RandomNumber { get; set; }
}

public record class User(int Id, string Username);