using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniTodoAPI.Controller;
using MySql.Data.MySqlClient;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<GetConnection>(sp => 
    async () => {
        string connectionString = sp.GetService<IConfiguration>()["ConnectionString"];
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", () => "Minimal User Data API");
app.MapGet("/users", async (GetConnection connectionGetter) =>
{
    using var con = await connectionGetter();
    return con.GetAll<ServiceUser>().ToList();
});

app.MapGet("/users/{id}", async (GetConnection connectionGetter, int id) =>
{
    using var con = await connectionGetter();
    return con.Get<ServiceUser>(id);
});

app.MapGet("/users/age/{age}", async (GetConnection connectionGetter, int age) =>
{
    using var con = await connectionGetter();
    return con.GetAll<ServiceUser>().Where(u => u.Age >= age).ToList();
});

app.MapDelete("/users/{id}", async (GetConnection connectionGetter, int id) =>
{
    using var con = await connectionGetter();
    con.Delete(new ServiceUser(id,"","", 0));
    return Results.NoContent();
});

app.MapPost("/users", async (GetConnection connectionGetter, ServiceUser user) =>
{
    if (MailValidationController.IsValidEmail(user.Mail))
    {
        using var con = await connectionGetter();
        var id = con.Insert(user);
        return Results.Created($"/todos/{id}", user);
    }

    return Results.Conflict($"Incorrect mail address format");
});

app.MapPut("/users", async (GetConnection connectionGetter, ServiceUser user) =>
{
    using var con = await connectionGetter();
    var validUser = con.Get<ServiceUser>(user.Id);
    if (validUser == null)
    {
        return Results.Conflict("User with this id not found in database");
    }
    if (MailValidationController.IsValidEmail(user.Mail))
    {
        con.Update(user);
        return Results.Ok();
    }

    con.Update(new ServiceUser(user.Id, user.UserName, validUser.Mail, user.Age));
    return Results.Ok("Invalid mail in request, mail not changed");
});

app.Run();

[Table("serviceuser")]
public record ServiceUser(int Id, string UserName, string Mail, int Age);

public delegate Task<IDbConnection> GetConnection();