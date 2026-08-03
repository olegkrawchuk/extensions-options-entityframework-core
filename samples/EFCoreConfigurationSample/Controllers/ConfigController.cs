using EFCoreConfigurationSample.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EFCoreConfigurationSample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConfigController : ControllerBase
{
    private readonly IdentificationOptions _identOptions;

    public ConfigController(IOptionsSnapshot<IdentificationOptions> options)
    {
        _identOptions = options.Value;
    }

    [HttpGet("ident")]
    public ActionResult GetIdentOptions() => Ok(_identOptions);
}
