using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iS3.MiniServer.Monitoring
{
    [Table("Monitoring_MonProject")]
    public class MonProject:iS3DGObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MonProjectType { get; set; }

        public int? RefObjID { get; set; }
        public string MonGroupIDs { get; set; }

        public int? CompanyInfoID { get; set; }
        public int? PerInfoID { get; set; }
        public string MonInstInfoIDs { get; set; }
        public string FileIDs { get; set; }
        public string Remark { get; set; }

        //ignore
        [NotMapped]
        public List<MonGroup> MonGroups { get; set; } = new List<MonGroup>();

    }
}
