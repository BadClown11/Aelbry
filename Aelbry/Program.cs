using System.Text;
using Aelbry.BL;
using Aelbry.BL.AI;
using Aelbry.BL.Documents;
using Aelbry.BL.Email;
using Aelbry.BL.Export;
using Aelbry.BL.Import;
using Aelbry.BL.Notifications;
using Aelbry.BL.Security;
using Aelbry.Web.Hubs;
using Aelbry.Web.Security;
using Aelbry.Web.Services;
using Hangfire;
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
builder.Services.AddScoped<AutomationEngineBL>();
builder.Services.AddScoped<AutomationRuleBL>();
builder.Services.AddScoped<ReportBL>();
builder.Services.AddScoped<ActivityBL>();
builder.Services.AddScoped<TimeEntryBL>();

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddHttpClient<IActivitySuggestionService, GeminiActivitySuggestionService>();
builder.Services.AddScoped<AiAssistantBL>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ExcelActivityImportBL>();

builder.Services.AddScoped<ChatBL>();
builder.Services.AddSignalR();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();
builder.Services.AddScoped<NotificationBL>();
builder.Services.AddScoped<DueDateReminderBL>();
builder.Services.AddHostedService<DueDateReminderService>();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.AddSingleton<FileStorageService>();
builder.Services.AddScoped<DocumentBL>();
builder.Services.AddScoped<FileBL>();

builder.Services.AddScoped<WeeklyReportEmailJob>();
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

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

        // El cliente JS de SignalR no puede mandar el header Authorization en todos los
        // transportes (ej. WebSockets); en su lugar manda el JWT como query string
        // "access_token" solo para las llamadas al hub.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
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

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new LocalOnlyDashboardAuthorizationFilter() },
});

try
{
    int weeklyReportHourUtc = builder.Configuration.GetValue("WeeklyReport:HourUtc", 8);
    RecurringJob.AddOrUpdate<WeeklyReportEmailJob>(
        "weekly-report-email",
        job => job.RunAsync(),
        Cron.Weekly(DayOfWeek.Monday, weeklyReportHourUtc));
}
catch (Exception ex)
{
    // Si el SQL Server de Hangfire no esta disponible en el arranque, no se debe tumbar toda
    // la app por esto: se registra el aviso y el job recurrente se reintentara registrar en el
    // siguiente reinicio (Hangfire.SqlServer tambien reintenta la migracion de su esquema solo).
    app.Logger.LogWarning(ex, "No se pudo registrar el job recurrente 'weekly-report-email' en Hangfire.");
}

app.Run();
