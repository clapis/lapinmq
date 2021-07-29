using System;
using GreenPipes;
using LapinMQ.Consumers;
using LapinMQ.Contracts;
using LapinMQ.Exceptions;
using LapinMQ.StateMachines;
using MassTransit;
using MassTransit.Saga;
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
                x.AddDelayedMessageScheduler();
                x.SetKebabCaseEndpointNameFormatter();

                x.AddConsumer<OrderRedeliveryConsumer>();

                x.UsingRabbitMq((context, config) =>
                {
                    config.UseDelayedMessageScheduler();

                    config.UseDelayedRedelivery(redelivery =>
                    {
                        redelivery.Interval(2, TimeSpan.FromSeconds(1));
                        redelivery.Handle<RedeliveryException>();
                    });
                    
                    config.UseMessageRetry(retry =>
                    {
                        retry.Interval(2, TimeSpan.FromSeconds(1));
                        retry.Handle<RedeliveryException>();
                    });
                    
                    // config.ConfigureEndpoints(context);
                    
                    config.ReceiveEndpoint("order-redelivery", endpoint =>
                    {
                        endpoint.Durable = true;
                        endpoint.PrefetchCount = 1;
                        endpoint.UseConcurrencyLimit(1);
                        
                        endpoint.Consumer<OrderRedeliveryConsumer>(context, consumer =>
                        {
                            
                        });
                    });
                    
                    config.ReceiveEndpoint("order-state", endpoint =>
                    {
                        endpoint.Durable = true;
                        endpoint.StateMachineSaga(
                            context.GetRequiredService<OrderStateMachine>(), 
                            context.GetRequiredService<ISagaRepository<OrderState>>(),
                            saga =>
                            {
                                
                            });
                    });
                });
                
                x.AddSagaStateMachine<OrderStateMachine, OrderState>()
                    .MongoDbRepository(r =>
                    {
                        r.Connection = "mongodb://127.0.0.1";
                        r.DatabaseName = "lapinmq";
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