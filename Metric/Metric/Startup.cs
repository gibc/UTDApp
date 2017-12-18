using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Metric.Startup))]
namespace Metric
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
