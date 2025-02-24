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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.SetSettings(builder.Configuration);
builder.Services.SetDbContext(builder.Configuration);
builder.Services.SetOurServices(builder.Configuration);
builder.Services.SetAuthentication(builder.Configuration);
builder.Services.SetCors(builder.Configuration, corsPolicyName);
builder.Services.AddCloudinaryService(builder.Configuration);
// builder.Services.AddTransient<IpAddressMiddleware>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
builder.Services.SetSwagger();

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

app.MapControllers().RequireAuthorization();
app.UseHttpsRedirection();

app.Run();
