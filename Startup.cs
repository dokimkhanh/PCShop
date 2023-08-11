using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PCShop.Startup))]
namespace PCShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
