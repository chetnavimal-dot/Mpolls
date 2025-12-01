using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace MPolls.WebUI.Components.Validation;

public sealed class SubmitOnlyDataAnnotationsValidator : ComponentBase, IDisposable
{
    private ValidationMessageStore? _validationMessageStore;

    [CascadingParameter]
    private EditContext CurrentEditContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(SubmitOnlyDataAnnotationsValidator)} requires a cascading parameter of type {nameof(EditContext)}. " +
                $"For example, use it inside an {nameof(EditForm)} component.");
        }

        _validationMessageStore = new ValidationMessageStore(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += HandleValidationRequested;
        CurrentEditContext.OnFieldChanged += HandleFieldChanged;
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs args)
    {
        if (CurrentEditContext is null || _validationMessageStore is null)
        {
            return;
        }

        _validationMessageStore.Clear();

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(CurrentEditContext.Model);

        Validator.TryValidateObject(
            CurrentEditContext.Model,
            validationContext,
            validationResults,
            validateAllProperties: true);

        foreach (var validationResult in validationResults)
        {
            if (validationResult.MemberNames.Any())
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    var fieldIdentifier = CurrentEditContext.Field(memberName);
                    _validationMessageStore.Add(fieldIdentifier, validationResult.ErrorMessage ?? string.Empty);
                }
            }
            else
            {
                var fieldIdentifier = new FieldIdentifier(CurrentEditContext.Model, string.Empty);
                _validationMessageStore.Add(fieldIdentifier, validationResult.ErrorMessage ?? string.Empty);
            }
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs args)
    {
        if (_validationMessageStore is null)
        {
            return;
        }

        _validationMessageStore.Clear(args.FieldIdentifier);
        CurrentEditContext?.NotifyValidationStateChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // This validator does not render any UI.
    }

    public void Dispose()
    {
        if (CurrentEditContext is null)
        {
            return;
        }

        CurrentEditContext.OnValidationRequested -= HandleValidationRequested;
        CurrentEditContext.OnFieldChanged -= HandleFieldChanged;
    }
}
