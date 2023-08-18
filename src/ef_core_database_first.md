# Add Nugets
Npgsql.EntityFrameworkCore.PostgreSQL (for pg connection)
Microsoft.EntityFrameworkCore.Tools (for scaffolding)

# Go to Tools -> NuGet Package Manager -> Package Manager Console.
for mac download the extension first by clicking on visual studio -> extensions, search nuget package management extension.
restart visual studio, click view -> other windows

```
Scaffold-DbContext “Host=localhost;Database=<db_name>;Username=<username>;Password=<password>” Npgsql.EntityFrameworkCore.PostgreSQL -OutputDir Models
```

Normally this will create the connection string into the protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) method
and provide a warning.
Delete this override and use dependency injection in Program.cs and have it inject in the option while reading connection string from DI

```
builder.Services.AddDbContext<KafkapubsubContext>(
            options => options.UseNpgsql(configuration["connectionstring"]));
```
