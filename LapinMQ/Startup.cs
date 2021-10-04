using System;
using GreenPipes;
using LapinMQ.Consumers;
using LapinMQ.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LapinMQ
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen();

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<OrderRedeliveryConsumer>();

                x.UsingRabbitMq((context, config) =>
                {
                    config.ConfigureEndpoints(context);
                    config.UseDelayedMessageScheduler();

                    config.UseDelayedRedelivery(redelivery =>
                    {
                        redelivery.Interval(2, TimeSpan.FromSeconds(1));
                        redelivery.Handle<RedeliveryException>();
                    });
                });
            });

            services.AddMassTransitHostedService();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}