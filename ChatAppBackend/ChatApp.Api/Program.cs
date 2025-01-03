    using System.Text;
    using ChatApp.Domain;
    using ChatApp.Domain.Configurations;
    using ChatApp.Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authentication:Google"));

    var googleConfigSection = builder.Configuration.GetSection("Authentication:Google");

    if (!googleConfigSection.Exists())
    {
        throw new InvalidOperationException("Configuration section 'Authentication:Google' is missing or empty.");
    }

    builder.Services.Configure<GoogleAuthSettings>(googleConfigSection);
    builder.Services.AddDbContext<ApplicationDbContext>(o =>
    {
        o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    
    builder.Services.AddControllers();
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
    
    builder.Services.AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters()
        {
            //При перевірці токена система звіряє issuer з очікуваним значенням, яке ти задаєш у конфігурації.
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            
        };
    })
    .AddGoogle(options =>
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        var googleAuthSettings = serviceProvider.GetRequiredService<IOptions<GoogleAuthSettings>>().Value;

        
        if (string.IsNullOrEmpty(googleAuthSettings.ClientId) ||
            string.IsNullOrEmpty(googleAuthSettings.ClientSecret))
        {
            throw new InvalidOperationException("Google Authentication settings are invalid or missing.");
        }
        
        
        options.ClientId = googleAuthSettings.ClientId;
        options.ClientSecret = googleAuthSettings.ClientSecret;
        
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });;
    
    
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
        
    builder.Services.AddOpenApi();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            options.RoutePrefix = string.Empty; 
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();