using CourseApp.UI.Exceptions;
using CourseApp.UI.Filters;
using CourseApp.UI.Models;
using CourseApp.UI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CourseApp.UI.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class StudentController : Controller
    {
        private HttpClient _client;
        private readonly ICrudService _crudService;

        public StudentController(ICrudService crudService)
        {
            _client = new HttpClient();
            _crudService = crudService;
        }

        private async Task<List<GroupListItemGetResponse>> GetGroupsAsync()
        {
            var token = Request.Cookies["token"];
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("https://localhost:44392/api/groups/all");
            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<PaginatedResponse<GroupListItemGetResponse>>(await response.Content.ReadAsStringAsync(), options);
            return data.Items;
        }

        public async Task<IActionResult> Index(int page = 1, int size = 4)
        {
            var data = await _crudService.GetAllPaginated<StudentListItemGetResponse>("students", page);

            if (data.TotalPages < page)
            {
                return RedirectToAction("index", new { page = data.TotalPages });
            }

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var groups = await GetGroupsAsync();
            ViewBag.Groups = groups;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(StudentCreateRequest dto)
        {
            try
            {
                await _crudService.CreateFromForm(dto, "students");
                return RedirectToAction("Index");
            }
            catch (ModelException ex)
            {
                foreach (var error in ex.Error.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Message);
                }
            }
            catch (HttpException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            var groups = await GetGroupsAsync();
            ViewBag.Groups = groups;
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _crudService.Get<StudentCreateRequest>($"students/{id}");
            var groups = await GetGroupsAsync();
            ViewBag.Groups = groups;
            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, StudentCreateRequest dto)
        {
            try
            {
                await _crudService.UpdateFromForm(dto, $"students/{id}");
                return RedirectToAction("Index");
            }
            catch (ModelException ex)
            {
                foreach (var error in ex.Error.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Message);
                }
            }
            catch (HttpException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            var groups = await GetGroupsAsync();
            ViewBag.Groups = groups;
            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _crudService.Delete($"students/{id}");
                return RedirectToAction("Index");
            }
            catch (HttpException ex)
            {
                return StatusCode((int)ex.Status);
            }
        }
    }
}
