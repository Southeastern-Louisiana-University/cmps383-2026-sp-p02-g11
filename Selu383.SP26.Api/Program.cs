using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;

public partial class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddDbContext<DataContext>(options =>
			options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
		builder.Services.AddDistributedMemoryCache();
		builder.Services.AddSession(options =>
		{
			options.Cookie.HttpOnly = true;
			options.Cookie.IsEssential = true;
		});

		var app = builder.Build();

		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<DataContext>();
			db.Database.Migrate();

			if (!db.Roles.Any())
			{
				db.Roles.AddRange(
					new Role { Name = "Admin" },
					new Role { Name = "User" }
				);
				db.SaveChanges();
			}

			var adminRole = db.Roles.First(r => r.Name == "Admin");
			var userRole = db.Roles.First(r => r.Name == "User");

			if (!db.Users.Any(u => u.UserName == "galkadi"))
			{
				db.Users.Add(new User { UserName = "galkadi", HashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!"), RoleId = adminRole.Id });
				db.SaveChanges();
			}

			if (!db.Users.Any(u => u.UserName == "bob"))
			{
				db.Users.Add(new User { UserName = "bob", HashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!"), RoleId = userRole.Id });
				db.SaveChanges();
			}

			if (!db.Users.Any(u => u.UserName == "sue"))
			{
				db.Users.Add(new User { UserName = "sue", HashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123!"), RoleId = userRole.Id });
				db.SaveChanges();
			}

			if (!db.Locations.Any())
			{
				var adminUser = db.Users.First(u => u.UserName == "galkadi");
				db.Locations.AddRange(
					new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10, ManagerId = adminUser.Id },
					new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20, ManagerId = adminUser.Id },
					new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15, ManagerId = adminUser.Id }
				);
				db.SaveChanges();
			}
		}

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseSession();
		app
			.UseRouting()
			.UseAuthorization()
			.UseAuthentication()
			.UseEndpoints(x =>
			{
				x.MapControllers();
			});

		app.UseStaticFiles();

		if (app.Environment.IsDevelopment())
		{
			app.UseSpa(x =>
			{
				x.UseProxyToSpaDevelopmentServer("https://localhost:5173");
			});
		}
		else
		{
			app.MapFallbackToFile("/index");
		}
		app.Run();
	}
}

public partial class Program { }