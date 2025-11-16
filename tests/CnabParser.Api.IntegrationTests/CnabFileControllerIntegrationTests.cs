using System.Net.Http.Json;
using CnabParser.Application.Services;
using Shouldly;
using Xunit;

namespace CnabParser.Api.Tests;

public class CnabFileControllerIntegrationTests : IClassFixture<CnabFileControllerFixture>
{
    private readonly CnabFileControllerFixture _fixture;

    public CnabFileControllerIntegrationTests(CnabFileControllerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UploadAndGetCnabFileData_CompleteWorkflow_ReturnsStoresAndTransactions()
    {
        // Arrange
        var testFilePath = Path.Combine(AppContext.BaseDirectory, "test-cnab.txt");
        var fileContent = File.ReadAllBytes(testFilePath);
        using var formContent = new MultipartFormDataContent
        {
            { new ByteArrayContent(fileContent), "file", "test-cnab.txt" }
        };

        // Act - Upload file
        var uploadResponse = await _fixture.HttpClient.PostAsync("/cnab-files", formContent);

        // Assert
        uploadResponse.IsSuccessStatusCode.ShouldBeTrue();
        var importResult = await uploadResponse.Content.ReadFromJsonAsync<ImportResponse>();
        importResult.ShouldNotBeNull();
        importResult.ImportId.ShouldNotBeNullOrEmpty();
        importResult.TransactionCount.ShouldBe(4);

        // Act - Retrieve the data
        var getResponse = await _fixture.HttpClient.GetAsync($"/cnab-files/{importResult.ImportId}");

        // Assert
        getResponse.IsSuccessStatusCode.ShouldBeTrue();
        var importDataResponse = await getResponse.Content.ReadFromJsonAsync<ImportDataResponse>();
        importDataResponse.ShouldNotBeNull();
        importDataResponse.Data.ShouldNotBeEmpty();
        importDataResponse.Data.Count.ShouldBe(3);

        // Check that each store has transactions
        foreach (var storeData in importDataResponse.Data)
        {
            storeData.ShouldNotBeNull();
            storeData.Name.ShouldNotBeNullOrEmpty();
            storeData.OwnerName.ShouldNotBeNullOrEmpty();
            storeData.Balance.ShouldNotBe(0);
            storeData.Transactions.Length.ShouldBeGreaterThan(0);
        }
    }

    [Fact]
    public async Task UploadCnabFile_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        using var formContent = new MultipartFormDataContent
        {
            { new ByteArrayContent(Array.Empty<byte>()), "file", "empty.txt" }
        };

        // Act
        var response = await _fixture.HttpClient.PostAsync("/cnab-files", formContent);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCnabFileData_WithInvalidImportId_ReturnsBadRequest()
    {
        // Act
        var response = await _fixture.HttpClient.GetAsync("/cnab-files/invalid-guid");

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCnabFileData_WithNonExistentImportId_ReturnsEmptyData()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await _fixture.HttpClient.GetAsync($"/cnab-files/{nonExistentId}");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        var data = await response.Content.ReadFromJsonAsync<ImportDataResponse>();
        data.ShouldNotBeNull();
        data.Data.ShouldBeEmpty();
    }
}
