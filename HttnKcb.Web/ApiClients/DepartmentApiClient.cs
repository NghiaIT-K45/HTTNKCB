using System.Net.Http;
using System.Net.Http.Json;
using HttnKcb.Web.ViewModels;

namespace HttnKcb.Web.ApiClients
{
  public class DepartmentApiClient
  {
    private readonly HttpClient _client;

    public DepartmentApiClient(IHttpClientFactory factory)
    {
      _client = factory.CreateClient("ApiClient");
    }

    public async Task<List<DepartmentViewModel>> GetAllAsync(string? keyword)
    {
      // GIẢ SỬ Swagger ghi: GET /api/Departments
      string url = "/api/Departments";

      if (!string.IsNullOrWhiteSpace(keyword))
      {
        url += $"?keyword={keyword}";
      }

      var result = await _client.GetFromJsonAsync<List<DepartmentViewModel>>(url);
      return result ?? new List<DepartmentViewModel>();
    }

    public async Task<DepartmentViewModel?> GetByIdAsync(int id)
    {
      // GET /api/Departments/{id}
      return await _client.GetFromJsonAsync<DepartmentViewModel>($"/api/Departments/{id}");
    }

    public async Task<bool> CreateAsync(DepartmentViewModel vm)
    {
      // POST /api/Departments
      var response = await _client.PostAsJsonAsync("/api/Departments", vm);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, DepartmentViewModel vm)
    {
      // PUT /api/Departments/{id}
      var response = await _client.PutAsJsonAsync($"/api/Departments/{id}", vm);
      return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
      // DELETE /api/Departments/{id}
      var response = await _client.DeleteAsync($"/api/Departments/{id}");
      return response.IsSuccessStatusCode;
    }
  }
}
