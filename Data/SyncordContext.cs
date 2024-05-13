using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Syncord.Models;

namespace Syncord.Data;

public class SyncordContext : IdentityDbContext<User>
{
    public SyncordContext(DbContextOptions<SyncordContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FriendRequest>()
        .HasKey(f => f.Id);


        modelBuilder.Entity<FriendRequest>()
        .HasOne(f => f.Sender)
        .WithMany(u => u.SentFriendRequests)
        .HasForeignKey(f => f.SenderId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendRequest>()
        .HasOne(f => f.Reciever)
        .WithMany(u => u.RecievedFriendRequests)
        .HasForeignKey(f => f.RecieverId)
        .OnDelete(DeleteBehavior.Restrict);

   
    }

    public DbSet<FriendRequest> friendRequests { get; set; }
}