using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Database;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<IdentityUser>(b =>
		{
			b.Ignore(u => u.PhoneNumber);
			b.Ignore(u => u.PhoneNumberConfirmed);
			b.Ignore(u => u.LockoutEnd);
			b.Ignore(u => u.LockoutEnabled);
			b.Ignore(u => u.TwoFactorEnabled);
			b.Ignore(u => u.SecurityStamp);
			b.Ignore(u => u.ConcurrencyStamp);
		});
	}
}