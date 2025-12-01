using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MPolls.WebUI.Models;

namespace MPolls.WebUI.Services;

public class EmployeesClient
{
    private readonly HttpClient _httpClient;

    public EmployeesClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<EmployeeDto>>("api/v1/employees")
                   ?? new List<EmployeeDto>();
        }
        catch
        {
            return new List<EmployeeDto>();
        }
    }

    public async Task<EmployeeDto?> AddEmployeeAsync(AddEmployeeCommand command)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/employees", command);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EmployeeDto>();
            }
        }
        catch
        {
        }

        return null;
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeCommand command)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/employees/{id}", command);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EmployeeDto>();
            }
        }
        catch
        {
        }

        return null;
    }

    public async Task DeleteEmployeeAsync(Guid id)
    {
        try
        {
            await _httpClient.DeleteAsync($"api/v1/employees/{id}");
        }
        catch
        {
        }
    }
}
