using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Terrun.Domain.Platform;

namespace Terrun.Infrastructure.Persistence.Configurations;

internal sealed class JobRunConfiguration : IEntityTypeConfiguration<JobRun>
{
    public void Configure(EntityTypeBuilder<JobRun> builder)
    {
        builder.ToTable("job_run");
        builder.HasKey(j => j.Name);
        builder.Property(j => j.Name).HasMaxLength(60);
    }
}
