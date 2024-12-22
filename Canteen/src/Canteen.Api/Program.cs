using Canteen;
using Canteen.DataAccess;
using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string corsPolicyName = "MyCustomPolicy";
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.SetSettings(builder.Configuration);
builder.Services.SetDbContext(builder.Configuration);
builder.Services.SetOurServices();
builder.Services.SetAuthentication(builder.Configuration);
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
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EntitiesContext>();
    dbContext.Database.Migrate();
}
using (var scopeDoWork = app.Services.CreateScope())
{
    var dbContext = scopeDoWork.ServiceProvider.GetRequiredService<EntitiesContext>();
    var userManager = scopeDoWork.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
}

await CreateData.AddDataIntoDatabaseAsync(app.Services, builder.Configuration);
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
