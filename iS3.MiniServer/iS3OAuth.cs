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

namespace iS3.MiniServer
{
    // iS3User class is for user authentication such as register and login.
    // iS3User class inherits from Microsoft.AspNet.IdentityUser.
    // 
    public class iS3User : IdentityUser
    {
        public iS3User() : base() { Password = ""; Role = ""; }

        // Password and Role is for admin to register new user from remote.
        // For example, we can register a new user
        //   UserName=john, Password=johnsPassword, Role=User
        // using following command remotely:
        //
        //   curl -d "Username=john&Password=johnsPassword&Role=User" 
        //        -H "Authorization:Bearer $token" http://$ip:$port/api/Accounts/Add
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
        public string Role { get; set; }
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
    //: DropCreateDatabaseIfModelChanges<iS3OAuthDbContext>
    : DropCreateDatabaseAlways<iS3OAuthDbContext>
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
    // To this point, we only provide the following WebAPIs：
    //   api/Accounts/GetUsers
    //   api/Accounts/AddUser
    //   api/Accounts/RemoveUser
    //
    // Note:
    //   This WebAPIs can only be invoked by a user with Admin role, see iS3User class
    //     for more detail on how to login as Admin.
    // 
    [Authorize(Roles = "Admin")]
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

        public IEnumerable<iS3User> GetUsers()
        {
            return dbContext.Users;
        }

        public async Task<IHttpActionResult> AddUser(iS3User user)
        {
            if (user == null)
            {
                return BadRequest("Argument Null");
            }
            var userExists = await dbContext.Users.AnyAsync(c => c.UserName == user.UserName);

            if (userExists)
            {
                return BadRequest("User already exists");
            }

            var manager = new iS3UserManager(
                new UserStore<iS3User>(dbContext));

            var result = await manager.CreateAsync(user, user.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            await manager.AddClaimAsync(user.Id,
                new Claim(ClaimTypes.Name, user.UserName));

            await manager.AddClaimAsync(user.Id,
                new Claim(ClaimTypes.Role, user.Role));

            await dbContext.SaveChangesAsync();

            string success = string.Format("User {0} created successfully.", user.UserName);

            return Ok(success);
        }

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

            dbContext.Users.Remove(result);
            await dbContext.SaveChangesAsync();

            string success = string.Format("User {0} removed successfully.", user);
            return Ok(success);
        }

    }
}


/*
 * The following commands can help your to test the above codes.
 * 
port=8080
curl http://localhost:$port/api/Accounts/GetUsers    [=>will be refused]
curl -d 'grant_type=password&username=Admin&password=iS3Admin' http://localhost:$port/Token    [=>will return token]
token1=$[token from above responses]
curl -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/GetUsers    [=>will succeeded, 1 user]
curl -d "Username=lxj&Password=lxjsPassword&Role=User" -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/AddUser
curl -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/GetUsers    [=>will succeeded, 2 users]
curl -d "Username=lxj&Password=" -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/RemoveUser

curl -d "Username=lxj&Password=lxjsPassword&Role=User" -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/AddUser
curl -d 'grant_type=password&Username=lxj&Password=lxjsPassword' http://localhost:$port/Token
token2=$[token from above responses]
curl -H "Authorization:Bearer $token2" http://localhost:$port/api/Accounts/GetUsers [=>will be denied because for insufficient authorization]

curl -H "Authorization:Bearer $token1" http://localhost:$port/api/Accounts/GetUsers [=>will succeeded, 2 users]
 *
 *
 */
