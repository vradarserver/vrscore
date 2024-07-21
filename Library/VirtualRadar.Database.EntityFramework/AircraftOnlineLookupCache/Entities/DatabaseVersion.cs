using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualRadar.Database.EntityFramework.AircraftOnlineLookupCache.Entities
{
    class DatabaseVersion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DatabaseVersionId { get; set; }

        public int Version { get; set; }
    }
}
