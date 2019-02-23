using iS3.MiniServer.UserAuthority;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iS3.MiniServer
{
    /*User-Authority Domain
     * User-Role-Authority
    */
    public class iS3UserAuthority:iS3Domain
    {
        public iS3UserAuthority(iS3DomainHandle handle) : base(handle)
        {}
    }
    public class iS3UserAuthorityDbContext : DbContext
    {
        public iS3UserAuthorityDbContext(string dbName):
            base(dbName)
        {
            Database.SetInitializer<iS3MainDbContext>(new DropCreateDatabaseAlways<iS3MainDbContext>());
        }
        public DbSet<iS3UserInfo> iS3UserInfo { get; set; }
    }
}
