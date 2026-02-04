using Microsoft.AspNetCore.DataProtection.Repositories;
using Sistema_Contable.Filters;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(AutenticacionFilter)));
});

// Registrar DbConnectionFactory como Singleton
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Registrar repositorios usando inyección de dependencias
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();

// Registrar repositorios y servicios de usuarios y bitácora
builder.Services.AddScoped<IUsuarioRepository>(sp => new UsuarioRepository(connectionString));
builder.Services.AddScoped<IBitacoraRepository>(sp => new BitacoraRepository(connectionString));
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();

// Registrar repositorios y servicios de pantallas
builder.Services.AddScoped<IPantallaRepository, PantallaRepository>();
builder.Services.AddScoped<IPantallaService, PantallaService>();

//Registrar el filtro de autenticación
builder.Services.AddScoped<AutenticacionFilter>();

// Configurar sesión - ADM4: 5 minutos de timeout
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseExceptionHandler("/Error");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar sesión ANTES de la autorización
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();