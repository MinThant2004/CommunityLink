using CommunityLink.Domain;
using CommunityLink.Domain.Features.ChatGroup;
using CommunityLink.Api.Controllers;
using CommunityLink.Api.Middlewares;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Domain.Features.LinkDrop;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Application Parts
builder.Services.AddControllers()
    .AddApplicationPart(typeof(AuthenticationController).Assembly)
    .AddApplicationPart(typeof(CommunityLink.Domain.Features.Community.CommunityController).Assembly);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CommunityLinkSuperSecretSigningKey1234567890!_SecurityKey";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "CommunityLink.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "CommunityLink.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat-groups"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Domain Services & DbContext
builder.Services.AddDomainServices(builder.Configuration);

var app = builder.Build();

// Seed RBAC on Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await RbacSeeder.SeedAsync(db);
    await LinkDropSeeder.SeedAsync(db);
}

// Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiRequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseMiddleware<PasswordChangeRequirementMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatGroupHub>("/hubs/chat-groups");

app.Run();

#pragma warning disable ASP0027
public partial class Program;
#pragma warning restore ASP0027