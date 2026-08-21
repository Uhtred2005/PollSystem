using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PollService.Data;
using PollService.Models;
using System.Net;
using System.Net.Http.Json;
using System.Linq;

namespace PollService.Tests;

public class PollApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PollApiTests(WebApplicationFactory<Program> factory)
    {
        var mockFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 1. Tìm và xóa sạch cấu hình PostgreSQL cũ
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. TẠO VÙNG NHỚ ĐỘC LẬP (Đây là chìa khóa sửa lỗi InternalServerError)
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // 3. Tiêm lại AppDbContext với vùng nhớ mới này
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestPollDb");
                    options.UseInternalServiceProvider(serviceProvider); // Ép dùng vùng nhớ độc lập
                });
            });
        });

        _client = mockFactory.CreateClient();
    }

    [Fact]
    public async Task CreatePoll_WithValidData_ReturnsCreatedResponse()
    {
        // Arrange
        var requestDto = new
        {
            Question = "What is your favorite cloud provider?",
            Options = new List<string> { "AWS", "Azure", "GCP" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/polls", requestDto);

        // Bẫy lỗi: In chi tiết nếu thất bại
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            throw new Exception($"API failed with {response.StatusCode}. Error Body: {errorDetails}");
        }

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdPoll = await response.Content.ReadFromJsonAsync<Poll>();
        Assert.NotNull(createdPoll);
        Assert.Equal(6, createdPoll.Code.Length);
        Assert.Equal(3, createdPoll.Options.Count);
    }

    [Fact]
    public async Task CreatePoll_WithOnlyOneOption_ReturnsBadRequest()
    {
        var requestDto = new
        {
            Question = "Is this a valid poll?",
            Options = new List<string> { "Yes" }
        };

        var response = await _client.PostAsJsonAsync("/api/polls", requestDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithSevenOptions_ReturnsBadRequest()
    {
        var requestDto = new
        {
            Question = "Too many options?",
            Options = new List<string> { "1", "2", "3", "4", "5", "6", "7" }
        };

        var response = await _client.PostAsJsonAsync("/api/polls", requestDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePoll_WithEmptyQuestion_ReturnsBadRequest()
    {
        var requestDto = new
        {
            Question = "",
            Options = new List<string> { "Option A", "Option B" }
        };

        var response = await _client.PostAsJsonAsync("/api/polls", requestDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}