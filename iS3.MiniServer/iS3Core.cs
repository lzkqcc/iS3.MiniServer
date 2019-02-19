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
        public iS3AreaDesc AreaDesc { get; set; }
        public iS3Area (iS3AreaDesc desc)
        {
            AreaDesc = desc;
        }
    }

    public class iS3Domain : iS3Area
    {
        public iS3DomainDesc DomainDesc { get; set; }
        public iS3Domain(iS3DomainDesc desc) : base(desc)
        {
            DomainDesc = desc;
        }
    }

    public class iS3Project : iS3Area
    {
        public iS3ProjectDesc ProjectDesc { get; set; }
        public iS3Project(iS3ProjectDesc desc) : base(desc)
        {
            ProjectDesc = desc;
        }
    }

    public class iS3Territory : iS3Area
    {
        public iS3TerritoryDesc TerritoryDesc { get; set; }
        public iS3Territory(iS3TerritoryDesc desc) : base(desc)
        {
            TerritoryDesc = desc;
        }
    }

    public class iS3RolesInArea
    {
        public string UserName { get; set; }
        public string AreaName { get; set; }
        public string Roles { get; set; }
    }

    // Main database of the server context
    //
    public class iS3MainDbContext : DbContext
    {
        public iS3MainDbContext() :
            base(MiniServer.DefaultDatabase)
        {
            //Database.SetInitializer<iS3MainDbContext>(new CreateDatabaseIfNotExists<iS3MainDbContext>());
            //Database.SetInitializer<iS3MainDbContext>(new DropCreateDatabaseIfModelChanges<iS3MainDbContext>());
            Database.SetInitializer<iS3MainDbContext>(new DropCreateDatabaseAlways<iS3MainDbContext>());
        }

        //public DbSet<iS3Domain> Domains { get; set; }
        public DbSet<iS3TerritoryDesc> TerritoryDescs { get; set; }
    }

    // MiniServer: OS of the server
    //  1. It operates on the default database to store territories, domains and projects.
    //  2. It maintains the relationship between territory and its domains and projects.
    //
    public static class MiniServer
    {
        // Default database name
        //
        public const string DefaultDatabase = "iS3Db";

        // Get subclasses of specified type T
        //
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

        // Get all territory descriptions.
        // In other words, get all territories that are hosted on the server.
        //
        public static async Task<ICollection<iS3TerritoryDesc>> GetAllTerritoryDescs()
        {
            ICollection<iS3TerritoryDesc> result = null;
            using (var ctx = new iS3MainDbContext())
            {
                result = await ctx.TerritoryDescs.ToListAsync();
            }
            return result;
        }

        // Get territory description.
        // Note:
        //     1. If not found, it will try to return the territory which is the default,
        //        i.e., iS3TerritoryDesc.Default==true
        //     2. if ctx is null, default context, i.e., the default database will be used. 
        // 
        public static async Task<iS3TerritoryDesc> getTerritoryDesc(string nameOrID,
            iS3MainDbContext ctx = null)
        {
            if (ctx == null)
            {
                using (var ctx_new = new iS3MainDbContext())
                {
                    return await getTerritoryDescInternal(nameOrID, ctx_new);
                }
            }
            else
            {
                return await getTerritoryDescInternal(nameOrID, ctx);
            }
        }

        static async Task<iS3TerritoryDesc> getTerritoryDescInternal(string nameOrID, iS3MainDbContext ctx)
        {
            iS3TerritoryDesc result = null;
            bool exist = await ctx.TerritoryDescs.AnyAsync(c => c.ID == nameOrID);
            if (exist)
            {
                result = await ctx.TerritoryDescs.FirstAsync(c => c.ID == nameOrID);
                return result;
            }
            exist = await ctx.TerritoryDescs.AnyAsync(c => c.Name == nameOrID);
            if (exist)
            {
                result = await ctx.TerritoryDescs.FirstAsync(c => c.Name == nameOrID);
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

        // Add a new territory description:
        //   name,  should be filled
        //   if type is not give, i.e., null, default is iS3SimpleTerritory
        //   if dbName is not given, i.e., null, default database name will be used.
        //
        public static async Task<iS3TerritoryDesc> AddTerritoryDesc(string name, string type = null, string dbName = null)
        {
            if (name == null)
            {
                throw new Exception("Argument Null");
            }
            if (type == null)
                type = typeof(iS3SimpleTerritory).ToString();

            using (var ctx = new iS3MainDbContext())
            {
                bool exists = await ctx.TerritoryDescs.AnyAsync(c => c.Name == name);
                if (exists)
                {
                    throw new Exception("Already exists");
                }

                iS3TerritoryDesc newTerritoryDesc = new iS3TerritoryDesc();
                newTerritoryDesc.ID = Guid.NewGuid().ToString();
                newTerritoryDesc.Name = name;
                newTerritoryDesc.Type = type;
                if (dbName == null)
                    newTerritoryDesc.DbName = MiniServer.DefaultDatabase;
                else
                    newTerritoryDesc.DbName = dbName;

                var result = ctx.TerritoryDescs.Add(newTerritoryDesc);
                await ctx.SaveChangesAsync();

                return newTerritoryDesc;
            }
        }


        // Add a new domain description:
        //   name, type, parentNameOrID  should be filled
        //   if dbName is not given, i.e., null, default database name will be used.
        //
        public static async Task<iS3DomainDesc> AddDomainDesc(string name, string type, string parentNameOrID, string dbName = null)
        {
            iS3TerritoryDesc territoryDesc = null;

            using (var ctx = new iS3MainDbContext())
            {
                territoryDesc = await getTerritoryDesc(parentNameOrID, ctx);
                if (territoryDesc == null)
                {
                    throw new Exception("Territory null and no default");
                }

                bool exist = territoryDesc.DomainDescs.Any(c => c.Name == name);
                if (exist)
                {
                    throw new Exception("Already exists");
                }

                iS3DomainDesc domainDesc = new iS3DomainDesc();
                domainDesc.ID = Guid.NewGuid().ToString();
                domainDesc.Name = name;
                domainDesc.Type = type;
                domainDesc.ParentID = territoryDesc.ID;

                if (domainDesc.DbName == null)
                {
                    if (territoryDesc.DbName != null)
                        domainDesc.DbName = territoryDesc.DbName;
                    else
                        domainDesc.DbName = MiniServer.DefaultDatabase;
                }

                territoryDesc.DomainDescs.Add(domainDesc);
                await ctx.SaveChangesAsync();

                return domainDesc;
            }
        }

        // Add a new domain description:
        //   The iS3DominDesc.Name and iS3DominDesc.ParentID should be filled.
        //   The iS3DominDesc.ParentID could be ID or Name of iS3TerritoryDesc
        //
        //public static async Task AddDomainDesc(iS3DomainDesc domainDesc)
        //{
        //    if (domainDesc == null || domainDesc.Name == null || domainDesc.ParentID == null)
        //    {
        //        throw new Exception("Argument Null");
        //    }

        //    iS3TerritoryDesc territoryDesc = null;

        //    using (var ctx = new iS3MainDbContext())
        //    {
        //        territoryDesc = await getTerritoryDesc(domainDesc.ParentID, ctx);
        //        if (territoryDesc == null)
        //        {
        //            throw new Exception("Territory null and no default");
        //        }

        //        bool exist = territoryDesc.DomainDescs.Any(c => c.Name == domainDesc.Name);
        //        if (exist)
        //        {
        //            throw new Exception("Already exists");
        //        }

        //        domainDesc.ID = Guid.NewGuid().ToString();
        //        domainDesc.ParentID = territoryDesc.ID;

        //        if (domainDesc.DbName == null)
        //        {
        //            if (territoryDesc.DbName != null)
        //                domainDesc.DbName = territoryDesc.DbName;
        //            else
        //                domainDesc.DbName = MiniServer.DefaultDatabase;
        //        }

        //        territoryDesc.DomainDescs.Add(domainDesc);
        //        await ctx.SaveChangesAsync();
        //    }

        //    // Check the domains can be loaded into the territory
        //    //
        //    //using (var ctx = new iS3DbContext())
        //    //{
        //    //    territory = await getTerritory(domain.ParentID, ctx);

        //    //    var entry = ctx.Entry(territory).Collection(t => t.Domains);
        //    //    var isLoaded = entry.IsLoaded;
        //    //    await entry.LoadAsync();
        //    //    isLoaded = entry.IsLoaded;

        //    //    var domains = territory.Domains;
        //    //}
        //}

        // Get domain description:
        //   NameOrID: should be filled
        //   ParentNameOrID: if not specified, the default Territory will be assumed
        //
        public static async Task<iS3DomainDesc> GetDomainDesc(string nameOrID, string parentNameOrID)
        {
            if (nameOrID == null)
            {
                return null;
            }

            iS3TerritoryDesc territory = null;
            iS3DomainDesc result = null;
            using (var ctx = new iS3MainDbContext())
            {
                territory = await getTerritoryDesc(parentNameOrID, ctx);
                if (territory == null)
                {
                    return null;
                }

                // explicit load domains
                //
                var entry = ctx.Entry(territory).Collection(t => t.DomainDescs);
                var isLoaded = entry.IsLoaded;
                await entry.LoadAsync();
                isLoaded = entry.IsLoaded;

                result = territory.GetDomainDesc(nameOrID);
            }

            return result;
        }

    }



}
