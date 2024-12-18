using AutoFixture;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Orders.Core.DTO;
using System.Net.Http.Json;
using System.Text.Json;

namespace Orders.Tests.IntegrationTests
{
	public class OrdersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
	{
		private readonly CustomWebApplicationFactory _factory;
		private readonly HttpClient _httpClient;
		private readonly IFixture _fixture;
		private readonly JsonSerializerOptions _jsonSerializerOptions;

		public OrdersControllerIntegrationTests(CustomWebApplicationFactory factory)
		{
			_factory = factory;
			_httpClient = _factory.CreateClient();
			//_httpClient.BaseAddress = new Uri("https://localhost:7273");
			_fixture = new Fixture();
			_jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
		}

		public async Task InitializeAsync()
		{
			await _factory.OpenConnection();
		}

		public async Task DisposeAsync()
		{
			await _factory.CloseConnection();
		}

		[Fact]
		public async Task GetAllOrders_SomeOrders_NotEmptyResponse()
		{
			List<OrderAddRequest> orderAddRequests = _fixture.Build<OrderAddRequest>()
				.CreateMany(5).ToList();
			foreach(OrderAddRequest addRequest in orderAddRequests)
			{
				await _httpClient.PostAsJsonAsync("/api/Orders", addRequest);
			}
			HttpResponseMessage result = await _httpClient.GetAsync("/api/Orders");

			string responseContent = await result.Content.ReadAsStringAsync();

			// Deserialize the JSON response to List<OrderResponse>
			List<OrderResponse> orders = JsonSerializer.Deserialize<List<OrderResponse>>(
				responseContent, _jsonSerializerOptions);

			orders.Should().NotBeEmpty();
		}

		[Fact]
		public async Task GetAllOrders_EmptyOrders_ReturnsEmptyList()
		{
			HttpResponseMessage result = await _httpClient.GetAsync("/api/Orders");

			string responseContent = await result.Content.ReadAsStringAsync();

			// Deserialize the JSON response to List<OrderResponse>
			List<OrderResponse> orders = JsonSerializer.Deserialize<List<OrderResponse>>(
				responseContent, _jsonSerializerOptions);

			orders.Should().BeEmpty();
		}

	}
}
