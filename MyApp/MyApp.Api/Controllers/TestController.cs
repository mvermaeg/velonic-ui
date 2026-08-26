using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using System.Data;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/test")]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public TestController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet("database")]
        public async Task<IActionResult> Database()
        {
            var connection = _db.Database.GetDbConnection();

            try
            {
                var serverBeforeOpen = connection.DataSource;
                var databaseBeforeOpen = connection.Database;

                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                return Ok(new
                {
                    success = true,
                    server = connection.DataSource,
                    database = connection.Database,
                    connectionState = connection.State.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,

                    // MOST IMPORTANT INFORMATION
                    serverBeingUsed = connection.DataSource,
                    databaseBeingUsed = connection.Database,

                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message,
                    baseMessage = ex.GetBaseException().Message
                });
            }
        }
    }
}