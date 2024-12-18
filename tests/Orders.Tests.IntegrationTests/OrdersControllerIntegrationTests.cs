using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Orders.Core.DTO;
using Orders.Infrastructure.Repositories;
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
			factory.ResetDb();
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
				responseContent,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			);

			orders.Should().NotBeEmpty();
		}

		[Fact]
		public async Task GetAllOrders_EmptyOrders_ReturnsEmptyList()
		{
			HttpResponseMessage result = await _httpClient.GetAsync("/api/Orders");

			string responseContent = await result.Content.ReadAsStringAsync();

			// Deserialize the JSON response to List<OrderResponse>
			List<OrderResponse> orders = JsonSerializer.Deserialize<List<OrderResponse>>(
				responseContent,
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
			);

			orders.Should().BeEmpty();
		}
	}
}
