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
        public const string DefaultDatabase = "iS3Db";
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

    // iS3AreaDesc: iS3Area Description
    // 
    public class iS3AreaDesc
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

        // Database name
        public string DbName { get; set; }
    }

    // iS3DomainDesc: iS3Domain Description
    //
    public class iS3DomainDesc : iS3AreaDesc
    {
        //[Key]
        ////public int iS3DomainID { get; set; }

        public iS3DomainDesc()
        {
            Type = GetType().ToString();
        }
    }

    // iS3ProjectDesc: iS3Project Description
    //
    public class iS3ProjectDesc : iS3AreaDesc
    {
        //[Key]
        //public int iS3ProjectID { get; set; }

        public iS3ProjectDesc()
        {
            Type = GetType().ToString();
        }
    }

    // iS3TerritoryDesc: iS3Territory Description
    //
    public class iS3TerritoryDesc : iS3AreaDesc
    {
        //[Key]
        //public int iS3TerritoryID { get; set; }

        public ICollection<iS3DomainDesc> DomainDescs { get; set; }
        public ICollection<iS3ProjectDesc> ProjectDescs { get; set; }

        public iS3TerritoryDesc()
        {
            Type = GetType().ToString();
            DomainDescs = new List<iS3DomainDesc>();
            ProjectDescs = new List<iS3ProjectDesc>();
        }

        public iS3DomainDesc GetDomainDesc(string NameOrID)
        {
            iS3DomainDesc result = null;
            bool exist = DomainDescs.Any(c => c.ID == NameOrID);
            if (exist)
            {
                result = DomainDescs.First(c => c.ID == NameOrID);
            }
            else
            {
                exist = DomainDescs.Any(c => c.Name == NameOrID);
                if (exist)
                {
                    result = DomainDescs.First(c => c.Name == NameOrID);
                }
            }
            return result;
        }
    }

    public class iS3Area
    {
        public iS3AreaDesc Desc { get; set; }
        public iS3Area (iS3AreaDesc desc)
        {
            Desc = desc;
        }
    }

    public class iS3Domain : iS3Area
    {
        public iS3Domain(iS3AreaDesc desc) : base(desc)
        {

        }
    }

    public class iS3Project : iS3Area
    {
        public iS3Project(iS3AreaDesc desc) : base(desc) { }
    }


    public class iS3RolesInArea
    {
        public string UserName { get; set; }
        public string AreaName { get; set; }
        public string Roles { get; set; }
    }

    public class iS3DbContext : DbContext
    {
        public iS3DbContext() :
            base(MiniServer.DefaultDatabase)
        {
            //Database.SetInitializer<iS3DbContext>(new CreateDatabaseIfNotExists<iS3DbContext>());
            //Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseIfModelChanges<iS3DbContext>());
            Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseAlways<iS3DbContext>());
        }

        //public DbSet<iS3Domain> Domains { get; set; }
        public DbSet<iS3TerritoryDesc> TerritoryDescs { get; set; }
    }

    
    [RoutePrefix("api/Territories")]
    [Authorize(Roles = "Admin")]
    public class TerritoriesController : ApiController
    {
        [HttpGet]
        [Route("SupportedTerritories")]
        public ICollection<string> SupportedTerritories()
        {
            ICollection<string> result = MiniServer.GetSubClasses<iS3Territory>();
            return result;
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

        // Get territory by NameOrID.
        // Note: If not found, it will try to return the territory which is the default,
        //     i.e., Default==true
        //
        public static async Task<iS3TerritoryDesc> getTerritoryDesc(string NameOrID,
            iS3DbContext ctx)
        {
            if (ctx == null)
            {
                using (var ctx_new = new iS3DbContext())
                {
                    return await getTerritoryDescInternal(NameOrID, ctx_new);
                }
            }
            else
            {
                return await getTerritoryDescInternal(NameOrID, ctx);
            }
        }

        static async Task<iS3TerritoryDesc> getTerritoryDescInternal(string NameOrID, iS3DbContext ctx)
        {
            iS3TerritoryDesc result = null;
            bool exist = await ctx.TerritoryDescs.AnyAsync(c => c.ID == NameOrID);
            if (exist)
            {
                result = await ctx.TerritoryDescs.FirstAsync(c => c.ID == NameOrID);
                return result;
            }
            exist = await ctx.TerritoryDescs.AnyAsync(c => c.Name == NameOrID);
            if (exist)
            {
                result = await ctx.TerritoryDescs.FirstAsync(c => c.Name == NameOrID);
                return result;
            }
            exist = await ctx.TerritoryDescs.AnyAsync(c => c.Default == true);
            if (exist)
            {
                result = await ctx.TerritoryDescs.FirstAsync(c => c.Default == true);
                return result;
            }
            return null;
        }

        [HttpPost]
        [Route("AddTerritory")]
        public async Task<IHttpActionResult> AddTerritory(iS3TerritoryDesc territoryDesc)
        {
            if (territoryDesc == null)
            {
                return BadRequest("Argument Null");
            }

            using (var ctx = new iS3DbContext())
            {
                bool exists = await ctx.TerritoryDescs.AnyAsync(c => c.Name == territoryDesc.Name);
                if (exists)
                {
                    return BadRequest("Already exists");
                }

                iS3TerritoryDesc newTerritoryDesc = new iS3TerritoryDesc();
                newTerritoryDesc.ID = Guid.NewGuid().ToString();
                newTerritoryDesc.Name = territoryDesc.Name;
                newTerritoryDesc.DbName = territoryDesc.DbName;

                var result = ctx.TerritoryDescs.Add(newTerritoryDesc);
                await ctx.SaveChangesAsync();

                return Ok(newTerritoryDesc);
            }
        }

        [HttpGet]
        [Route("GetTerritoryDesc")]
        public async Task<IHttpActionResult> GetTerritoryDesc(string NameOrID)
        {
            if (NameOrID == null)
            {
                return BadRequest("Argument Null");
            }

            iS3TerritoryDesc result = null;
            using (var ctx = new iS3DbContext())
            {
                result = await getTerritoryDesc(NameOrID, ctx);
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("GetAllTerritoryDescs")]
        public async Task<IHttpActionResult> GetAllTerritoryDescs()
        {
            using (var ctx = new iS3DbContext())
            {
                ICollection<iS3TerritoryDesc> result = await ctx.TerritoryDescs.ToListAsync();
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("AddDomain")]
        public async Task<IHttpActionResult> AddDomain(iS3DomainDesc domainDesc)
        {
            if (domainDesc == null)
            {
                return BadRequest("Argument Null");
            }

            iS3TerritoryDesc territoryDesc = null;
            iS3DomainDesc newDomainDesc = null;

            using (var ctx = new iS3DbContext())
            {
                territoryDesc = await getTerritoryDesc(domainDesc.ParentID, ctx);
                if (territoryDesc == null)
                {
                    return BadRequest("Territory null and no default");
                }

                bool exist = territoryDesc.DomainDescs.Any(c => c.Name == domainDesc.Name);
                if (exist)
                {
                    return BadRequest("Domain exist");
                }

                newDomainDesc = new iS3DomainDesc();
                newDomainDesc.ID = Guid.NewGuid().ToString();
                newDomainDesc.ParentID = territoryDesc.ID;
                newDomainDesc.Name = domainDesc.Name;

                if (domainDesc.DbName != null)
                    newDomainDesc.DbName = domainDesc.DbName;
                else if (territoryDesc.DbName != null)
                    newDomainDesc.DbName = territoryDesc.DbName;
                else
                    newDomainDesc.DbName = MiniServer.DefaultDatabase;

                territoryDesc.DomainDescs.Add(newDomainDesc);
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

            return Ok(newDomainDesc);
        }


        [HttpPost]
        [Route("GetDomainDesc")]
        public async Task<IHttpActionResult> GetDomainDesc(iS3DomainDesc domain)
        {
            if (domain == null)
            {
                return BadRequest("Argument Null");
            }

            iS3TerritoryDesc territory = null;
            iS3DomainDesc result = null;
            using (var ctx = new iS3DbContext())
            {
                territory = await getTerritoryDesc(domain.ParentID, ctx);
                if (territory == null)
                {
                    return BadRequest("Territory null and no default");
                }

                // explicit load domains
                //
                var entry = ctx.Entry(territory).Collection(t => t.DomainDescs);
                var isLoaded = entry.IsLoaded;
                await entry.LoadAsync();
                isLoaded = entry.IsLoaded;

                result = territory.GetDomainDesc(domain.ID);
                if (result == null)
                    result = territory.GetDomainDesc(domain.Name);
            }

            return Ok(result);
        }
    }

}
