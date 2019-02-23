using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iS3.MiniServer.Monitoring
{
    [Table("Monitoring_MonPoint")]
    public class MonPoint:iS3DGObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<int> MonPointType { get; set; }

        public Nullable<int> MonGroupID { get; set; }

        public Nullable<int> DACID { get; set; }
        public string SensorName { get; set; }
        public string Component { get; set; }

        public string Unit { get; set; }

        public Nullable<decimal> XCoordinate { get; set; }
        public Nullable<decimal> YCoordinate { get; set; }
        public Nullable<decimal> ZCoordinate { get; set; }


        public Nullable<decimal> IniValue { get; set; }
        public Nullable<System.DateTime> STime { get; set; }
        public Nullable<int> PerInfoID { get; set; }
        public string FileIDs { get; set; }
        public string Remark { get; set; }

        //ignore
        [NotMapped]
        public List<MonData> monDatas { get; set; } = new List<MonData>();
        [NotMapped]
        public double LocalDist { get; set; }

        [NotMapped]
        public List<string> ComponentList
        {
            get
            {
                if (Component == null) { return new List<string>(); }
                else
                {
                    return Component.Split(',').ToList();
                }
            }
        }
    }
}
