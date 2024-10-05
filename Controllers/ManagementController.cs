using I72_Backend.Entities;
using I72_Backend.Entities.Enums;
using I72_Backend.Exceptions;
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
    [Authorize(Roles = "Admin,Staff")]
    public ActionResult GetData([FromQuery] String table, [FromQuery] PaginationParams pageable ,[FromBody] Dictionary<String, String?> conditions)
    {
        _logger.Log(LogLevel.Information, "Received request to create tables");
        var response = new ResponseRestDto();
        try
        {
            var queryRes = _managementService.PerformRead(table, pageable, conditions);
            response.Data = queryRes.Rows;
            response.Page = queryRes.Page;
            response.PageSize = queryRes.PageSize;
            response.TotalPage = queryRes.TotalPage;
            return Ok(response);
        }
        catch (Exception e)
        {
            response.Message = e.Message;
            response.Data = e.Data;
            return BadRequest(response);
        }
    }
    /* [HttpGet("/pie-chart")] */
    /* [HttpGet("/pie-chart")] */
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public ActionResult InsertData([FromQuery] String table, [FromBody] Dictionary<String, String?> values)
    {
        _logger.Log(LogLevel.Information, "Received request to Insert Data");
        var response = new ResponseRestDto();
        try
        {
            var queryRes = _managementService.PerformInsert(table, values);
            response.Message = queryRes;
            return Ok(response);
        }
        catch (AppSqlException e)
        {
            response.Message = e.Message;
            response.Data = e.Data;
            return BadRequest(response);
        }
    }
    
    [HttpDelete("{id}")]
    [AllowAnonymous]
    public JsonResult DeleteData([FromQuery] String table, String column, String id)
    {
        _logger.Log(LogLevel.Information, "Received request to Delete Data");
        var queryRes = _managementService.PerformDeleteById(table, column, id);
        var response = new ResponseRestDto();
        response.Message = queryRes;
        return new JsonResult(Ok(response));
    }

    [HttpDelete("batch")]
    [Authorize(Roles = "Admin,Staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult BatchDeleteData([FromQuery] String table, [FromBody] Dictionary<String, String?> conditions)
    {
        _logger.Log(LogLevel.Information, "Received request to Delete Data");
        var response = new ResponseRestDto();
        try
        {
            var queryRes = _managementService.PerformBatchDelete(table, conditions);
            response.Message = queryRes;
            return Ok(response);
        }
        catch (AppSqlException e)
        {
            response.Message = e.Message;
            response.Data = e.Data;
            return BadRequest(response);
        }
    }
    
    [HttpPut("batch")]
    [AllowAnonymous]
    public JsonResult UpdateData([FromQuery] String table, [FromBody] UpdateRequestDto updateRequest)
    {
        _logger.Log(LogLevel.Information, "Received request to Update Data");
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