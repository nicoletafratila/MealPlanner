namespace Identity.Shared.Constants
{
    public class IdentityControllers
    {
        public const string Authentication = "Authentication";
        public const string ApplicationUser = "ApplicationUser";
        public const string ContactUs = "ContactUs";

        public const string AuthenticationUrl = "api/authentication";
        public const string ApplicationUserUrl = "api/applicationuser";
        public const string ContactUsUrl = "api/contactus";

        // Authentication sub-routes
        public const string LoginRoute = "login";
        public const string RegisterRoute = "register";
        public const string LogoutRoute = "logout";
        public const string ForgotPasswordRoute = "forgot-password";
        public const string ResetPasswordRoute = "reset-password";
        public const string ChangePasswordRoute = "change-password";

        // ApplicationUser sub-routes
        public const string EditRoute = "edit";
        public const string UnlockRoute = "unlock";

        // ContactUs sub-routes
        public const string SendRoute = "send";
    }
}
