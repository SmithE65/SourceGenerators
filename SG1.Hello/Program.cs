
namespace SG1.Hello;

public static partial class Program
{
    static void Main()
    {
        HelloFrom("Source Generators");
    }

    static partial void HelloFrom(string name);
}