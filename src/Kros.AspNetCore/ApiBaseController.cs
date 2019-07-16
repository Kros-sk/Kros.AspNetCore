using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kros.AspNetCore
{
    /// <summary>
    /// A base class for all api controllers.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("/api/[controller]")]
    public abstract class ApiBaseController : ControllerBase
    {
    }
}
