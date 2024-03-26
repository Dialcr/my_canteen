using Canteen;
using Canteen.DataAccess;
using Canteen.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;

//const string corsPolicyName = "MyCustomPolicy";
const string corsPolicyName = "MyCustomPolicy";
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//servicecollections
//builder.Services.SetCors(builder.Configuration, corsPolicyName);
builder.Services.SetSettings(builder.Configuration);
builder.Services.SetDbContext(builder.Configuration);
builder.Services.SetOurServices();
builder.Services.SetCors(builder.Configuration, corsPolicyName);
builder.Services.AddTransient<IpAddressMiddleware>();

builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
var app = builder.Build();
app.UseMiddleware<GlobalErrorHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();

