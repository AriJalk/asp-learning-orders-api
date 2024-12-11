using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orders.Tests.IntegrationTests
{
	public class OrdersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
	{
		private readonly HttpClient _httpClient;
		private readonly IFixture _fixture;

		public OrdersControllerIntegrationTests(CustomWebApplicationFactory factory)
		{
			_httpClient = factory.CreateClient();
			//_httpClient.BaseAddress = new Uri("https://localhost:7273");
			_fixture = new Fixture();
		}

		[Fact]
		public async Task Orders_GetAllOrders_Successful()
		{
			await _httpClient.PostAsJsonAsync("/api/Orders", _fixture.Create<OrderAddRequest>());
			HttpResponseMessage result = await _httpClient.GetAsync("/api/Orders");

			string responseContent = await result.Content.ReadAsStringAsync();

			// Deserialize the JSON response to List<OrderResponse>
			List<OrderResponse> orders = JsonSerializer.Deserialize<List<OrderResponse>>(
				responseContent,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			);

			Assert.NotEmpty( orders );
		}
	}
}
