using Microsoft.AspNetCore.Mvc;
using SG4.Boilerplate.Data.Models;

namespace SG4.Boilerplate.Controllers;

public partial class AddressController
{
    [HttpGet]
    public IActionResult GetFive()
    {
        return Ok(_repo.GetFive());
    }
}
