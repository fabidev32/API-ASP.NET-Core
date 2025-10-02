using API;
using API_Trabalho_Pratico;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<LocadoraDB>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/clientes", async (LocadoraDB db) => await db.Clientes.ToListAsync());
app.MapPost("/clientes", async (Cliente cliente, LocadoraDB db) =>
{
    db.Clientes.Add(cliente);
    await db.SaveChangesAsync();
    return Results.Created($"/clientes/{cliente.Id}", cliente);
});

app.MapGet("/veiculos", async (LocadoraDB db) =>
    await db.Veiculos.Include(v => v.Fabricante).ToListAsync()
);

app.MapPost("/veiculos", async (Veiculo veiculo, LocadoraDB db) =>
{
    db.Veiculos.Add(veiculo);
    await db.SaveChangesAsync();
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
});

app.MapGet("/alugueis", async (LocadoraDB db) =>
    await db.Alugueis
        .Include(a => a.Cliente)
        .Include(a => a.Veiculo)
        .Include(a => a.Funcionario)
        .ToListAsync()
);

app.MapPost("/alugueis", async (Aluguel aluguel, LocadoraDB db) =>
{
    db.Alugueis.Add(aluguel);
    await db.SaveChangesAsync();
    return Results.Created($"/alugueis/{aluguel.Id}", aluguel);
});

app.MapGet("/funcionarios", async (LocadoraDB db) =>
    await db.Funcionarios.ToListAsync()
);

app.MapPost("/funcionarios", async (Funcionario funcionario, LocadoraDB db) =>
{
    db.Funcionarios.Add(funcionario);
    await db.SaveChangesAsync();
    return Results.Created($"/funcionarios/{funcionario.Id}", funcionario);
});

app.MapGet("/fabricantes", async (LocadoraDB db) =>
    await db.Fabricantes.ToListAsync()
);

app.MapPost("/fabricantes", async (Fabricante fabricante, LocadoraDB db) =>
{
    db.Fabricantes.Add(fabricante);
    await db.SaveChangesAsync();
    return Results.Created($"/fabricantes/{fabricante.Id}", fabricante);
});

app.Urls.Add("https://localhost:3000");
app.Run();
