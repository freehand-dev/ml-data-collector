using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ml_data_collector.Models
{
    [Table("MlDataCollector")]
    public class MlDataCollector
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        [StringLength(50)]
        public String TenderId { get; set; }

        [StringLength(50)]
        public String ProzorroHash { get; set; }

        public Boolean IsGood { get; set; }

    }
}
