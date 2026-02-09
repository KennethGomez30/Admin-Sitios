using Sistema_Contable.Repository;
using Sistema_Contable.Services;
using Sistema_Contable.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Registrar filtro de autenticación globalmente
    options.Conventions.ConfigureFilter(new AutenticacionFilter());
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registrar repositorios y servicios
builder.Services.AddScoped<IUsuarioRepository>(sp => new UsuarioRepository(connectionString));
builder.Services.AddScoped<IBitacoraRepository>(sp => new BitacoraRepository(connectionString));
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();

builder.Services.AddScoped<IEstadosAsientoRepository>(sp => new EstadosAsientoRepository(connectionString));
builder.Services.AddScoped<IEstadosAsientoService, EstadoAsientoService>();

builder.Services.AddScoped<IPeriodoContableRepository>(_ => new PeriodoContableRepository(connectionString!));
builder.Services.AddScoped<IPeriodoContableService, PeriodoContableService>();

builder.Services.AddScoped<ICambiarEstadoAsientoRepository>(sp =>new CambiarEstadoAsientoRepository(connectionString!));

builder.Services.AddScoped<ICambiarEstadoAsientoService, CambiarEstadoAsientoService>();



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