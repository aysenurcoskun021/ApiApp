using apideneme.Controllers.Data;
using apideneme.Controllers.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<RandomUserService>();

// CORS politikasý (React uygulamasýna izin veriliyor)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:3000")  // React uygulamanýn çalýþtýðý adres
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// Controller ve Swagger servisleri //DEÐÝÞÝKLÝK YAPIYORUM 23.07
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Döngüsel referanslarý görmezden gelmek için bu satýrý ekliyoruz
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Eðer CORS hatasý alýyorsanýz ve CORS ayarýnýz yoksa bu kýsmý da ekleyebilirsiniz
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // React uygulamanýzýn çalýþtýðý port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient servisi (RandomUserService için)
builder.Services.AddHttpClient<RandomUserService>();

// PostgreSQL baðlantýsý (appsettings.json içindeki baðlantý stringine göre)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Uygulama nesnesi oluþturuluyor
var app = builder.Build();

// CORS middleware'i kullanýma alýnýyor
app.UseCors("AllowReactApp");

// Geliþtirme ortamýnda Swagger aktif ediliyor
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS yönlendirmesi
app.UseHttpsRedirection();

app.UseCors();//EKLEDÝM23.07 CORS ÝÇÝN

// Yetkilendirme middleware'i
app.UseAuthorization();

// Controller endpointlerine yönlendirme
app.MapControllers();

// Uygulama baþlatýlýyor
app.Run();
