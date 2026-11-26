using PersistentAgents.Local.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using AgentThread = PersistentAgents.Local.Persistence.Entities.Thread;

namespace PersistentAgents.Local.Persistence.Context;

public class AgentsDbContext : DbContext
{
    public AgentsDbContext(DbContextOptions<AgentsDbContext> options)
        : base(options)
    {
    }
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Chunk> Chunks => Set<Chunk>();
    public DbSet<AgentThread> Threads => Set<AgentThread>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Run> Runs => Set<Run>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AGENTS
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("AGENTS");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).IsRequired();

            entity.Property(e => e.Name).IsRequired();

            entity.Property(e => e.Instructions).IsRequired();

            entity.Property(e => e.AgentType).IsRequired();

            entity.Property(e => e.Model).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // DOCUMENTS
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("DOCUMENTS");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName).IsRequired();

            entity.Property(e => e.FilePath).IsRequired();

            entity.Property(e => e.UploadedAt).IsRequired();

            entity.HasOne(e => e.Agent)
                .WithMany(a => a.Documents)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CHUNKS
        modelBuilder.Entity<Chunk>(entity =>
        {
            entity.ToTable("CHUNKS");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.ChunkIndex).IsRequired();

            entity.Property(e => e.Content).IsRequired();

            entity.Property(e => e.Embedding).IsRequired();

            entity.HasOne(e => e.Document)
                .WithMany(d => d.Chunks)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.DocumentId, e.ChunkIndex })
                .IsUnique();
        });

        // THREADS
        modelBuilder.Entity<AgentThread>(entity =>
        {
            entity.ToTable("THREADS");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Agent)
                .WithMany(a => a.Threads)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MESSAGES
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("MESSAGES");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Role).IsRequired();

            entity.Property(e => e.Content).IsRequired();

            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Thread)
                .WithMany(t => t.Messages)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RUNS
        modelBuilder.Entity<Run>(entity =>
        {
            entity.ToTable("RUNS");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status).IsRequired();

            entity.Property(e => e.StartedAt).IsRequired();

            entity.HasOne(e => e.Agent)
                .WithMany(a => a.Runs)
                .HasForeignKey(e => e.AgentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Thread)
                .WithMany(t => t.Runs)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}