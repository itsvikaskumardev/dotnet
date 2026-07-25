using Microsoft.AspNetCore.Mvc;

namespace NZWalksWEBUI.Controllers
{
    public class RegionsController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        public RegionsController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get all regions from Web API
                var client = httpClientFactory.CreateClient();

                var httpResponseMessage = await client.GetAsync("https://localhost:7000/api/regions");

                httpResponseMessage.EnsureSuccessStatusCode();

                var response = await httpResponseMessage.Content.ReadAsStringAsync();

                // Deserialize the response here if needed
                // Example:
                // var regions = JsonConvert.DeserializeObject<List<RegionDto>>(response);

                return View();
                // return View(regions);
            }
            catch (Exception ex)
            {
                // Log the exception if required
                return View("Error");
            }
        }
    }
}