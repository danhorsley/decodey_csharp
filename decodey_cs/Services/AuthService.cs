using System;
using System.Threading.Tasks;

namespace Decodey.Services
{
    public class AuthService : IAuthService
    {
        private UserInfo _currentUser;
        private bool _isAuthenticated;

        public event EventHandler<AuthStateChangedEventArgs> AuthStateChanged;

        public bool IsAuthenticated => _isAuthenticated;
        public bool IsSubAdmin => _currentUser?.IsSubAdmin ?? false;
        public UserInfo CurrentUser => _currentUser;

        public AuthService()
        {
            // Constructor
        }

        public async Task Initialize()
        {
            // Check for saved token in preferences
            var token = Preferences.Get("auth-token", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                // For simplicity in this implementation, we'll just set a dummy user
                // In a real implementation, you would validate the token with your API
                _currentUser = new UserInfo
                {
                    Id = "user123",
                    Username = "Player",
                    IsSubAdmin = false
                };

                _isAuthenticated = true;

                // Notify about state change
                OnAuthStateChanged();
            }

            await Task.CompletedTask;
        }

        public async Task<AuthResult> Login(string username, string password, bool rememberMe)
        {
            // In a real implementation, you would call your API here
            // For this simple implementation, we'll accept any login with non-empty values

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Username and password are required"
                };
            }

            // Set dummy user
            _currentUser = new UserInfo
            {
                Id = "user123",
                Username = username,
                IsSubAdmin = username.ToLower() == "admin"
            };

            _isAuthenticated = true;

            // Save token if remember me is selected
            if (rememberMe)
            {
                Preferences.Set("auth-token", "dummy-token");
            }

            // Notify about state change
            OnAuthStateChanged();

            return new AuthResult
            {
                Success = true,
                User = _currentUser,
                HasActiveGame = false
            };
        }

        public async Task<AuthResult> Signup(SignupRequest request)
        {
            // In a real implementation, you would call your API here
            // For this simple implementation, we'll accept any signup with non-empty values

            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "All fields are required"
                };
            }

            // Set dummy user
            _currentUser = new UserInfo
            {
                Id = "user123",
                Username = request.Username,
                IsSubAdmin = false
            };

            _isAuthenticated = true;

            // Save token
            Preferences.Set("auth-token", "dummy-token");

            // Notify about state change
            OnAuthStateChanged();

            return new AuthResult
            {
                Success = true,
                User = _currentUser,
                HasActiveGame = false
            };
        }

        public async Task<AuthResult> Logout()
        {
            // Clear user and token
            _currentUser = null;
            _isAuthenticated = false;
            Preferences.Remove("auth-token");

            // Notify about state change
            OnAuthStateChanged();

            return new AuthResult
            {
                Success = true
            };
        }

        public async Task<AuthResult> ForgotPassword(string email)
        {
            // In a real implementation, you would call your API here
            return new AuthResult
            {
                Success = true,
                Message = "If an account exists with this email, a reset link will be sent."
            };
        }

        public async Task<UsernameAvailabilityResult> CheckUsernameAvailability(string username)
        {
            // In a real implementation, you would call your API here
            // For this simple implementation, we'll return true unless the username is "admin"
            return new UsernameAvailabilityResult
            {
                Available = !string.IsNullOrWhiteSpace(username) && username.ToLower() != "admin",
                Message = username.ToLower() == "admin" ? "This username is already taken" : "Username is available"
            };
        }

        private void OnAuthStateChanged()
        {
            AuthStateChanged?.Invoke(this, new AuthStateChangedEventArgs
            {
                IsAuthenticated = _isAuthenticated,
                User = _currentUser,
                HasActiveGame = false
            });
        }
    }
}