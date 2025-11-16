using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CnabParser.Web.Models;

namespace CnabParser.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public IFormFile? UploadedFile { get; set; }

    public List<StoreResponse>? Stores { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ErrorMessage = "Please select a file to upload.";
            return Page();
        }

        var httpClient = _httpClientFactory.CreateClient("api");

        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(UploadedFile.OpenReadStream()), "file", UploadedFile.FileName);

            var response = await httpClient.PostAsync("cnab-files", content);

            if (response.IsSuccessStatusCode)
            {
                var uploadResult = await response.Content.ReadFromJsonAsync<UploadResponse>();
                if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.ImportId))
                {
                    var queryResponse = await httpClient.GetAsync($"cnab-files/{uploadResult.ImportId}");
                    if (queryResponse.IsSuccessStatusCode)
                    {
                        var queryPayload = await queryResponse.Content.ReadFromJsonAsync<QueryResponse>();
                        Stores = queryPayload?.Data;
                    }
                    else
                    {
                        var errorContent = await queryResponse.Content.ReadAsStringAsync();
                        ErrorMessage = $"Error fetching transactions: {queryResponse.StatusCode} - {errorContent}";
                    }
                }
                else
                {
                    ErrorMessage = "Upload successful, but no ImportId was returned.";
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Error uploading file: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
        }

        return Page();
    }
}
