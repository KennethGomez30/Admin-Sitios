using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sistema_Contable.Filters;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new Microsoft.AspNetCore.Mvc.ServiceFilterAttribute(typeof(AutenticacionFilter)));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Registrar DbConnectionFactory como Singleton
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// Registrar repositorios usando inyección de dependencias de usuarios y roles
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ICuentaContableRepository, CuentaContableRepository>();


// Registrar servicios de autenticacion y roles
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<ICuentaContableService, CuentaContableService>();

// Registrar repositorios y servicios de pantallas
builder.Services.AddScoped<IPantallaRepository, PantallaRepository>();
builder.Services.AddScoped<IPantallaService, PantallaService>();

// Registrar repositorios y servicios de cierre contable
builder.Services.AddScoped<Sistema_Contable.Repository.ICierreContableRepository, Sistema_Contable.Repository.CierreContableRepository>();
builder.Services.AddScoped<Sistema_Contable.Services.ICierreContableService, Sistema_Contable.Services.CierreContableService>();

//Asientos
builder.Services.AddScoped<IAsientoRepository, AsientoRepository>();
builder.Services.AddScoped<IAsientoService, AsientoService>();



//Cuentas(Para selects en Create/Edit Asientos
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();
builder.Services.AddScoped<ICuentaService, CuentaService>();

//Registrar el filtro de autenticación
builder.Services.AddScoped<AutenticacionFilter>();

// Configurar sesión - ADM4: 5 minutos de timeout
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(6);
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