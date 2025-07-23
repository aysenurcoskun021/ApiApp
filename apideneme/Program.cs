using apideneme.Controllers.Data;
using apideneme.Controllers.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<RandomUserService>();

// CORS politikas� (React uygulamas�na izin veriliyor)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:3000")  // React uygulaman�n �al��t��� adres
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// Controller ve Swagger servisleri //DE����KL�K YAPIYORUM 23.07
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // D�ng�sel referanslar� g�rmezden gelmek i�in bu sat�r� ekliyoruz
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// E�er CORS hatas� al�yorsan�z ve CORS ayar�n�z yoksa bu k�sm� da ekleyebilirsiniz
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // React uygulaman�z�n �al��t��� port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient servisi (RandomUserService i�in)
builder.Services.AddHttpClient<RandomUserService>();

// PostgreSQL ba�lant�s� (appsettings.json i�indeki ba�lant� stringine g�re)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Uygulama nesnesi olu�turuluyor
var app = builder.Build();

// CORS middleware'i kullan�ma al�n�yor
app.UseCors("AllowReactApp");

// Geli�tirme ortam�nda Swagger aktif ediliyor
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS y�nlendirmesi
app.UseHttpsRedirection();

app.UseCors();//EKLED�M23.07 CORS ���N

// Yetkilendirme middleware'i
app.UseAuthorization();

// Controller endpointlerine y�nlendirme
app.MapControllers();

// Uygulama ba�lat�l�yor
app.Run();
