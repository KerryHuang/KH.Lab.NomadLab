using Microsoft.AspNetCore.Mvc;

namespace KH.Lab.NomadLab.Controllers;

[ApiController]
[Route("api/nomad/variables")]
public class NomadController : ControllerBase
{
    private readonly NomadService _nomadService;

    public NomadController(NomadService nomadService)
    {
        _nomadService = nomadService;
    }

    // 建立 Variable
    [HttpPost]
    public async Task<IActionResult> CreateVariable(string path, [FromBody] Dictionary<string, string> data)
    {
        var success = await _nomadService.CreateVariableAsync(path, new { Items = data });
        if (!success)
        {
            return StatusCode(500, "Failed to set variable.");
        }
        return NoContent();
    }

    // 單一取得 Variable
    [HttpGet("{path}")]
    public async Task<IActionResult> GetVariable(string path)
    {
        var variable = await _nomadService.GetVariableAsync(path);
        if (variable == null) return NotFound();
        return Ok(variable);
    }

    // 取得全部 Variables
    [HttpGet]
    public async Task<IActionResult> GetAllVariables()
    {
        var variables = await _nomadService.GetAllVariablesAsync();
        return Ok(variables);
    }

    // 修改 Variable
    [HttpPut("{path}")]
    public async Task<IActionResult> UpdateVariable(string path, [FromBody] Dictionary<string, string> data)
    {
        await _nomadService.UpdateVariableAsync(path, data);
        return Ok();
    }

    // 刪除 Variable
    [HttpDelete("{path}")]
    public async Task<IActionResult> DeleteVariable(string path)
    {
        await _nomadService.DeleteVariableAsync(path);
        return Ok();
    }
}
