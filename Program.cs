using Api.Microservice.Autor.Aplicacion;
using Api.Microservice.Autor.Persistencia;
using AutoMapper;
using Grpc_AutorImagen;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContextoAutor>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Configura Polly para el cliente gRPC
var retryPolicy = Policy.Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var fallbackPolicy = Policy<Grpc_AutorImagen.ImagenResponse>.Handle<Exception>()
    .FallbackAsync(new Grpc_AutorImagen.ImagenResponse(), (exception, context) =>
    {
        // Log the error or handle it accordingly
        return Task.CompletedTask;
    });

builder.Services.AddGrpcClient<AutorImagenService.AutorImagenServiceClient>(o =>
{
    o.Address = new Uri("http://localhost:5185"); // Dirección del servicio gRPC
});

// Registra las políticas en el contenedor de dependencias
builder.Services.AddSingleton<IAsyncPolicy<Grpc_AutorImagen.ImagenResponse>>(fallbackPolicy);

builder.Services.AddMediatR(typeof(Nuevo.Manejador).Assembly);
builder.Services.AddAutoMapper(typeof(Consulta.Manejador));

builder.Services.AddCors(options => {
    options.AddPolicy("NuevaPolitica", app => {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
