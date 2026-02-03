using Microsoft.AspNetCore.DataProtection.Repositories;
using Sistema_Contable.Filters;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Registrar el filtro de autenticación globalmente
    options.Conventions.ConfigureFilter(new AutenticacionFilter());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");

// Registrar repositorios
builder.Services.AddScoped<IUsuarioRepository>(sp => new UsuarioRepository(connectionString));
builder.Services.AddScoped<IBitacoraRepository>(sp => new BitacoraRepository(connectionString));
builder.Services.AddScoped<IRolRepository>(sp => new RolRepository(connectionString));

// Registrar servicios
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();

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