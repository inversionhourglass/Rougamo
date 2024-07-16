using System.Reflection;

namespace WebApis
{
    public static class MvcExtensions
    {
        public static IMvcBuilder AddCurrentApplicationPart(this IMvcBuilder builder)
        {
            var currentAssembly = typeof(MvcExtensions).Assembly;
            if (currentAssembly != Assembly.GetEntryAssembly())
            {
                builder.AddApplicationPart(currentAssembly);
            }

            return builder;
        }
    }
}
