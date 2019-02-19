using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;

namespace iS3.MiniServer
{
    public class iS3SimpleTerritory : iS3Territory
    {
        public iS3SimpleTerritory(iS3TerritoryDesc desc) : base(desc)
        {
        }


    }

    public class iS3TerritoryDbContext : DbContext
    {
        public iS3TerritoryDbContext(string dbName) :
            base(dbName)
        {
            //Database.SetInitializer<iS3TerritoryDbContext>(new CreateDatabaseIfNotExists<iS3TerritoryDbContext>());
            //Database.SetInitializer<iS3TerritoryDbContext>(new DropCreateDatabaseIfModelChanges<iS3TerritoryDbContext>());
            Database.SetInitializer<iS3TerritoryDbContext>(new DropCreateDatabaseAlways<iS3TerritoryDbContext>());
        }

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

        [HttpGet]
        [Route("GetAllTerritoryDescs")]
        public async Task<IHttpActionResult> GetAllTerritoryDescs()
        {
            ICollection<iS3TerritoryDesc> result = null;
            result = await MiniServer.GetAllTerritoryDescs();
            return Ok(result);
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
            result = await MiniServer.getTerritoryDesc(NameOrID);
            return Ok(result);
        }


        [HttpPost]
        [Route("AddTerritory")]
        public async Task<IHttpActionResult> AddTerritory(iS3TerritoryDesc desc)
        {
            if (desc == null)
            {
                return BadRequest("Argument Null");
            }

            await MiniServer.AddTerritory(desc);

            return Ok();
        }

        [HttpPost]
        [Route("AddDomain")]
        public async Task<IHttpActionResult> AddDomain(iS3DomainDesc desc)
        {
            if (desc == null)
            {
                return BadRequest("Argument Null");
            }

            await MiniServer.AddDomain(desc);       

            return Ok();
        }



        //[HttpPost]
        //[Route("GetDomainDesc")]
        //public async Task<IHttpActionResult> GetDomainDesc1(iS3DomainDesc domain)
        //{
        //    if (domain == null)
        //    {
        //        return BadRequest("Argument Null");
        //    }

        //    iS3TerritoryDesc territory = null;
        //    iS3DomainDesc result = null;
        //    using (var ctx = new iS3MainDbContext())
        //    {
        //        territory = await getTerritoryDesc(domain.ParentID, ctx);
        //        if (territory == null)
        //        {
        //            return BadRequest("Territory null and no default");
        //        }

        //        // explicit load domains
        //        //
        //        var entry = ctx.Entry(territory).Collection(t => t.DomainDescs);
        //        var isLoaded = entry.IsLoaded;
        //        await entry.LoadAsync();
        //        isLoaded = entry.IsLoaded;

        //        result = territory.GetDomainDesc(domain.ID);
        //        if (result == null)
        //            result = territory.GetDomainDesc(domain.Name);
        //    }

        //    return Ok(result);
        //}
    }


}
