﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Models;
using SchoolManagement.Security;

namespace SchoolManagement
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("StudentDBConnection")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            }).AddEntityFrameworkStores<AppDbContext>();

            //services.AddMvc();

            services.ConfigureApplicationCookie(options => {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddAuthorization(options => {
                options.AddPolicy("DeleteRolePolicy", policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement());
                
                options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role"));

                //options.AddPolicy("EditRolePolicy", policy => policy.RequireAssertion(context =>
                //      context.User.IsInRole("Admin") &&
                //      context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                //      context.User.IsInRole("Super Admin")
                //      ));

                options.AddPolicy("CreateRolePolicy", policy => policy.RequireClaim("Create Role"));

                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));
            });

            services.AddScoped<IStudentRepository, SQLStudentRepository>();
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
             
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes => {
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            //app.UseMvcWithDefaultRoute();

            //app.Run(async (context) =>
            //{
            //    //throw new Exception("Some error processing the request");
            //    //await context.Response.WriteAsync(_config["MyKey"]);
            //    await context.Response.WriteAsync("Hosting Environment: " + env.EnvironmentName);
            //});

        }
    }
}
