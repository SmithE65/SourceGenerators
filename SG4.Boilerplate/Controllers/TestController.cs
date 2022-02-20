using SG4.Boilerplate.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SG4.Boilerplate.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IProductRepository _repo;
    public TestController(IProductRepository productRepo)
    {
        _repo = productRepo;
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok(_repo.Find(id));
    }
}
