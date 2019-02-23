using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;
using iS3.MiniServer.Monitoring;
using iS3.MiniServer.Help;

namespace iS3.MiniServer
{
    public class iS3Monitoring : iS3Domain
    {
        public iS3Monitoring(iS3DomainHandle handle) : base(handle)
        { }
    }

    public class iS3MonitoringDbContext : DbContext
    {
        public iS3MonitoringDbContext(string dbName) :
            base(dbName)
        {
            //Database.SetInitializer<iS3DbContext>(new CreateDatabaseIfNotExists<iS3DbContext>());
            //Database.SetInitializer<iS3DbContext>(new DropCreateDatabaseIfModelChanges<iS3DbContext>());
            Database.SetInitializer<iS3MainDbContext>(new DropCreateDatabaseAlways<iS3MainDbContext>());
        }
        public DbSet<MonProject> MonProject { get; set; }
        public DbSet<MonGroup> MonGroup { get; set; }
        public DbSet<MonPoint> MonPoint { get; set; }
        public DbSet<MonData> MonData { get; set; }
    }

    [RoutePrefix("api/Monitoring")]
    [Authorize(Roles = "Admin")]
    public class MonitoringController : ApiController
    {
        iS3MonitoringDbContext context = new iS3MonitoringDbContext("iS3Db");
        #region MonProject Operation
        [HttpGet]
        [Route("GetAllMonProjects")]
        public async Task<IHttpActionResult> GetAllMonProjects()
        {
            var result = context.MonProject.ToList();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMonProjectByID")]
        public async Task<IHttpActionResult> GetMonProjectByID(int id)
        {
            var result = context.MonProject.Where(x => x.ID == id);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddMonProject")]
        public async Task<IHttpActionResult> AddMonProject([FromBody] MonProject model)
        {
            var result= context.MonProject.Add(model);
            context.SaveChanges();
            return Ok(result);
        }

        [HttpPut]
        [Route("ModifyMonProject")]
        public async Task<IHttpActionResult> ModifyMonProject([FromBody] MonProject model)
        {
            MonProject obj = context.Set<MonProject>().Find((model.ID));
            UpdateModelHelp.Update(model, obj);
            context.Entry(obj).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
            return Ok(obj);
        }
        [HttpDelete]
        [Route("RemoveMonProject")]
        public async Task<IHttpActionResult> RemoveMonProject(int id)
        {
            MonProject obj = context.Set<MonProject>().Find(id);
            context.Entry(obj).State = System.Data.Entity.EntityState.Deleted;
            context.SaveChanges();
            return Ok(obj);
        }
        #endregion
        #region MonGroup Operation
        [HttpGet]
        [Route("GetAllMonGroups")]
        public async Task<IHttpActionResult> GetAllMonGroups()
        {
            var result = context.MonGroup.ToList();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMonGroupByID")]
        public async Task<IHttpActionResult> GetMonGroupByID(int id)
        {
            var result = context.MonGroup.Where(x => x.ID == id);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddMonGroup")]
        public async Task<IHttpActionResult> AddMonGroup([FromBody] MonGroup model)
        {
            var result = context.MonGroup.Add(model);
            context.SaveChanges();
            return Ok(result);
        }

        [HttpPut]
        [Route("ModifyMonGroup")]
        public async Task<IHttpActionResult> ModifyMonGroup([FromBody] MonGroup model)
        {
            MonGroup obj = context.Set<MonGroup>().Find((model.ID));
            UpdateModelHelp.Update(model, obj);
            context.Entry(obj).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
            return Ok(obj);
        }
        [HttpDelete]
        [Route("RemoveMonGroup")]
        public async Task<IHttpActionResult> RemoveMonGroup(int id)
        {
            MonGroup obj = context.Set<MonGroup>().Find(id);
            context.Entry(obj).State = System.Data.Entity.EntityState.Deleted;
            context.SaveChanges();
            return Ok(obj);
        }
        #endregion
        #region MonPoint Operation
        [HttpGet]
        [Route("GetAllMonPoints")]
        public async Task<IHttpActionResult> GetAllMonPoints()
        {
            var result = context.MonPoint.ToList();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMonPointByID")]
        public async Task<IHttpActionResult> GetMonPointByID(int id)
        {
            var result = context.MonPoint.Where(x => x.ID == id);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddMonPoint")]
        public async Task<IHttpActionResult> AddMonPoint([FromBody] MonPoint model)
        {
            var result = context.MonPoint.Add(model);
            context.SaveChanges();
            return Ok(result);
        }

        [HttpPut]
        [Route("ModifyMonPoint")]
        public async Task<IHttpActionResult> ModifyMonPoint([FromBody] MonPoint model)
        {
            MonPoint obj = context.Set<MonPoint>().Find((model.ID));
            UpdateModelHelp.Update(model, obj);
            context.Entry(obj).State = System.Data.Entity.EntityState.Modified;
            context.SaveChanges();
            return Ok(obj);
        }
        [HttpDelete]
        [Route("RemoveMonPoint")]
        public async Task<IHttpActionResult> RemoveMonPoint(int id)
        {
            MonPoint obj = context.Set<MonPoint>().Find(id);
            context.Entry(obj).State = System.Data.Entity.EntityState.Deleted;
            context.SaveChanges();
            return Ok(obj);
        }
        #endregion
    }
}
