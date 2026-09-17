using Microsoft.EntityFrameworkCore;
using TbtbChallenge.Api.Entities;

namespace TbtbChallenge.Api.Data;

public class TbtbChallengeDbContext : DbContext
{
    public TbtbChallengeDbContext(DbContextOptions<TbtbChallengeDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Gestor> Gestors => Set<Gestor>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<ContactAmendment> ContactAmendments => Set<ContactAmendment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("Patients");
            entity.HasKey(p => p.Id);
        });

        modelBuilder.Entity<Gestor>(entity =>
        {
            entity.ToTable("Gestors");
            entity.HasKey(g => g.Id);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contacts");
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.Patient)
                .WithMany()
                .HasForeignKey(c => c.PatientId);
            entity.HasOne(c => c.Gestor)
                .WithMany()
                .HasForeignKey(c => c.GestorId);
        });

        modelBuilder.Entity<ContactAmendment>(entity =>
        {
            entity.ToTable("ContactAmendments");
            entity.HasKey(a => a.Id);
            entity.HasOne(a => a.Contact)
                .WithMany()
                .HasForeignKey(a => a.ContactId);
            entity.HasOne(a => a.AmendedByGestor)
                .WithMany()
                .HasForeignKey(a => a.AmendedByGestorId);
        });
    }
}
