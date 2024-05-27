using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;
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

        //Configure friend ships relations

        modelBuilder.Entity<FriendShip>()
        .HasOne(fs => fs.User1)
        .WithMany(u => u.FriendShips)
        .HasForeignKey(fs => fs.UserId1)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FriendShip>()
        .HasOne(fs => fs.User2)
        .WithMany(u => u.FriendShipsHolder)
        .HasForeignKey(fs => fs.UserId2)
        .OnDelete(DeleteBehavior.Restrict);

        //Configure Message relations

        modelBuilder.Entity<Message>()
        .HasOne(m => m.FriendShip)
        .WithMany(fs => fs.Messages)
        .HasForeignKey(m => m.FriendShipId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
        .HasOne(m => m.Sender)
        .WithMany()
        .HasForeignKey(m => m.SenderId)
        .OnDelete(DeleteBehavior.Restrict);



    }


    public DbSet<FriendRequest> friendRequests { get; set; }
    public DbSet<FriendShip> FriendShips { get; set; }
    public DbSet<Message> Messages { get; set; }
}