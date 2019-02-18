using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;

namespace iS3.MiniServer
{
    public class iS3Territory : iS3Area
    {
        public iS3Territory(iS3AreaDesc desc) : base(desc) { }
    }

    public class iS3TerritoryDbContext : DbContext
    {
        public iS3TerritoryDbContext(string dbName) :
            base(dbName)
        {
            //Database.SetInitializer<iS3DbContext>(new CreateDatabaseIfNotExists<iS3DbContext>());
            //Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseIfModelChanges<iS3DbContext>());
            Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseAlways<iS3DbContext>());
        }

    }

    [RoutePrefix("api/Territory")]
    [Authorize(Roles = "Admin")]
    public class TerritoryController : ApiController
    {
        [HttpGet]
        [Route("TerritoryAPI")]
        public async Task<IHttpActionResult> TerritoryAPI(string tID)
        {
            iS3TerritoryDesc tDesc = await TerritoriesController.getTerritoryDesc(tID, null);

            using (var ctx = new iS3TerritoryDbContext(tDesc.DbName))
            {

            }

            return Ok("TerritoryAPI");
        }
    }

}
