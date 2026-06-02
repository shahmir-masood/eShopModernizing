namespace eShopModernizedMVC
{
    /// <summary>
    /// Holds the resolved physical paths for the running application so that
    /// non-controller components (EF initializer, image services) can locate
    /// content files without depending on the legacy System.Web HostingEnvironment.
    /// </summary>
    public static class HostingConfiguration
    {
        public static string ContentRootPath { get; private set; } = string.Empty;

        public static string WebRootPath { get; private set; } = string.Empty;

        public static void Initialize(string contentRootPath, string webRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = webRootPath;
        }
    }
}
