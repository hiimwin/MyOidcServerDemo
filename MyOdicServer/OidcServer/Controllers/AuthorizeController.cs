using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OidcServer.Helpers;
using OidcServer.Models;
using OidcServer.Repositories;
using System.Security.Claims;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OidcServer.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly JsonWebKey jsonWebKey;
        private readonly TokenIssuingOptions tokenIssuingOptions;
        private readonly ICodeItemRepository codeRepository;
        private readonly IUserRepository userRepository;

        public AuthorizeController(IUserRepository userRepository, 
            ICodeItemRepository codeRepository, 
            TokenIssuingOptions tokenIssuingOptions,
            JsonWebKey jsonWebKey)
        {
            this.userRepository = userRepository;
            this.codeRepository = codeRepository;
            this.tokenIssuingOptions = tokenIssuingOptions;
            this.jsonWebKey = jsonWebKey;
        }
        public IActionResult Index(AuthenticationRequestModel authenticationRequest)
        {
            return View(authenticationRequest);
        }

        public IActionResult Authorize(AuthenticationRequestModel authenticationRequest, string user, string[] scopes)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                ModelState.AddModelError("", "User is required");
                return View("Index", authenticationRequest);
            }

            var foundUser = userRepository.FindByUserName(user);

            if (foundUser == null)
            {
                ModelState.AddModelError("", "User not found");
                return View("Index", authenticationRequest);
            }

            scopes ??= authenticationRequest.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string code = GenerateCode();

            codeRepository.Add(code, new CodeItem()
            {
                AuthenticationRequest = authenticationRequest,
                User = user,
                Scopes = scopes
            });

            var model = new CodeFlowResponseViewModel()
            {
                Code = code,
                State = authenticationRequest.State,
                RedirectUri = authenticationRequest.RedirectUri
            };

            var allowedRedirectUris = new List<string>
            {
                "http://localhost:5001/signin-oidc",     // Docker
                "https://localhost:7219/signin-oidc"     // Local
            };

            if (!allowedRedirectUris.Contains(authenticationRequest.RedirectUri))
            {
                return BadRequest("Invalid redirect_uri");
            }
            var redirectUrl = $"{authenticationRequest.RedirectUri}?code={code}&state={authenticationRequest.State}";
            return Redirect(redirectUrl);
        }

        [Route("oauth/token")]
        [HttpPost]
        public IActionResult ReturnTokens(string grant_type, string code, string redirect_uri)
        {
            if(grant_type != "authorization_code")
            {
                return BadRequest();
            }

            var codeItem = codeRepository.FindByCode(code);
            if (codeItem == null)
            {
                return BadRequest();
            }

            codeRepository.Delete(code);
            if (codeItem.AuthenticationRequest.RedirectUri != redirect_uri)
            {
                return BadRequest();
            }

            var model = new AuthenticationResponseModel()
            {
                AccessToken = GenerateAccessToken(codeItem.User, string.Join(" ", codeItem.Scopes), codeItem.AuthenticationRequest.ClientId, codeItem.AuthenticationRequest.Nonce, jsonWebKey),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = GenerateRefreshToken(),
                IdToken = GenerateIdToken(codeItem.User, codeItem.AuthenticationRequest.ClientId, codeItem.AuthenticationRequest.Nonce, jsonWebKey),
            };
            return Json(model);
        }

        private static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string GenerateIdToken(string userId, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // we can return some claims defined here: https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Name, userId)
            };

            var idToken = JwtGenerator.GenerateJWTToken(
                tokenIssuingOptions.IdTokenExpirySeconds,
                tokenIssuingOptions.Issuer,
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }

        private string GenerateAccessToken(string userId, string scope, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // access_token can be the same as id_token, but here we might have different values for expirySeconds so we use 2 different functions

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Name, userId),
                new("scope", scope) // Jeg vet ikke hvorfor JwtRegisteredClaimNames inneholder ikke "scope"??? Det har kun OIDC ting?  https://datatracker.ietf.org/doc/html/rfc8693#name-scope-scopes-claim
            };
            var idToken = JwtGenerator.GenerateJWTToken(
                tokenIssuingOptions.AccessTokenExpirySeconds,
                tokenIssuingOptions.Issuer,
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }


        private static string GenerateCode()
        {
            // 32 random bytes -> URL-safe Base64 string (no padding)
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            string base64 = Convert.ToBase64String(bytes);
            return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
