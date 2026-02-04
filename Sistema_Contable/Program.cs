using Sistema_Contable.Repository;
using Sistema_Contable.Services;
using Sistema_Contable.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(AutenticacionFilter)));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

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

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Error");
app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar sesión
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();