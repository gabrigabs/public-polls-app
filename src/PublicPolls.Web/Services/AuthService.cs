using System.Net.Http.Json;
using System.Text.Json;

namespace PublicPolls.Web.Services;

public interface IAuthService
{
    bool IsAuthenticated { get; }
    UserInfo? CurrentUser { get; }
    event Action? OnAuthStateChanged;
    Task<AuthResponse> LoginAsync(string email, string password);
    Task<AuthResponse> RegisterAsync(string email, string password, string name);
    void Logout();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private string? _token;
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    public UserInfo? CurrentUser { get; private set; }
    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
            
            if (!response.IsSuccessStatusCode)
                return await HandleErrorResponse(response);
            
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (result?.Success == true)
            {
                _token = result.Token;
                CurrentUser = result.User;
                _http.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                OnAuthStateChanged?.Invoke();
            }
            
            return result ?? new AuthResponse { Success = false, Error = "Resposta inválida do servidor" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Error = ex.Message };
        }
    }

    public async Task<AuthResponse> RegisterAsync(string email, string password, string name)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new { email, password, name });
            
            if (!response.IsSuccessStatusCode)
                return await HandleErrorResponse(response);
            
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (result?.Success == true)
            {
                _token = result.Token;
                CurrentUser = result.User;
                _http.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                OnAuthStateChanged?.Invoke();
            }
            
            return result ?? new AuthResponse { Success = false, Error = "Resposta inválida do servidor" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Error = ex.Message };
        }
    }

    private async Task<AuthResponse> HandleErrorResponse(HttpResponseMessage response)
    {
        try 
        {
            // Tenta ler como AuthResponse primeiro
            var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResult != null && !string.IsNullOrEmpty(authResult.Error))
            {
                return authResult;
            }

            // Tenta ler como ProblemDetails
            var errorContent = await response.Content.ReadAsStringAsync();
            try 
            {
                var problem = JsonSerializer.Deserialize<JsonElement>(errorContent);
                if (problem.TryGetProperty("detail", out var detail))
                {
                    return new AuthResponse { Success = false, Error = detail.GetString() };
                }
                if (problem.TryGetProperty("title", out var title))
                {
                     return new AuthResponse { Success = false, Error = title.GetString() };
                }
            }
            catch {}

            return new AuthResponse { Success = false, Error = $"Erro: {response.ReasonPhrase} ({response.StatusCode})" };
        }
        catch
        {
            return new AuthResponse { Success = false, Error = $"Erro de comunicação: {response.StatusCode}" };
        }
    }

    public void Logout()
    {
        _token = null;
        CurrentUser = null;
        _http.DefaultRequestHeaders.Authorization = null;
        OnAuthStateChanged?.Invoke();
    }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
