using DanishCvr.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.DanishCvr;

[TestClass]
public partial class DanishCvrServiceTests : BaseTests
{
    private IDanishCvrService DanishCvrService => this.serviceProvider.GetService<IDanishCvrService>();
}