using Xunit;

namespace IntegrationTests
{
    /// <summary>
    /// 
    /// </summary>
    [CollectionDefinition("ServicesLaunch")]
    public class ServiceCommandCollection : ICollectionFixture<Fonlow.Testing.ServiceCommandsFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
