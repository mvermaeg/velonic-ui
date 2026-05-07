using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.Clients;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public ClientsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetClients(
            [FromQuery] string? search,
            [FromQuery] string? tier,
            [FromQuery] string? accountStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.Clients
                .Where(x => x.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.ClientName.Contains(search) ||
                    (x.Timezone != null && x.Timezone.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(tier))
            {
                query = query.Where(x => x.Tier == tier);
            }

            if (!string.IsNullOrWhiteSpace(accountStatus))
            {
                query = query.Where(x => x.AccountStatus == accountStatus);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClientListDto
                {
                    Id = x.Id,
                    ClientName = x.ClientName,
                    Tier = x.Tier,
                    AccountStatus = x.AccountStatus,
                    Timezone = x.Timezone,
                    AcceptsWebLeads = x.AcceptsWebLeads,
                    AcceptsInboundCalls = x.AcceptsInboundCalls,
                    IsBuyer = x.IsBuyer,
                    IsVendor = x.IsVendor,
                    AccountManagerUserId = x.AccountManagerUserId,
                    SubAccountManagerUserId = x.SubAccountManagerUserId,
                    ReturnAgreement = x.ReturnAgreement,
                    CreatedOn = x.CreatedOn,
                    UpdatedOn = x.UpdatedOn
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(long id)
        {
            var client = await _db.Clients
                .Where(x => x.Id == id && x.IsDeleted == false)
                .FirstOrDefaultAsync();

            if (client == null)
                return NotFound(new { message = "Client not found." });

            return Ok(client);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient(CreateClientDto model)
        {
            if (string.IsNullOrWhiteSpace(model.ClientName))
                return BadRequest(new { message = "Client name is required." });

            var client = new Client
            {
                ClientName = model.ClientName,
                Tier = model.Tier,
                AccountStatus = model.AccountStatus,
                Timezone = model.Timezone,
                AcceptsWebLeads = model.AcceptsWebLeads,
                AcceptsInboundCalls = model.AcceptsInboundCalls,
                IsBuyer = model.IsBuyer,
                IsVendor = model.IsVendor,
                AccountManagerUserId = model.AccountManagerUserId,
                SubAccountManagerUserId = model.SubAccountManagerUserId,
                ReturnAgreement = model.ReturnAgreement,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Client created successfully.",
                id = client.Id
            });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] string status)
        {
            var client = await _db.Clients.FindAsync(id);

            if (client == null || client.IsDeleted == true)
                return NotFound(new { message = "Client not found." });

            client.AccountStatus = status;
            client.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Client status updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(long id)
        {
            var client = await _db.Clients.FindAsync(id);

            if (client == null || client.IsDeleted == true)
                return NotFound(new { message = "Client not found." });

            client.IsDeleted = true;
            client.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Client archived successfully." });
        }
    }
}