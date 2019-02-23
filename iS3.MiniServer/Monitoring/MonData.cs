using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iS3.MiniServer.Monitoring
{
    [Table("Monitoring_MonData")]
    public class MonData : iS3DGObject
    {
        public string SensorName { get; set; }
        public string Part { get; set; }

        public DateTime? AcqTime { get; set; }
        public DateTime? RecTime { get; set; }

        public decimal? Value { get; set; }
        public decimal? Data { get; set; }

        public decimal? CurrVariation { get; set; }
        public decimal? AccuVariation { get; set; }
        public decimal? VariationRate { get; set; }
        public decimal? ValuePerDesign { get; set; }

        public string Remark { get; set; }
    }
}
