using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CSharpTesterAspNet.Startup))]
namespace CSharpTesterAspNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
