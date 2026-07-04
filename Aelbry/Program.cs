using System.Text;
using Aelbry.BL;
using Aelbry.BL.AI;
using Aelbry.BL.Import;
using Aelbry.BL.Security;
using Aelbry.Web.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddScoped<CompanyBL>();
builder.Services.AddScoped<DepartmentBL>();
builder.Services.AddScoped<TeamBL>();
builder.Services.AddScoped<UserBL>();
builder.Services.AddScoped<RoleBL>();
builder.Services.AddScoped<AuthBL>();
builder.Services.AddScoped<ProjectBL>();
builder.Services.AddScoped<ProjectStatusBL>();
builder.Services.AddScoped<TagBL>();
builder.Services.AddScoped<ProjectTemplateBL>();
builder.Services.AddScoped<ActivityBL>();

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddHttpClient<IActivitySuggestionService, GeminiActivitySuggestionService>();
builder.Services.AddScoped<AiAssistantBL>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ExcelActivityImportBL>();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
