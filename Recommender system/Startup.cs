using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Recommender_system.Startup))]
namespace Recommender_system
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
