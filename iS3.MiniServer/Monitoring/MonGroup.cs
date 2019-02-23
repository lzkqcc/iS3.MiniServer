using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iS3.MiniServer.Monitoring
{
    [Table("Monitoring_MonGroup")]
    public class MonGroup:iS3DGObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? MonGroupType { get; set; }

        public int? MonProjectID { get; set; }
        public int? RefObjID { get; set; }
        public string MonPointIDs { get; set; }

        public string RefSpecifications { get; set; }

        public int? PerInfoID { get; set; }
        public string FileIDs { get; set; }
        public string Remark { get; set; }


        //ignore
        [NotMapped]
        public List<MonPoint> MonPoints { get; set; } = new List<MonPoint>();

        [NotMapped]
        public string GroupShape { get; set; }
        [NotMapped]
        public string GroupDire { get; set; }

    }
}
