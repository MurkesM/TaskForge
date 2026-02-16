using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TaskForge.Data;
using TaskForge.Repositories;
using TaskForge.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=taskforge.db"));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TaskForge.Middleware.RequestLoggingMiddleware>();
app.UseMiddleware<TaskForge.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();