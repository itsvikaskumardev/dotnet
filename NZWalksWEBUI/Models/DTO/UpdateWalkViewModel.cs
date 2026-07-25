using Microsoft.AspNetCore.Mvc.Rendering;

namespace NZWalksWEBUI.Models.DTO
{
    public class UpdateWalkViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double LengthInKm { get; set; }
        public string? WalkImageUrl { get; set; }
        public Guid DifficultyId { get; set; }
        public Guid RegionId { get; set; }
        
        public IEnumerable<SelectListItem>? Regions { get; set; }
        public IEnumerable<SelectListItem>? Difficulties { get; set; }
    }
}
