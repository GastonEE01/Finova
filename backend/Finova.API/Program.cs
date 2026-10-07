using System.Text;
using Finova.Application.Interfaces;
using Finova.Application.UseCases;
using Finova.Infrastructure.Persistence;
using Finova.Infrastructure.Repositories;
using Finova.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<FinovaDbContext>(options =>
    options.UseNpgsql(builder.Configuration["NeonTech:connectionString"]));

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherAdapter>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IMovementRepository, MovementRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ISavingGoalRepository, SavingGoalRepository>();
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<LoginUserUseCase>();
builder.Services.AddScoped<CreateAccountUseCase>();
builder.Services.AddScoped<ListAccountsUseCase>();
builder.Services.AddScoped<GetAccountUseCase>();
builder.Services.AddScoped<CreateMovementUseCase>();
builder.Services.AddScoped<ListMovementsUseCase>();
builder.Services.AddScoped<GetMovementHistoryUseCase>();
builder.Services.AddScoped<GetDashboardUseCase>();
builder.Services.AddScoped<GetExpensesByCategoryUseCase>();
builder.Services.AddScoped<GetIncomeVsExpensesUseCase>();
builder.Services.AddScoped<GetBalanceEvolutionUseCase>();
builder.Services.AddScoped<GetComparisonsUseCase>();
builder.Services.AddScoped<GetBudgetsByMonthUseCase>();
builder.Services.AddScoped<CreateBudgetUseCase>();
builder.Services.AddScoped<UpdateBudgetUseCase>();
builder.Services.AddScoped<DeleteBudgetUseCase>();
builder.Services.AddScoped<ListSavingGoalsUseCase>();
builder.Services.AddScoped<CreateSavingGoalUseCase>();
builder.Services.AddScoped<UpdateSavingGoalUseCase>();
builder.Services.AddScoped<DeleteSavingGoalUseCase>();
builder.Services.AddScoped<AddGoalContributionUseCase>();
builder.Services.AddScoped<AskAssistantUseCase>();
builder.Services.AddScoped<ListCategoriesUseCase>();
builder.Services.AddHttpClient<IChatClient, OllamaChatClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(100);
});

// Application Insights queda registrado solo si configurás su connection string:
// builder.Services.AddApplicationInsightsTelemetry();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key");
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
