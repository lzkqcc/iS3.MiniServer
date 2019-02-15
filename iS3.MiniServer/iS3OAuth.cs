using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;
using System.Net.Http;

using Microsoft.Owin;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;

/*
 * This file defines a few classes for account authentication and authorization.
 * 
 * 1. iS3User and iS3UserManager for account control
 * 2. iS3OAuthServerProvider for provding authenticaiton service
 * 3. iS3OAuthDbContext and iS3OAuthDbInitializer for managing accounts in database
 * 4. AccountsController for providing account control WebAPI
 * 
 * Account api usage examples in ubuntu-linux shell:
 * 
port=8080
curl http://localhost:$port/api/Accounts/GetUsers    
    [=>will be refused]
curl -d 'grant_type=password&username=Admin&password=iS3Admin' http://localhost:$port/Token
    [=>will return token for Admin]
token1=$[token from above responses]
curl -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/GetUsers
    [=>will succeeded, 1 user]
curl -d "Username=lxj&Password=lxjsPassword&ConfirmPassword=lxjsPassword&Role=User" -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/AddUser
    [=>will succeeded, 1 user added]
curl -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/GetUsers
    [=>will succeeded, 2 users]
curl -d "Username=lxj" -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/RemoveUser
    [=>will succeeded, 1 user removed]
curl -d '{"Username":"lxj","Password":"lxjsPassword", "ConfirmPassword":"lxjsPassword", "Role":"User"}' -H "Content-Type:application/json" -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/AddUser 
     [=>will fail, user exists]
curl -d 'grant_type=password&Username=lxj&Password=lxjsPassword' http://localhost:$port/Token
     [=>will return token for lxj]
token2=$[token from above responses]
curl -H "Authorization:Bearer $token2" http://localhost:$port/api/Accounts/GetUsers
     [=>will be denied because for insufficient authorization]
curl -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/GetUsers 
     [=>will succeeded, 2 users]
curl -H "Authorization:Bearer $token2" -d "OldPassword=lxjsPassword&Password=NewPassword&ConfirmPassword=NewPassword" http://localhost:$port/api/Accounts/ChangePassword
     [=>will succeeded, password changed (for lxj)]
* 
*/

namespace iS3.MiniServer
{
    public static class iS3ClaimTypes
    {
        public const string AuthorizedProjects = "iS3AuthorizedProjects";

    }

    // iS3User class is for user authentication such as register and login.
    // iS3User class inherits from Microsoft.AspNet.IdentityUser.
    // 
    public class iS3User : IdentityUser
    {
        public iS3User() : base() {  }

        // Password and Role is for admin to register new user remotely.
        // For example, we can register a new user
        //   UserName=john, Password=johnsPassword, ConfirmPassword=johnsPassword, Role=User
        // using following command remotely:
        //
        //   curl -d "Username=john&Password=johnsPassword&ConfirmPassword=johnsPassword&Role=User" 
        //        -H "Authorization:Bearer $token" http://$ip:$port/api/Accounts/AddUser
        //
        // In above, we use the 'curl' command. $ip is the address of host, $port
        //   is the host port, $token is the token of a user with Admin role.
        //
        // In our default context(see iS3OAuthDbInitializer.Seed), a user with the
        // Admin role is created as:
        //   Username=Admin, Password=iS3Admin, Role=Admin
        //
        // Therefore, you can login as the Admin user use the following command.
        // The server will return a token.
        //
        //   curl -d "grant_type=password&username=Admin&password=iS3Admin"
        //        http://$ip:$port/Token
        //

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string OldPassword { get; set; }
        public string Role { get; set; }

        // projects that the user can visit
        public string AuthorizedProjects { get; set; }
    }


    // iS3UserManager class is for user management.
    // iS3UserManager class inherits from Microsoft.AspNet.UserManager.
    // 
    public class iS3UserManager : UserManager<iS3User>
    {
        public iS3UserManager(IUserStore<iS3User> store)
            : base(store) { }


        public static iS3UserManager Create(
            IdentityFactoryOptions<iS3UserManager> options,
            IOwinContext context)
        {
            return new iS3UserManager(
                new UserStore<iS3User>(
                    context.Get<iS3OAuthDbContext>()));
        }
    }

    // iS3OAuthServerProvider class is for the authentication.
    // iS3OAuthServerProvider class inherits from Microsoft.Owin.Security.OAuth.
    // 
    public class iS3OAuthServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(
            OAuthValidateClientAuthenticationContext context)
        {
            // This call is required...
            // but we're not using client authentication, so validate and move on...
            await Task.FromResult(context.Validated());
        }


        public override async Task GrantResourceOwnerCredentials(
            OAuthGrantResourceOwnerCredentialsContext context)
        {
            // ** Use extension method to get a reference to the user manager from the Owin Context:
            var manager = context.OwinContext.GetUserManager<iS3UserManager>();

            // UserManager allows us to retrieve use with name/password combo:
            var user = await manager.FindAsync(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError(
                    "invalid_grant", "The user name or password is incorrect.");
                context.Rejected();
                return;
            }

            // Add claims associated with this user to the ClaimsIdentity object:
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            foreach (var userClaim in user.Claims)
            {
                identity.AddClaim(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            }

            context.Validated(identity);
        }
    }

    // iS3OAuthDbContext class is for authentication database management.
    // iS3OAuthDbContext class inherits from Microsoft.AspNet.Identity.EntityFramework.
    // Note:
    //   A default database "iS3Database" will be created using default EntityFramework
    //     database service provider - SqlCeProviderServices (SqlServerCe).
    // 
    public class iS3OAuthDbContext : IdentityDbContext<iS3User>
    {
        // The default database is specified here.
        //
        public iS3OAuthDbContext()
            : base("iS3Database") { }

        // Set database initializer, which will seed default Admin user.
        //
        static iS3OAuthDbContext()
        {
            Database.SetInitializer(new iS3OAuthDbInitializer());
        }

        // Create an instance of iS3OAuthDbContext for gloabl usage.
        // You can get the instance using:
        //    var context = Request.GetOwinContext().Get<iS3OAuthDbContext>();
        //
        public static iS3OAuthDbContext Create()
        {
            return new iS3OAuthDbContext();
        }
    }

    // iS3OAuthDbInitializer class is for intialize database.
    // Note:
    //   Inherits from DropCreateDatabaseAlways will re-create database every time.
    //     In this situation, all the inputted data will be lost,
    //     thus it is usually for debugging purposes.
    //
    //   Inherits from DropCreateDatabaseIfModelChanges only re-create database
    //     when data model is changed. For example, if we add a new data member
    //     to iS3User class, the data model is changed, and the database will
    //     re-created in such situation. When you deploy this program to a server,
    //     Therefore, avoid use DropCreateDatabaseAlways as base class.
    // 
    public class iS3OAuthDbInitializer
    : DropCreateDatabaseIfModelChanges<iS3OAuthDbContext>
    //: DropCreateDatabaseAlways<iS3OAuthDbContext>
    {
        // Seed a default user: Admin
        //   Username=Admin, Password=iS3Admin, Role=Admin
        //
        // You should change it to your desired name and password.
        //
        protected async override void Seed(iS3OAuthDbContext context)
        {
            // Set up initial user: admin
            var admin = new iS3User
            {
                UserName = "Admin"
            };

            // Introducing...the UserManager:
            var manager = new iS3UserManager(
                new UserStore<iS3User>(context));

            var result = await manager.CreateAsync(admin, "iS3Admin");

            // Add claims for Admin
            await manager.AddClaimAsync(admin.Id,
                new Claim(ClaimTypes.Name, "Admin"));

            await manager.AddClaimAsync(admin.Id,
                new Claim(ClaimTypes.Role, "Admin"));

            context.SaveChanges();
        }
    }

    // AccountsController class is for managing user accounts remotely using WebAPI.
    // To this point, we provide the following WebAPIs：
    //   api/Accounts/GetUsers
    //   api/Accounts/GetUsersFullInfo
    //   api/Accounts/AddUser
    //   api/Accounts/RemoveUser
    //   api/Accounts/ChangePassword
    //
    // Note:
    //   These WebAPIs can only be invoked by an authorized user, and Admin role 
    //     is required to invoke:
    //        api/Accounts/GetUsers
    //        api/Accounts/GetUsersFullInfo
    //        api/Accounts/AddUser
    //        api/Accounts/RemoveUser
    // 
    [RoutePrefix("api/Accounts")]
    [Authorize]
    public class AccountsController : ApiController
    {
        // Get the globle install of iS3OAuthDbContext class.
        //
        iS3OAuthDbContext dbContext
        {
            get
            {
                return Request.GetOwinContext().Get<iS3OAuthDbContext>();
            }
        }

        [HttpGet]
        [Route("GetUsers")]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult GetUsers()
        {
            List<string> names = new List<string>();
            foreach (var user in dbContext.Users)
                names.Add(user.UserName);
            return Ok(names);
        }

        [HttpGet]
        [Route("GetUsersFullInfo")]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult GetUsersFullInfo()
        {
            return Ok(dbContext.Users);
        }

        [HttpPost]
        [Route("AddUser")]
        [Authorize(Roles = "Admin")]
        // Add a new user according to:
        //      UserName, Password, Role
        //
        public async Task<IHttpActionResult> AddUser(iS3User user)
        {
            if (user == null)
            {
                return BadRequest("Argument Null");
            }
            if (user.Password != user.ConfirmPassword)
            {
                return BadRequest("Password not consistent");
            }

            string password = user.Password;
            // Erase the password for safety.
            user.Password = null;
            user.ConfirmPassword = null;

            var userExists = await dbContext.Users.AnyAsync(c => c.UserName == user.UserName);

            if (userExists)
            {
                //var exist = await dbContext.Users.FirstAsync(c => c.UserName == user.UserName);
                return BadRequest("User already exists");
            }

            var manager = new iS3UserManager(
                new UserStore<iS3User>(dbContext));

            var result = await manager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            await manager.AddClaimAsync(user.Id,
                new Claim(ClaimTypes.Name, user.UserName));

            await manager.AddClaimAsync(user.Id,
                new Claim(ClaimTypes.Role, user.Role));

            // add a claim to Identity.Claims
            //   Claim.Type = iS3ClaimTypes.AuthorizedProjects,
            //   Claim.Value = user.AuthorizedProjects
            //
            await manager.AddClaimAsync(user.Id,
                new Claim(iS3ClaimTypes.AuthorizedProjects, user.AuthorizedProjects));

            await dbContext.SaveChangesAsync();

            string success = string.Format("User {0} created successfully.", user.UserName);

            return Ok(success);
        }

        [HttpPost]
        [Route("RemoveUser")]
        [Authorize(Roles = "Admin")]
        // Remove a new user according to:
        //      UserName
        // Note: This operation cannot be recovered.
        //
        public async Task<IHttpActionResult> RemoveUser(iS3User user)
        {
            if (user == null)
            {
                return BadRequest("Argument Null");
            }
            var result = await dbContext.Users.FirstOrDefaultAsync(c => c.UserName == user.UserName);

            if (result == null)
            {
                return BadRequest("User does not exists");
            }

            var userName = RequestContext.Principal.Identity.GetUserName();
            if (string.Compare(user.UserName, userName, true) == 0)
            {
                return BadRequest("Cannot remove self");
            }

            dbContext.Users.Remove(result);
            await dbContext.SaveChangesAsync();

            string success = string.Format("User {0} removed successfully.", user.UserName);
            return Ok(success);
        }

        [HttpPost]
        [Route("ChangePassword")]
        // Change password of current user, the following three passwords should be provided.
        //      OldPassword, Password, ConfirmPassword
        // Note: This operation cannot be recovered.
        //
        public async Task<IHttpActionResult> ChangePassword(iS3User user)
        {
            if (user == null)
            {
                return BadRequest("Argument Null");
            }
            if (user.OldPassword == null || user.OldPassword.Length == 0)
            {
                return BadRequest("Old password could not be empty");
            }
            if (user.Password != user.ConfirmPassword)
            {
                return BadRequest("Password not consistent");
            }

            var userName = RequestContext.Principal.Identity.GetUserName();
            var userExists = await dbContext.Users.FirstAsync(c => c.UserName == userName);
            var userID = userExists.Id;

            var manager = Request.GetOwinContext().GetUserManager<iS3UserManager>();
            var result = await manager.ChangePasswordAsync(userID, user.OldPassword, user.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            await dbContext.SaveChangesAsync();
            return Ok("Password changed");
        }
    }
}
