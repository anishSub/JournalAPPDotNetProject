using Microsoft.Maui.Storage;

namespace JournalApp.Services;

/// <summary>
/// Manages user authentication state and credential storage (PIN/Password).
/// Using Preferences for simplified storage.
/// </summary>
public class AuthService
{
    private const string PasswordKey = "MasterPassword";
    private const string PinKey = "MasterPin";

    public bool IsAuthenticated { get; private set; } = false;
    public bool JustLoggedIn { get; private set; } = false;

    public void LoginSuccess()
    {
        IsAuthenticated = true;
        JustLoggedIn = true;
    }

    public void ClearLoginFlag()
    {
        JustLoggedIn = false;
    }

    public void Logout()
    {
        IsAuthenticated = false;
        JustLoggedIn = false;
    }

    public async Task<bool> HasPasswordAsync()
    {
        // Using Preferences instead of SecureStorage to avoid Keychain Entitlement issues on MacCatalyst without provisioning profile
        var password = Preferences.Default.Get(PasswordKey, string.Empty);
        return !string.IsNullOrEmpty(password);
    }

    public async Task<bool> HasPinAsync()
    {
        var pin = Preferences.Default.Get(PinKey, string.Empty);
        return !string.IsNullOrEmpty(pin);
    }

    public async Task SetPasswordAsync(string password)
    {
        Preferences.Default.Set(PasswordKey, password);
        await Task.CompletedTask;
    }

    public async Task SetPinAsync(string pin)
    {
        Preferences.Default.Set(PinKey, pin);
        await Task.CompletedTask;
    }
    
    public void RemovePin()
    {
        Preferences.Default.Remove(PinKey);
    }

    public async Task<bool> VerifyPasswordAsync(string input)
    {
        var storedPassword = Preferences.Default.Get(PasswordKey, string.Empty);
        return storedPassword == input;
    }

    public async Task<bool> VerifyPinAsync(string input)
    {
        var storedPin = Preferences.Default.Get(PinKey, string.Empty);
        return storedPin == input;
    }
}
