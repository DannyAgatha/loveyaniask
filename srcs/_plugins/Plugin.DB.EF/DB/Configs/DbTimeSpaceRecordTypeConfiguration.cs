using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities.ServerData;

namespace Plugin.Database.DB.Configs
{
    public class DbTimeSpaceRecordTypeConfiguration : IEntityTypeConfiguration<DbTimeSpaceRecord>
    {
        public void Configure(EntityTypeBuilder<DbTimeSpaceRecord> builder)
        {
        }
    }
}