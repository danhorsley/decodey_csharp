namespace Decodey.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Gets whether the user is authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets whether the user is a subadmin
        /// </summary>
        bool IsSubAdmin { get; }

        /// <summary>
        /// Gets the current user
        /// </summary>
        UserInfo CurrentUser { get; }

        /// <summary>
        /// Logs in with the given credentials
        /// </summary>
        Task<AuthResult> Login(string username, string password, bool rememberMe);

        /// <summary>
        /// Signs up with the given information
        /// </summary>
        Task<AuthResult> Signup(SignupRequest request);

        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task<AuthResult> Logout();

        /// <summary>
        /// Sends a forgot password request
        /// </summary>
        Task<AuthResult> ForgotPassword(string email);

        /// <summary>
        /// Checks if a username is available
        /// </summary>
        Task<UsernameAvailabilityResult> CheckUsernameAvailability(string username);

        /// <summary>
        /// Initializes authentication from stored tokens
        /// </summary>
        Task Initialize();

        /// <summary>
        /// Event that is raised when the authentication state changes
        /// </summary>
        event EventHandler<AuthStateChangedEventArgs> AuthStateChanged;
    }

    /// <summary>
    /// Result of an authentication operation
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }
        public UserInfo User { get; set; }
        public bool HasActiveGame { get; set; }
    }

    /// <summary>
    /// Result of a username availability check
    /// </summary>
    public class UsernameAvailabilityResult
    {
        public bool Available { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Request for signup
    /// </summary>
    public class SignupRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EmailConsent { get; set; }
    }

    /// <summary>
    /// Information about the user
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public bool IsSubAdmin { get; set; }
    }

    /// <summary>
    /// Event args for authentication state changed
    /// </summary>
    public class AuthStateChangedEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; set; }
        public UserInfo User { get; set; }
        public bool HasActiveGame { get; set; }
    }
}