using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;

namespace iS3.MiniServer
{
    public class iS3Geology : iS3Domain
    {
        public iS3Geology(iS3DomainHandle handle) : base(handle)
        { }
    }

    public class iS3GeologyDbContext : DbContext
    {
        public iS3GeologyDbContext(string dbName) :
            base(dbName)
        {
            //Database.SetInitializer<iS3DbContext>(new CreateDatabaseIfNotExists<iS3DbContext>());
            //Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseIfModelChanges<iS3DbContext>());
            Database.SetInitializer<iS3MainDbContext>(new DropCreateDatabaseAlways<iS3MainDbContext>());
        }

        public DbSet<iS3TerritoryHandle> Territories { get; set; }
    }

    [RoutePrefix("api/Geology")]
    [Authorize(Roles = "Admin")]
    public class GeologyController : ApiController
    {

    }
}
