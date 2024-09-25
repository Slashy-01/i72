using I72_Backend.Entities;
using I72_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace I72_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManagementController : ControllerBase
{
    private readonly ILogger<ManagementController> _logger;
    private IManagementService _managementService;
    private IManagementRepository _repository;
    public ManagementController(ILogger<ManagementController> logger, IManagementService managementService, IManagementRepository repository)
    {
        _logger = logger;
        _managementService = managementService;
        _repository = repository;
    }
    [HttpPost("create-tables")]
    [AllowAnonymous]
    public JsonResult CreateTables([FromBody] CreateListTablesDto dto)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        _managementService.CreateTables(dto);
        return new JsonResult(Created());
    }
    
    [HttpGet]
    [AllowAnonymous]
    public JsonResult Something()
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        var res = _repository.QuerySomething();
        return new JsonResult(Created());
    }
}
