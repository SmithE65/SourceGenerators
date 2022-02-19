using MapFrom;

namespace SG3.Mapping.Models;

[MapFrom(typeof(MyModel))]
public partial class MyViewModel
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public int RandomNumber { get; set; }
}
