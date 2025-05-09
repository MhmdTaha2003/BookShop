using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repository;
using WebApplication1.Repository.IRepository;
using WebApplication1.Utility;
using WebApplication1.Utility.BrainTree;

namespace WebApplication1
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options =>

			options.UseSqlServer(
				Configuration.GetConnectionString("DefaultConnection")
				));


            services.AddIdentity<IdentityUser , IdentityRole>()
				.AddDefaultTokenProviders().AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddTransient<IEmailSender, EmailSender>();

			services.AddHttpContextAccessor();
			services.AddSession(Option => {
				Option.IdleTimeout = TimeSpan.FromMinutes(10);
				Option.Cookie.HttpOnly= true;
				Option.Cookie.IsEssential= true; 
				});

			services.Configure<BrainTreeSettings>(Configuration.GetSection("BrainTree"));
			services.AddSingleton<IBrainTreeGate , BrainTreeGate>();

			services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

        //    services.AddScoped<IInquiryDetailRepository, InquiryDetailRepository>();

        //    services.AddScoped<IInquiryHeaderRepository,InquiryHeaderRepository>();
			services.AddScoped<IApplicationUserRepository , ApplicationUserRepository>();

            services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

        }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseSession();


			app.UseEndpoints(endpoints =>
			{
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Books}/{action=Index}/{id?}");
			});
		}
	}
}
