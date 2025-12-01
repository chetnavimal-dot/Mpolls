using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MPolls.WebUI.Components;
using MPolls.WebUI.Components.Modals;
using MPolls.WebUI.Models;
using MPolls.WebUI.Services;
using MudBlazor;

namespace MPolls.WebUI.Pages;

public partial class Employees : ComponentBase
{
    [Inject] private EmployeesClient EmployeesClient { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthState AuthState { get; set; } = default!;

    private List<EmployeeDto>? _employees;

    public class CardModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private List<CardModel> Cards = new()
    {
        new CardModel { Id = Guid.Parse("9e7f2b42-bc4e-4d1a-9a84-2ac4a47f6d2f"), Title = "Automobile Survey" },
        new CardModel { Id = Guid.Parse("3c9d6baf-2e1d-4b0c-bdb6-16e1b2cb053d"), Title = "Cosmetic Survey" },
        new CardModel { Id = Guid.Parse("12fd1a9f-8487-4c5c-9e7c-8bfc2d45f1b1"), Title = "Household Survey" }
    };

    private void NavigateToSurvey(Guid id)
    {
        NavigationManager.NavigateTo($"/engage/{id}");
    }

    protected override async Task OnInitializedAsync()
    {
        // Employee management is restricted; wait for the session to hydrate before loading data.
        await AuthState.InitializeAsync();

        if (!AuthState.IsAuthenticated)
        {
            // Surface the dedicated 403 page so visitors see clear instructions for regaining access.
            NavigationManager.NavigateTo("/error/403Forbidden", forceLoad: false, replace: true);
            return;
        }

        try
        {
            _employees = await EmployeesClient.GetEmployeesAsync();

            var currentUser = AuthState.CurrentUser!;

            if (!currentUser.vFlag)
            {
                var parameters = new DialogParameters
                {
                    { "ContentText", "Your account is not yet verified. Please verify your email address to access all features.\nCheck your inbox for a verification link or click below to resend it." },
                    { "ButtonText", "Resend" }
                };

                var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, BackdropClick = false };

                var dialog = await DialogService.ShowAsync<AlertDialog>("Account Verification Required", parameters, options);
                var result = await dialog.Result;

                if (!result.Canceled)
                    StateHasChanged();
            }
        }
        catch
        {
            Snackbar.Add("Failed to load employees.", Severity.Error);
            _employees = new List<EmployeeDto>();
        }
    }

    private async Task RefreshAsync()
    {
        _employees = await EmployeesClient.GetEmployeesAsync();
    }

    private async Task AddEmployee()
    {
        await ShowEmployeeDialog(new UpdateEmployeeCommand());
    }

    private async Task EditEmployee(EmployeeDto dto)
    {
        var model = new UpdateEmployeeCommand
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Salary = dto.Salary
        };

        await ShowEmployeeDialog(model);
    }

    private async Task ShowEmployeeDialog(UpdateEmployeeCommand model)
    {
        var parameters = new DialogParameters { ["Model"] = model };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var title = model.Id == Guid.Empty ? "Add Employee" : "Edit Employee";
        var dialog = await DialogService.ShowAsync<EmployeeDialog>(title, parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: UpdateEmployeeCommand employeeDetails })
        {
            if (employeeDetails.Id == Guid.Empty)
            {
                var add = new AddEmployeeCommand
                {
                    FirstName = employeeDetails.FirstName,
                    LastName = employeeDetails.LastName,
                    Email = employeeDetails.Email,
                    Salary = employeeDetails.Salary
                };
                var added = await EmployeesClient.AddEmployeeAsync(add);
                if (added is null)
                {
                    Snackbar.Add("Failed to add employee.", Severity.Error);
                    return;
                }
            }
            else
            {
                var updated = await EmployeesClient.UpdateEmployeeAsync(employeeDetails.Id, employeeDetails);
                if (updated is null)
                {
                    Snackbar.Add("Failed to update employee.", Severity.Error);
                    return;
                }
            }

            await RefreshAsync();
        }
    }

    private async Task DeleteEmployeeAsync(Guid id)
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        bool? confirm = await DialogService.ShowMessageBox("Confirm", "Are you sure you want to delete the selected employee?", yesText: "Delete", cancelText: "Cancel", options: options);

        if (confirm == true)
        {
            try
            {
                await EmployeesClient.DeleteEmployeeAsync(id);
                await RefreshAsync();
            }
            catch
            {
                Snackbar.Add("Failed to delete employee.", Severity.Error);
            }
        }
    }
}
