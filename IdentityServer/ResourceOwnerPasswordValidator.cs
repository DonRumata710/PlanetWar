using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        //repository to get user from db
        private readonly DatabaseService _userRepository;

        public ResourceOwnerPasswordValidator(DatabaseService userRepository)
        {
            if (userRepository == null)
                throw new ArgumentNullException("userRepository");

            _userRepository = userRepository; //DI
            _userRepository.Open();
        }

        //this is used to validate your user account with provided grant at /connect/token
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            try
            {
                //get your user model from db (by username - in my case its email)
                var user = _userRepository.FetchByUsername(context.UserName);
                if (user != null)
                {
                    //check if password match - remember to hash password if stored as hash in db
                    if (CompareCredentials(context.Password, user.password))
                    {
                        //set the result
                        context.Result = new GrantValidationResult(
                            subject: user.userId.ToString(),
                            authenticationMethod: "custom",
                            claims: GetUserClaims(user));

                        return;
                    }

                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Incorrect password");
                    return;
                }
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "User does not exist.");
                return;
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid username or password");
                Log.Error("Failed to validate credentials: " + ex.Message);
            }
        }

        public bool CompareCredentials(string password, string hash)
        {
            return _userRepository.CompareCredentials(password, hash);
        }

        //build claims array from user data
        public static Claim[] GetUserClaims(User user)
        {
            return new Claim[]
            {
            new Claim("user_id", user.userId.ToString()),
            new Claim(JwtClaimTypes.Subject, user.userId.ToString()),
            new Claim(JwtClaimTypes.Name, user.name),
            new Claim(JwtClaimTypes.Email, user.email  ?? "")
            };
        }
    }
}
