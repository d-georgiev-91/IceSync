using IceSync.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceSync.Data.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.MultiExecBehavior)
            .IsRequired()
            .HasMaxLength(8);
    }
}