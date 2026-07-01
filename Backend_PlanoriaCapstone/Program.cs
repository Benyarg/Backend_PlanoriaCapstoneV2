using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlanoriaCapstone.Bll.Interface;
using PlanoriaCapstone.Bll.Service;
using PlanoriaCapstone.Dal;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// DATABASE
builder.Environment.WebRootPath =
    Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

Directory.CreateDirectory(builder.Environment.WebRootPath);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// SERVICES
builder.Services.AddHttpClient();

// REPOSITORIES
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserCourseExamProgressRepository, UserCourseExamProgressRepository>();
builder.Services.AddScoped<IFlashcardRepository, FlashcardRepository>();
builder.Services.AddScoped<IFlashcardDeckRepository, FlashcardDeckRepository>();
builder.Services.AddScoped<IUserProgressFlashcardRepository, UserProgressFlashcardRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
builder.Services.AddScoped<IUserProgressQuizRepository, UserProgressQuizRepository>();
builder.Services.AddScoped<IFileUploadRepository, FileUploadRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IStudyScheduleRepository, StudyScheduleRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IFlashcardDeckService, FlashcardDeckService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IQuizService, QuizService>();

// BLL SERVICES
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFlashcardDeckService, FlashcardDeckService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<IFlashcardStudyService, FlashcardStudyService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IAiGenerationService, AiGenerationService>();
builder.Services.AddScoped<IFlashcardProgressService, FlashcardProgressService>();
builder.Services.AddScoped<IQuizProgressService, QuizProgressService>();
builder.Services.AddScoped<ICourseProgressService, CourseProgressService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IIntervalService, IntervalService>();
builder.Services.AddScoped<IScheduleContentService, ScheduleContentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISystemService, SystemService>();
builder.Services.AddScoped<IReportService, ReportService>();

// CONTROLLERS
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });

// SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Planoria API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Bearer TOKEN"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            {
                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }, Array.Empty<string>() }
    });
});

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:7075")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// AUTO-MIGRATE with recovery
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    for (int i = 0; i < 30; i++)
    {
        try { if (db.Database.CanConnect()) break; }
        catch { if (i == 29) throw; Thread.Sleep(1000); }
    }

    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var inner = ex;
        while (inner is not null and not Microsoft.Data.SqlClient.SqlException)
            inner = inner.InnerException;
        if (inner is Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            if (sqlEx.Number == 1801) return;
            if (sqlEx.Number != 2714) throw;
        }
        else throw;

        foreach (var m in db.Database.GetMigrations())
        {
            db.Database.ExecuteSqlRaw(
                "IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0}) " +
                "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                m, "8.0.0");
        }
    }
}

// GLOBAL EXCEPTION HANDLER
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var exceptionHandlerPathFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => 401,
            KeyNotFoundException => 404,
            ArgumentException => 400,
            _ => 500
        };

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = exception?.Message ?? "Ocurrió un error inesperado.",
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Planoria API v1"); });

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();