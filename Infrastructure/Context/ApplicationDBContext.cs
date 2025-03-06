using Domain.Common.Const;
using Domain.Entities;
using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,
    ApplicationRole,
    string,
    IdentityUserClaim<string>,
    ApplicationUserRole,
    IdentityUserLogin<string>,
    ApplicationRoleClaim,
    IdentityUserToken<string>>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<EventRequest> EventRequests { get; set; }
        public DbSet<MatchEvent> MatchEvents { get; set; }
        public DbSet<Attendee> AttendeeMaster { get; set; }
        public DbSet<Conference> ConferenceMaster { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<AddOnMaster> AddOnMasters { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUserRole>()
           .HasOne(aur => aur.Role)
           .WithMany()
           .HasForeignKey(aur => aur.RoleId);

            builder.Entity<ApplicationUserRole>()
              .HasOne(aur => aur.User)
              .WithMany()
              .HasForeignKey(aur => aur.UserId);

            string schema = SchemaNames.Identity;

            var entities = new (Action<EntityTypeBuilder> configure, Type entityType)[]
            {
            (entity => entity.ToTable(IdentityTables.Users, schema), typeof(ApplicationUser)),
            (entity => entity.ToTable(IdentityTables.Roles, schema), typeof(ApplicationRole)),
            (entity => entity.ToTable(IdentityTables.UserRoles, schema), typeof(ApplicationUserRole)),
            (entity => entity.ToTable(IdentityTables.UserClaims, schema), typeof(IdentityUserClaim<string>)),
            (entity => entity.ToTable(IdentityTables.UserLogins, schema), typeof(IdentityUserLogin<string>)),
            (entity => entity.ToTable(IdentityTables.RoleClaims, schema), typeof(ApplicationRoleClaim)),
            (entity => entity.ToTable(IdentityTables.UserTokens, schema), typeof(IdentityUserToken<string>))
            };

            foreach (var (configure, entityType) in entities)
            {
                builder.Entity(entityType, configure);
            }

            builder.HasDefaultSchema(SchemaNames.Dbo);
        }
    }
}
