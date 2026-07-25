using Web_API_Versoning_API.Models.Domain;

namespace Web_API_Versoning_API
{
    public static class CountriesData
    {
        public static List<Country> Get()
        {
            var countries = new[]
            {
                new { Id = 1, Name = "United States" },
                new { Id = 2, Name = "Canada" },
                new { Id = 3, Name = "United Kingdom" },
                new { Id = 4, Name = "India" },
                new { Id = 5, Name = "Australia" },
                new { Id = 6, Name = "Germany" },
                new { Id = 7, Name = "France" },
                new { Id = 8, Name = "Japan" },
                new { Id = 9, Name = "Brazil" },
                new { Id = 10, Name = "South Africa" }
            };

            return countries
                .Select(c => new Country
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();
        }
    }
}