using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plugin.Database.Entities;

namespace Plugin.Database.DB.Configs
{
    public abstract class BaseAuditableEntityTypeConfiguration<T> : IEntityTypeConfiguration<T>
    where T : class, IAuditableEntity
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasQueryFilter(s => s.DeletedAt == null);
            ConfigureEntity(builder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
    }
}