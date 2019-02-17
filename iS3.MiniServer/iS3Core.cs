using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;
using System.ComponentModel.DataAnnotations;

namespace iS3.MiniServer
{
    public static class MiniServer
    {
        public static ICollection<string> GetSubClasses<T>()
        {
            IEnumerable<Type> subclasses =
                   from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from type in assembly.GetTypes()
                   where type.IsSubclassOf(typeof(T))
                   select type;
            List<string> result = new List<string>();
            foreach (var x in subclasses)
                result.Add(x.ToString());

            return result;
        }
    }

    public abstract class iS3Area
    {
        // ID of the object
        public string ID { get; set; }
        // Name of the object
        public string Name { get; set; }
        // Type of the object
        public string Type { get; set; }

        // Indicates if the object is the default
        public bool Default { get; set; }

        // Parent area ID
        public string ParentID { get; set; }
    }

    public class iS3RolesInArea
    {
        public string UserName { get; set; }
        public string AreaName { get; set; }
        public string Roles { get; set; }
    }

    public class iS3Territory : iS3Area
    {
        //[Key]
        //public int iS3TerritoryID { get; set; }

        public ICollection<iS3Domain> Domains { get; set; }
        public ICollection<iS3Project> Projects { get; set; }

        public iS3Territory()
        {
            Type = GetType().ToString();
            Domains = new List<iS3Domain>();
            Projects = new List<iS3Project>();
        }

        public iS3Domain GetDomain(string NameOrID)
        {
            iS3Domain result = null;
            bool exist = Domains.Any(c => c.ID == NameOrID);
            if (exist)
            {
                result = Domains.First(c => c.ID == NameOrID);
            }
            else
            {
                exist = Domains.Any(c => c.Name == NameOrID);
                if (exist)
                {
                    result = Domains.First(c => c.Name == NameOrID);
                }
            }
            return result;
        }
    }

    public class iS3Domain : iS3Area
    {
        //[Key]
        ////public int iS3DomainID { get; set; }

        public iS3Domain()
        {
            Type = GetType().ToString();
        }
    }

    public class iS3DomainGeology : iS3Domain { }

    public class iS3Project : iS3Area
    {
        //[Key]
        //public int iS3ProjectID { get; set; }

        public iS3Project()
        {
            Type = GetType().ToString();
        }
    }

    public class iS3DbContext : DbContext
    {
        public iS3DbContext() :
            base(iS3ServerConfig.DefaultDatabase)
        {
            //Database.SetInitializer<iS3DbContext>(new CreateDatabaseIfNotExists<iS3DbContext>());
            //Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseIfModelChanges<iS3DbContext>());
            Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseAlways<iS3DbContext>());
        }

        //public DbSet<iS3Domain> Domains { get; set; }
        public DbSet<iS3Territory> Territories { get; set; }
    }

    
    [RoutePrefix("api/Territories")]
    [Authorize(Roles = "Admin")]
    public class TerritoriesController : ApiController
    {
        // Get territory by NameOrID.
        // Note: If not found, it will try to return the territory which is the default,
        //     i.e., Default==true
        //
        internal async Task<iS3Territory> getTerritory(string NameOrID, iS3DbContext ctx)
        {
            iS3Territory result = null;
            bool exist = await ctx.Territories.AnyAsync(c => c.ID == NameOrID);
            if (exist)
            {
                result = await ctx.Territories.FirstAsync(c => c.ID == NameOrID);
                return result;
            }
            exist = await ctx.Territories.AnyAsync(c => c.Name == NameOrID);
            if (exist)
            {
                result = await ctx.Territories.FirstAsync(c => c.Name == NameOrID);
                return result;
            }
            exist = await ctx.Territories.AnyAsync(c => c.Default == true);
            if (exist)
            {
                result = await ctx.Territories.FirstAsync(c => c.Default == true);
                return result;
            }
            return null;
        }

        [HttpGet]
        [Route("GetAllTerritories")]
        public async Task<IHttpActionResult> GetAllTerritories()
        {
            using (var ctx = new iS3DbContext())
            {
                ICollection<iS3Territory> result = await ctx.Territories.ToListAsync();
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("GetTerritory")]
        public async Task<IHttpActionResult> GetTerritory(string NameOrID)
        {
            if (NameOrID == null)
            {
                return BadRequest("Argument Null");
            }

            iS3Territory result = null;
            using (var ctx = new iS3DbContext())
            {
                result = await getTerritory(NameOrID, ctx);
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("AddTerritory")]
        public async Task<IHttpActionResult> AddTerritory(iS3Territory territory)
        {
            if (territory == null)
            {
                return BadRequest("Argument Null");
            }

            using (var ctx = new iS3DbContext())
            {
                bool exists = await ctx.Territories.AnyAsync(c => c.Name == territory.Name);
                if (exists)
                {
                    return BadRequest("Already exists");
                }

                iS3Territory newTerritory = new iS3Territory();
                newTerritory.Name = territory.Name;
                newTerritory.ID = Guid.NewGuid().ToString();

                var result = ctx.Territories.Add(newTerritory);
                await ctx.SaveChangesAsync();

                return Ok(newTerritory);
            }
        }

        [HttpPost]
        [Route("AddDomain")]
        public async Task<IHttpActionResult> AddDomain(iS3Domain domain)
        {
            if (domain == null)
            {
                return BadRequest("Argument Null");
            }

            iS3Territory territory = null;
            iS3Domain newDomain = null;
            using (var ctx = new iS3DbContext())
            {
                territory = await getTerritory(domain.ParentID, ctx);
                if (territory == null)
                {
                    return BadRequest("Territory null and no default");
                }

                bool exist = territory.Domains.Any(c => c.Name == domain.Name);
                if (exist)
                {
                    return BadRequest("Domain exist");
                }

                newDomain = new iS3Domain();
                newDomain.Name = domain.Name;
                newDomain.ID = Guid.NewGuid().ToString();
                newDomain.ParentID = territory.ID;

                territory.Domains.Add(newDomain);
                await ctx.SaveChangesAsync();
            }

            // Check the domains can be loaded into the territory
            //
            //using (var ctx = new iS3DbContext())
            //{
            //    territory = await getTerritory(domain.ParentID, ctx);

            //    var entry = ctx.Entry(territory).Collection(t => t.Domains);
            //    var isLoaded = entry.IsLoaded;
            //    await entry.LoadAsync();
            //    isLoaded = entry.IsLoaded;

            //    var domains = territory.Domains;
            //}

            return Ok(newDomain);
        }


        [HttpPost]
        [Route("GetDomain")]
        public async Task<IHttpActionResult> GetDomain(iS3Domain domain)
        {
            if (domain == null)
            {
                return BadRequest("Argument Null");
            }

            iS3Territory territory = null;
            iS3Domain result = null;
            using (var ctx = new iS3DbContext())
            {
                territory = await getTerritory(domain.ParentID, ctx);
                if (territory == null)
                {
                    return BadRequest("Territory null and no default");
                }

                // explicit load domains
                //
                var entry = ctx.Entry(territory).Collection(t => t.Domains);
                var isLoaded = entry.IsLoaded;
                await entry.LoadAsync();
                isLoaded = entry.IsLoaded;

                result = territory.GetDomain(domain.ID);
                if (result == null)
                    result = territory.GetDomain(domain.Name);
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("SupportedDomains")]
        public ICollection<string> SupportedDomains()
        {
            ICollection<string> result = MiniServer.GetSubClasses<iS3Domain>();
            return result;
        }

        [HttpGet]
        [Route("SupportedProjects")]
        public ICollection<string> SupportedProjects()
        {
            ICollection<string> result = MiniServer.GetSubClasses<iS3Project>();
            return result;
        }

    }

}
