using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiniTodoAPI.Controller;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<GetConnection>(sp => 
    async () =>
    {
        string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") == null
            ? sp.GetService<IConfiguration>()["DB_CONNECTION_STRING"]
            : Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
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

app.MapGet("/", () => "Minimal User API v1");

app.MapGet("/users", async (GetConnection connectionGetter) =>
    {
        using var con = await connectionGetter();
        var users = con.GetAll<UserResponse>().ToList();
        return users.Count > 0 ? Results.Ok(users) : Results.NoContent();
    })
    .WithOpenApi()
    .WithDescription("description")
    .Produces(200, responseType:typeof(List<UserResponse>))
    .Produces(204, responseType:null);

app.MapGet("/users/{id:int}", async (GetConnection connectionGetter, int id) =>
{
    using var con = await connectionGetter();
    return await con.GetAsync<UserResponse>(id);
});

app.MapGet("/users/age/{age:int}", async (GetConnection connectionGetter, int age) =>
{
    using var con = await connectionGetter();
    return con.GetAll<UserResponse>().Where(u => u.Age >= age).ToList();
});

app.MapDelete("/users/{id:int}", async (GetConnection connectionGetter, int id) =>
{
    using var con = await connectionGetter();
    bool isDeleted = con.Delete(new UserResponse(id,"","", 0));
    return isDeleted ? Results.Ok() : Results.NotFound();
});

app.MapPost("/users", async (GetConnection connectionGetter, UserRequest user) =>
{
    if (MailValidationController.IsValidEmail(user.Mail))
    {
        using var con = await connectionGetter();
        var id = await con.InsertAsync(user);
        return Results.Created($"/users/{id}", user);
    }

    return Results.BadRequest("Incorrect mail address format");
})
    .WithOpenApi()
    .WithDescription("description")
    .Produces(201, responseType:typeof(UserRequest))
    .Produces(400, responseType:typeof(string), contentType:"text/plain");

app.MapPut("/users", async (GetConnection connectionGetter, UserResponse user) =>
{
    using var con = await connectionGetter();
    var validUser = con.Get<UserResponse>(user.Id);
    if (validUser == null)
    {
        return Results.NotFound("User with this id not found in database");
    }
    if (MailValidationController.IsValidEmail(user.Mail))
    {
        con.Update(user);
        return Results.Ok("User changed succesfully");
    }

    con.Update(new UserResponse(user.Id, user.UserName, validUser.Mail, user.Age));
    return Results.Ok("Invalid mail in request, user updated, but mail was not changed.");
})
    .WithOpenApi()
    .WithDescription("description")
    .Produces(200, responseType:typeof(string), contentType:"text/plain")
    .Produces(200, responseType:typeof(string), contentType:"text/plain")
    .Produces(404, responseType:typeof(string), contentType:"text/plain");

app.Run();

[Table("useraccount")]
public record UserResponse(int Id, string UserName, string Mail, int Age);
[Table("useraccount")]
public record UserRequest(string UserName, string Mail, int Age);

public delegate Task<IDbConnection> GetConnection();