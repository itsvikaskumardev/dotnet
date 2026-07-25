using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NZWalksWEBUI.Models.DTO;
using System.Text;
using System.Text.Json;

namespace NZWalksWEBUI.Controllers
{
    public class WalksController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        public WalksController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            List<WalkDto> response = new List<WalkDto>();
            try
            {
                var client = httpClientFactory.CreateClient();
                var httpResponseMessage = await client.GetAsync("https://localhost:7000/api/walks");

                httpResponseMessage.EnsureSuccessStatusCode();
                var stringResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                response = JsonSerializer.Deserialize<List<WalkDto>>(stringResponseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new AddWalkViewModel();
            model.Regions = await GetRegionsForDropdown();
            model.Difficulties = await GetDifficultiesForDropdown();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddWalkViewModel model)
        {
            var client = httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7000/api/walks"),
                Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = await client.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            model.Regions = await GetRegionsForDropdown();
            model.Difficulties = await GetDifficultiesForDropdown();
            ModelState.AddModelError("", "Failed to add walk.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7000/api/walks/{id}");

            if (response.IsSuccessStatusCode)
            {
                var stringResponseBody = await response.Content.ReadAsStringAsync();
                var walk = JsonSerializer.Deserialize<WalkDto>(stringResponseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var updateModel = new UpdateWalkViewModel
                {
                    Id = walk.Id,
                    Name = walk.Name,
                    Description = walk.Description,
                    LengthInKm = walk.LengthInKm,
                    WalkImageUrl = walk.WalkImageUrl,
                    DifficultyId = walk.DifficultyId,
                    RegionId = walk.RegionId,
                    Regions = await GetRegionsForDropdown(),
                    Difficulties = await GetDifficultiesForDropdown()
                };

                return View(updateModel);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateWalkViewModel request)
        {
            var client = httpClientFactory.CreateClient();
            var httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"https://localhost:7000/api/walks/{request.Id}"),
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            var httpResponseMessage = await client.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            request.Regions = await GetRegionsForDropdown();
            request.Difficulties = await GetDifficultiesForDropdown();
            ModelState.AddModelError("", "Failed to update walk.");
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                var httpResponseMessage = await client.DeleteAsync($"https://localhost:7000/api/walks/{id}");

                httpResponseMessage.EnsureSuccessStatusCode();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex);
            }

            return RedirectToAction("Edit", new { id = id });
        }

        private async Task<IEnumerable<SelectListItem>> GetRegionsForDropdown()
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7000/api/regions");
            if (response.IsSuccessStatusCode)
            {
                var stringResponseBody = await response.Content.ReadAsStringAsync();
                var regions = JsonSerializer.Deserialize<List<RegionDto>>(stringResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return regions.Select(r => new SelectListItem { Text = r.Name, Value = r.Id.ToString() });
            }
            return new List<SelectListItem>();
        }

        private async Task<IEnumerable<SelectListItem>> GetDifficultiesForDropdown()
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7000/api/difficulties");
            if (response.IsSuccessStatusCode)
            {
                var stringResponseBody = await response.Content.ReadAsStringAsync();
                var difficulties = JsonSerializer.Deserialize<List<DifficultyDto>>(stringResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return difficulties.Select(d => new SelectListItem { Text = d.Name, Value = d.Id.ToString() });
            }
            return new List<SelectListItem>();
        }
    }
}
