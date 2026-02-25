using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

// --- IDENTITY SETUP ---
builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
});
// ----------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- DATABASE MIGRATION & SEEDING ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<DataContext>();
    db.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    if (!roleManager.Roles.Any())
    {
        roleManager.CreateAsync(new Role { Name = "Admin" }).GetAwaiter().GetResult();
        roleManager.CreateAsync(new Role { Name = "User" }).GetAwaiter().GetResult();
    }

    var userManager = services.GetRequiredService<UserManager<User>>();
    if (!userManager.Users.Any())
    {
        var password = "Password123!";

        var bob = new User { UserName = "bob" };
        userManager.CreateAsync(bob, password).GetAwaiter().GetResult();
        userManager.AddToRoleAsync(bob, "User").GetAwaiter().GetResult();

        var sue = new User { UserName = "sue" };
        userManager.CreateAsync(sue, password).GetAwaiter().GetResult();
        userManager.AddToRoleAsync(sue, "User").GetAwaiter().GetResult();

        var galkadi = new User { UserName = "galkadi" };
        userManager.CreateAsync(galkadi, password).GetAwaiter().GetResult();
        userManager.AddToRoleAsync(galkadi, "Admin").GetAwaiter().GetResult();
    }

    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10 },
            new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20 },
            new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15 }
        );
        db.SaveChanges();
    }
}
// -------------------------------------

// SWAGGER IS NOW OUTSIDE THE IF-BLOCK
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    // You can keep other dev-only tools here if needed
}

app.UseHttpsRedirection();

// IMPORTANT: Authentication must come BEFORE Authorization

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }