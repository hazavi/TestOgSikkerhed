// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using QRCoder;

namespace TestOgSikkerhed.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public LoginWith2faModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginWith2faModel> logger,
            UrlEncoder urlEncoder)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// QR code related properties
        /// </summary>
        public string SharedKey { get; set; }
        public string QrCodeImageSource { get; set; }
        public bool ShowQrCode { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            try
            {
                // Try to get the user from 2FA authentication flow
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                
                if (user == null)
                {
                    // If not in 2FA flow, check if user is authenticated already
                    user = await _userManager.GetUserAsync(User);
                    
                    if (user == null)
                    {
                        ErrorMessage = "Please log in first before accessing the two-factor authentication page.";
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }
                }

                ReturnUrl = returnUrl ?? Url.Content("~/");
                RememberMe = rememberMe;

                // Generate QR code for the authenticator app
                await LoadSharedKeyAndQrCodeAsync(user);
                ShowQrCode = true;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginWith2fa OnGetAsync: {Message}", ex.Message);
                ErrorMessage = "An error occurred. Please try logging in again.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                // Regenerate QR code for invalid model
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? 
                           await _userManager.GetUserAsync(User);
                           
                if (user != null)
                {
                    await LoadSharedKeyAndQrCodeAsync(user);
                    ShowQrCode = true;
                }
                
                return Page();
            }

            returnUrl ??= Url.Content("~/");

            try
            {
                // First try to get the user from the 2FA flow
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                
                if (user == null)
                {
                    // If not in 2FA flow, get the currently authenticated user
                    user = await _userManager.GetUserAsync(User);
                    
                    if (user == null)
                    {
                        ErrorMessage = "Please log in first before accessing the two-factor authentication page.";
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }
                    
                    // Make sure 2FA is enabled for this user
                    if (!await _userManager.GetTwoFactorEnabledAsync(user))
                    {
                        // 2FA not enabled, enable it and redirect
                        await _userManager.SetTwoFactorEnabledAsync(user, true);
                    }
                }

                // Clean up and verify the 2FA code
                var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
                
                // Different approach based on which authentication flow we're in
                Microsoft.AspNetCore.Identity.SignInResult result;
                
                if (await _signInManager.GetTwoFactorAuthenticationUserAsync() != null)
                {
                    // Normal 2FA flow - user already authenticated with username/password
                    result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                        authenticatorCode, rememberMe, Input.RememberMachine);
                }
                else
                {
                    // Direct flow - user is already signed in, just verifying the 2FA code
                    var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                        user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);
                        
                    if (isValid)
                    {
                        // If RememberMachine is checked, remember this browser for 2FA
                        if (Input.RememberMachine)
                        {
                            await _signInManager.RememberTwoFactorClientAsync(user);
                        }
                        
                        result = Microsoft.AspNetCore.Identity.SignInResult.Success;
                    }
                    else
                    {
                        result = Microsoft.AspNetCore.Identity.SignInResult.Failed;
                    }
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                    return LocalRedirect(returnUrl);
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                    ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                    
                    // Regenerate QR code for the view
                    await LoadSharedKeyAndQrCodeAsync(user);
                    ShowQrCode = true;
                    
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginWith2fa OnPostAsync: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                
                // Try to regenerate QR code
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync() ?? 
                           await _userManager.GetUserAsync(User);
                           
                if (user != null)
                {
                    await LoadSharedKeyAndQrCodeAsync(user);
                    ShowQrCode = true;
                }
                
                return Page();
            }
        }

        private async Task LoadSharedKeyAndQrCodeAsync(IdentityUser user)
        {
            try
            {
                // Get the authenticator key for the user
                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(unformattedKey))
                {
                    // If no key exists, create a new one
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                }

                SharedKey = FormatKey(unformattedKey);

                // Get the user's email for the QR code
                var email = await _userManager.GetEmailAsync(user);
                
                // Generate the QR code for the authenticator app
                var authenticatorUri = GenerateQrCodeUri(email, unformattedKey);
                QrCodeImageSource = GenerateQrCodeImageAsBase64(authenticatorUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code: {Message}", ex.Message);
                // Don't rethrow - we'll just not show the QR code
                ShowQrCode = false;
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                AuthenticatorUriFormat,
                _urlEncoder.Encode("TestOgSikkerhed"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private string GenerateQrCodeImageAsBase64(string text)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                
                return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
            }
        }
    }
}