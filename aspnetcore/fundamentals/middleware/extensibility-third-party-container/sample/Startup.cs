using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using MiddlewareExtensibilitySample.Data;
using MiddlewareExtensibilitySample.Middleware;


namespace MiddlewareExtensibilitySample
{
    public class Startup
    {
        private Container _container = new Container();

        #region snippet1
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Replace the default middleware factory with the 
            // SimpleInjectorMiddlewareFactory.
            services.AddTransient<IMiddlewareFactory>(_ =>
            {
                return new SimpleInjectorMiddlewareFactory(_container);
            });

            // Wrap ASP.NET Core requests in a Simple Injector execution 
            // context.
            services.UseSimpleInjectorAspNetRequestScoping(_container);

            // Provide the database context from the Simple 
            // Injector container whenever it's requested from 
            // the default service container.
            services.AddScoped<AppDbContext>(provider => 
                _container.GetInstance<AppDbContext>());

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            _container.Register<AppDbContext>(() => 
            {
                var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
                optionsBuilder.UseInMemoryDatabase("InMemoryDb");
                return new AppDbContext(optionsBuilder.Options);
            }, Lifestyle.Scoped);

            _container.Register<SimpleInjectorActivatedMiddleware>();

            _container.Verify();
        }
        #endregion

        #region snippet2
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSimpleInjectorActivatedMiddleware();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvc();
        }
        #endregion
    }
}
