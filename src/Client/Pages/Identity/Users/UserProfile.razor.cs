﻿using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.BlazorWebAssembly.Shared.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FSH.BlazorWebAssembly.Client.Pages.Identity.Users;

public partial class UserProfile
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    private ClaimsPrincipal? _currentUser;
    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }
    [Parameter]
    public string? Title { get; set; }
    [Parameter]
    public string? Description { get; set; }

    private bool _active;
    private char _firstLetterOfName;
    private string? _firstName;
    private string? _lastName;
    private string? _phoneNumber;
    private string? _email;
    private string? _imageUrl;
    private bool _loaded;
    private bool _canToggleUserStatus;

    private async Task ToggleUserStatus()
    {
        var request = new ToggleUserStatusRequest { ActivateUser = _active, UserId = Id };
        await UsersClient.ToggleUserStatusAsync(request);
        Snackbar.Add(_localizer["Updated User Status."], Severity.Success);
        Navigation.NavigateTo("/users");
    }

    [Parameter]
    public string? ImageUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        string? userId = Id;
        var result = await UsersClient.GetByIdAsync(userId);
        if (result.Succeeded)
        {
            var user = result.Data;
            if (user != null)
            {
                _firstName = user.FirstName;
                _lastName = user.LastName;
                _email = user.Email;
                _phoneNumber = user.PhoneNumber;
                _active = user.IsActive;
                _imageUrl = user.ImageUrl?.Replace("{server_url}/", Config[ConfigNames.ApiBaseUrl]);
            }

            Title = $"{_firstName} {_lastName}'s {_localizer["Profile"]}";
            Description = _email;
            if (_firstName?.Length > 0)
            {
                _firstLetterOfName = _firstName[0];
            }
        }

        var state = await AuthState;
        _currentUser = state.User;
        _canToggleUserStatus = (await AuthService.AuthorizeAsync(_currentUser, FSHPermissions.Users.Edit)).Succeeded;
        _loaded = true;
    }
}