using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Schubert.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Schubert.Framework.Web.Validation;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.DataAnnotations;

namespace Schubert.Framework.Web
{
    public class FluentValidationStarter : WebStarter
    {
        public override void ConfigureServices(SchubertServicesBuilder servicesBuilder, SchubertWebOptions webOptions)
        {
            servicesBuilder.ServiceCollection.Configure<MvcOptions>(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new FluentValidationMetadataProvider());
                options.ModelValidatorProviders.Add(new FluentValidationModelValidatorProvider());
            });

            servicesBuilder.ServiceCollection.AddTransient<IConfigureOptions<MvcViewOptions>, LabijieMvcViewOptionsSetup>();
        }

        public override void Start(IApplicationBuilder appBuilder, SchubertWebOptions options)
        {
        }

        public class LabijieMvcViewOptionsSetup : ConfigureOptions<MvcViewOptions>
        {
            public LabijieMvcViewOptionsSetup(IServiceProvider serviceProvider)
                : base(options => ConfigureMvc(options, serviceProvider))
            {
            }

            public static void ConfigureMvc(
                MvcViewOptions options,
                IServiceProvider serviceProvider)
            {
                //var dataAnnotationsLocalizationOptions =
                //    serviceProvider.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>();
                //var stringLocalizerFactory = serviceProvider.GetService<IStringLocalizerFactory>();

                // Set up client validators
                options.ClientModelValidatorProviders.Add(new FluentValidationClientModelValidatorProvider());
            }
        }
    }
}
