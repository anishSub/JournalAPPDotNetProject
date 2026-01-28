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

    //login success
    public void LoginSuccess()
    {
        IsAuthenticated = true;
        JustLoggedIn = true;
    }

    //clear login flag
    public void ClearLoginFlag()
    {
        JustLoggedIn = false;
    }

    //logout
    public void Logout()
    {
        IsAuthenticated = false;
        JustLoggedIn = false;
    }

    //check if password exists
    public async Task<bool> HasPasswordAsync()
    {
        // Using Preferences instead of SecureStorage to avoid Keychain Entitlement issues on MacCatalyst without provisioning profile
        var password = Preferences.Default.Get(PasswordKey, string.Empty);
        return !string.IsNullOrEmpty(password);
    }

    //check if pin exists
    public async Task<bool> HasPinAsync()
    {
        var pin = Preferences.Default.Get(PinKey, string.Empty);
        return !string.IsNullOrEmpty(pin);
    }
    
    //save password
    public async Task SetPasswordAsync(string password)
    {
        Preferences.Default.Set(PasswordKey, password);
        await Task.CompletedTask;
    }
    
    //save pin
    public async Task SetPinAsync(string pin)
    {
        Preferences.Default.Set(PinKey, pin);
        await Task.CompletedTask;
    }
    
    //remove pin
    public void RemovePin()
    {
        Preferences.Default.Remove(PinKey);
    }
    
    //verify password
    public async Task<bool> VerifyPasswordAsync(string input)
    {
        var storedPassword = Preferences.Default.Get(PasswordKey, string.Empty);
        return storedPassword == input;
    }
    //verify pin
    public async Task<bool> VerifyPinAsync(string input)
    {
        var storedPin = Preferences.Default.Get(PinKey, string.Empty);
        return storedPin == input;
    }
}
