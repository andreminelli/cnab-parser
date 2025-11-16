using CnabParser.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace CnabParser.Api.Controllers;

[ApiController]
[Route("cnab-files")]
public class CnabFileController : ControllerBase
{
    private readonly ICnabImporterService _cnabImporterService;

    public CnabFileController(ICnabImporterService cnabImporterService)
    {
        _cnabImporterService = cnabImporterService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointDescription("Upload a CNAB file")]
    public async Task<ActionResult<ImportResponse>> UploadCnabFileAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required and cannot be empty");
        }

        await using var fileStream = file.OpenReadStream();
        var result = await _cnabImporterService.ImportAsync(fileStream, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{importId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointDescription("Get stores and transactions from a previously imported file")]
    public async Task<ActionResult<ImportDataResponse>> GetCnabFileDataAsync(
        [Description("Value from property \"importId\", returned from a success upload")] string importId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(importId, out var dataSourceId))
        {
            return BadRequest($"Invalid importId value: {importId}");
        }

        var result = await _cnabImporterService.GetImportDataAsync(dataSourceId, cancellationToken);
        return Ok(result);
    }
}
