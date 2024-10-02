using I72_Backend.Entities;
using I72_Backend.Entities.Enums;
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
    private readonly IManagementService _managementService;
    public ManagementController(ILogger<ManagementController> logger, IManagementService managementService)
    {
        _logger = logger;
        _managementService = managementService;
    }
    [HttpPost("create-tables")]
    [AllowAnonymous]
    public JsonResult CreateTables([FromBody] CreateListTablesDto dto)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        _managementService.CreateTables(dto);
        return new JsonResult(Created());
    }
    
    [HttpPost("get-data")]
    [AllowAnonymous]
    public JsonResult GetData([FromQuery] String table, [FromQuery] PaginationParams pageable ,[FromBody] Dictionary<String, String?> conditions)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        var queryRes = _managementService.PerformRead(table, pageable, conditions);
        var response = new ResponseRestDto();
        response.Message = "Retrieved data";
        response.Data = queryRes;
        return new JsonResult(Ok(response));
    }
    /* [HttpGet("/pie-chart")] */
    /* [HttpGet("/pie-chart")] */
    [HttpPost]
    [AllowAnonymous]
    public JsonResult InsertData([FromQuery] String table, [FromBody] Dictionary<String, String?> values)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        var queryRes = _managementService.PerformInsert(table, values);
        var response = new ResponseRestDto();
        response.Message = queryRes;
        return new JsonResult(Ok(response));
    }
    
    [HttpDelete("{id}")]
    [AllowAnonymous]
    public JsonResult DeleteData([FromQuery] String table, String column, String id)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        var queryRes = _managementService.PerformDeleteById(table, column, id);
        var response = new ResponseRestDto();
        response.Message = queryRes;
        return new JsonResult(Ok(response));
    }
    
    [HttpPut("batch")]
    [AllowAnonymous]
    public JsonResult UpdateData([FromQuery] String table, [FromBody] UpdateRequestDto updateRequest)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        var queryRes = _managementService.PerformBatchUpdate(table, updateRequest.Where, updateRequest.UpdatedField);
        var response = new ResponseRestDto();
        response.Message = queryRes;
        return new JsonResult(Ok(response));
    }
    
    // [HttpGet("aggregate-chart")]
    // [AllowAnonymous]
    // public JsonResult GetAggregateChartData([FromQuery] String table, [FromQuery] String x, [FromQuery] String y, [FromQuery] AggregationType aggregationType)
    // {
    //     _logger.Log(LogLevel.Information, "Received request to get bar-chart");
    //     var queryRes = _managementService.GetBarChartData(table, x, y, aggregationType);
    //     var response = new ResponseRestDto();
    //     response.Data = response;
    //     return new JsonResult(Ok(response));
    // }

    [HttpGet("aggregate-chart")]
    [AllowAnonymous]
    public IActionResult GetAggregateChartData([FromQuery] string table, [FromQuery] string x, [FromQuery] string y, [FromQuery] AggregationType aggregationType)
    {
        _logger.Log(LogLevel.Information, "Received request to get bar-chart");

        // Call your service to get data
        var queryRes = _managementService.GetBarChartData(table, x, y, aggregationType);

        // Construct a response DTO
        var response = new ResponseRestDto
        {
            Data = queryRes // Assign actual data here
        };

        // Return the response
        return Ok(response); // Directly return Ok with response
    }

}