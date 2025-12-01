using System;
using System.Net;
using System.Threading.Tasks;
using MPolls.WebUI.Models.Auth;

namespace MPolls.WebUI.Services;

/// <summary>
/// Tracks the currently authenticated user in the Web UI and coordinates session refreshes.
/// </summary>
public class AuthState
{
    private readonly AuthClient _authClient;
    private AuthLoginResponse? _currentUser;
    private bool _isInitialized;
    private Task? _initializationTask;
    private readonly object _initializationLock = new();

    public event Action? AuthenticationStateChanged;

    public bool IsAuthenticated => _currentUser is not null;

    public AuthLoginResponse? CurrentUser => _currentUser;

    public AuthState(AuthClient authClient)
    {
        _authClient = authClient;
    }

    /// <summary>
    /// Ensures the session has been loaded before components render gated content.
    /// </summary>
    public Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return _initializationTask ?? Task.CompletedTask;
        }

        lock (_initializationLock)
        {
            if (_initializationTask is null || _initializationTask.IsFaulted || _initializationTask.IsCanceled)
            {
                // Reuse the same task so every caller observes the same load pipeline.
                _initializationTask = InitializeInternalAsync();
            }

            return _initializationTask;
        }
    }

    /// <summary>
    /// Updates the cached authenticated user once a login or refresh completes.
    /// </summary>
    public Task SetAuthenticatedUser(AuthLoginResponse user)
    {
        var hasChanged = _currentUser is null || !_currentUser.Equals(user);
        _currentUser = user;

        if (hasChanged)
        {
            NotifyStateChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears the local user session and notifies subscribers when logout succeeds.
    /// </summary>
    public async Task Logout()
    {
        var wasAuthenticated = _currentUser is not null;

        try
        {
            await _authClient.LogoutAsync();
        }
        catch
        {
            // Ignore logout errors and clear local state regardless.
        }

        _currentUser = null;

        if (wasAuthenticated)
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Updates the cached ULID when profile edits occur without reloading the full session.
    /// </summary>
    public Task UpdateUlidAsync(string? ulid)
    {
        if (_currentUser is null || string.IsNullOrWhiteSpace(ulid))
        {
            return Task.CompletedTask;
        }

        var normalized = ulid.Trim();

        if (string.Equals(_currentUser.Ulid, normalized, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        _currentUser = _currentUser with { Ulid = normalized };
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggers an explicit session reload, for instance after a refresh token exchange.
    /// </summary>
    public Task RefreshSessionAsync()
    {
        return LoadSessionAsync();
    }

    private void NotifyStateChanged() => AuthenticationStateChanged?.Invoke();

    private async Task InitializeInternalAsync()
    {
        try
        {
            await LoadSessionAsync();
            _isInitialized = true;
        }
        catch
        {
            lock (_initializationLock)
            {
                // Allow retries after a transient failure instead of locking in a failed task.
                _initializationTask = null;
            }

            throw;
        }
    }

    private async Task LoadSessionAsync(bool allowRefresh = true)
    {
        try
        {
            var sessionResponse = await _authClient.GetSessionAsync();

            if (sessionResponse.Payload is not null)
            {
                var hasChanged = _currentUser is null || !_currentUser.Equals(sessionResponse.Payload);
                _currentUser = sessionResponse.Payload;

                if (hasChanged)
                {
                    NotifyStateChanged();
                }

                return;
            }

            if (allowRefresh && sessionResponse.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                var refreshResponse = await _authClient.RefreshSessionAsync();

                if (refreshResponse.Payload is not null)
                {
                    // When refresh succeeds, reload so the session payload is normalized by the API.
                    await LoadSessionAsync(allowRefresh: false);
                    return;
                }
            }

            if (_currentUser is not null)
            {
                _currentUser = null;
                NotifyStateChanged();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            // Ignore network errors when loading the session to avoid breaking the UI.
        }
    }
}
