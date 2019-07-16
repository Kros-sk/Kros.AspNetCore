using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kros.AspNetCore
{
    /// <summary>
    /// A base class for all api controllers.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public abstract class ApiBaseController : ControllerBase
    {
    }
}
