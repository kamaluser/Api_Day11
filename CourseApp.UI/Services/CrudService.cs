using CourseApp.UI.Exceptions;
using CourseApp.UI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseApp.UI.Services
{
    public class CrudService : ICrudService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private const string baseUrl = "https://localhost:44392/api/";

        public CrudService(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext.Request.Cookies["token"];
            if (token != null)
            {
                if (_client.DefaultRequestHeaders.Contains(HeaderNames.Authorization))
                {
                    _client.DefaultRequestHeaders.Remove(HeaderNames.Authorization);
                }
                _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {token}");
            }
        }

        public async Task<PaginatedResponse<TResponse>> GetAllPaginated<TResponse>(string path, int page)
        {
            AddAuthorizationHeader();
            var response = await _client.GetAsync(baseUrl + path + "?page=" + page);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<PaginatedResponse<TResponse>>(content, options);
            }
            else
            {
                Console.WriteLine($"Error: {content}");
                throw new HttpException(response.StatusCode);
            }
        }

        public async Task<TResponse> Get<TResponse>(string path)
        {
            AddAuthorizationHeader();
            var response = await _client.GetAsync(baseUrl + path);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<TResponse>(content, options);
            }
            else
            {
                Console.WriteLine($"Error: {content}");
                throw new HttpException(response.StatusCode);
            }
        }

        public async Task<CreateResponse> Create<TRequest>(TRequest request, string path)
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(baseUrl + path, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<CreateResponse>(responseContent, options);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new ModelException(System.Net.HttpStatusCode.BadRequest, errorResponse);
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new HttpException(response.StatusCode, errorResponse.Message);
            }
        }

        public async Task Update<TRequest>(TRequest request, string path)
        {
            AddAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(baseUrl + path, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new ModelException(System.Net.HttpStatusCode.BadRequest, errorResponse);
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new HttpException(response.StatusCode, errorResponse.Message);
            }
        }

        public async Task Delete(string path)
        {
            AddAuthorizationHeader();
            var response = await _client.DeleteAsync(baseUrl + path);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpException(response.StatusCode);
            }
        }

        public async Task<CreateResponse> CreateFromForm<TRequest>(TRequest request, string path)
        {
            AddAuthorizationHeader();
            var multipartContent = CreateMultipartFormDataContent(request);
            var response = await _client.PostAsync(baseUrl + path, multipartContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<CreateResponse>(responseContent, options);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new ModelException(System.Net.HttpStatusCode.BadRequest, errorResponse);
            }
            else
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new HttpException(response.StatusCode, errorResponse.Message);
            }
        }

        public async Task UpdateFromForm<TRequest>(TRequest request, string path)
        {
            AddAuthorizationHeader();
            var multipartContent = CreateMultipartFormDataContent(request);
            var response = await _client.PutAsync(baseUrl + path, multipartContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new ModelException(System.Net.HttpStatusCode.BadRequest, errorResponse);
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, options);
                throw new HttpException(response.StatusCode, errorResponse.Message);
            }
        }

        private MultipartFormDataContent CreateMultipartFormDataContent<TRequest>(TRequest request)
        {
            var content = new MultipartFormDataContent();

            foreach (var prop in request.GetType().GetProperties())
            {
                var value = prop.GetValue(request);

                if (value is IFormFile file)
                {
                    content.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                }
                else if (value is DateTime dateTime)
                {
                    content.Add(new StringContent(dateTime.ToLongDateString()), prop.Name);
                }
                else if (value != null)
                {
                    content.Add(new StringContent(value.ToString()), prop.Name);
                }
            }

            return content;
        }
    }
}
