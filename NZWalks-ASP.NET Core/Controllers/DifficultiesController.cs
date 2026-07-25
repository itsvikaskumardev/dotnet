using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NZWalks_ASP.NET_Core.Data;
using NZWalks_ASP.NET_Core.Models.DTO;

namespace NZWalks_ASP.NET_Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DifficultiesController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;

        public DifficultiesController(NZWalksDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var difficultiesDomain = await dbContext.Difficulties.ToListAsync();
            
            var difficultiesDto = new List<DifficultyDto>();
            foreach (var difficulty in difficultiesDomain)
            {
                difficultiesDto.Add(new DifficultyDto
                {
                    Id = difficulty.Id,
                    Name = difficulty.Name
                });
            }

            return Ok(difficultiesDto);
        }
    }
}
