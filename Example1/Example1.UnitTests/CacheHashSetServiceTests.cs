#nullable disable

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Example1.UnitTests;

[TestClass]
public class CacheHashSetServiceTests
{
    private readonly MemoryCache _memoryCache;
    private readonly Mock<ILogger<CacheHashSetService>> _mockLogger;
    private readonly Mock<IRedisService> _mockRedisService;

    /// <summary>
    /// Constructor
    /// </summary>
    public CacheHashSetServiceTests()
    {
        _mockRedisService = new Mock<IRedisService>();
        _mockLogger = new Mock<ILogger<CacheHashSetService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions() { });
    }

    /// <summary>
    /// The fallback, which is usually the database, might not find the object in question. When it
    /// can't, null is returned.
    /// </summary>
    [TestMethod]
    public async Task CacheService_IfDataCannotBeFoundInTheFallbackNullIsReturnedAsync()
    {
        // Arrange
        var mockDataRetriever = new Mock<IMockDataRetriever<Employee>>();
        mockDataRetriever.Setup(t => t.GetDataAsync()).Returns(Task.FromResult((Employee)null));
        var classUnderTest = CreateClassUnderTest();

        // Act
        var actualResult = await classUnderTest.GetAsync<Employee>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());

        // Assert
        Assert.IsNull(actualResult);
        mockDataRetriever.Verify(t => t.GetDataAsync(), Times.Once);
    }

    /// <summary>
    /// Redis cache will expire at some point. If it expires, it will return null.
    /// This test assumes that some time has passed between each call.
    /// </summary>
    [TestMethod]
    public async Task CacheService_IfRedisContinuesToReturnNull_WeWillOnlyPullFromTheDatabaseEachTimeIfTheCallsAreMadeAtLeastMinimumSecondsBetweenRedisChecksApartAsync()
    {
        // Arrange
        _mockRedisService.Setup(s => s.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None))
            .ReturnsAsync(new HashEntry[0]);

        var mockDataRetriever = new Mock<IMockDataRetriever<string[]>>();
        var items = new string[] { "thing" };
        mockDataRetriever.Setup(t => t.GetDataAsync()).Returns(Task.FromResult(items));

        var classUnderTest = CreateClassUnderTest();

        // Act
        var firstCall = await classUnderTest.GetAsync<string[]>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        ChangeTheTimeWeLastCheckedRedis<string[]>("testKey", CacheHashSetService.MinimumSecondsBetweenRedisChecks);
        var secondCall = await classUnderTest.GetAsync<string[]>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        ChangeTheTimeWeLastCheckedRedis<string[]>("testKey", CacheHashSetService.MinimumSecondsBetweenRedisChecks);
        var thirdCall = await classUnderTest.GetAsync<string[]>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());

        // Assert
        Assert.IsNotNull(firstCall);
        Assert.AreEqual(items.Length, firstCall.Length);
        Assert.IsNotNull(secondCall);
        Assert.AreEqual(items.Length, secondCall.Length);
        Assert.IsNotNull(thirdCall);
        Assert.AreEqual(items.Length, thirdCall.Length);

        _mockRedisService.Verify(v => v.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None), Times.Exactly(3));
        mockDataRetriever.Verify(t => t.GetDataAsync(), Times.Exactly(3));
    }

    /// <summary>
    /// Redis cache will expire at some point. If it expires, it will return null. Here we are
    /// assuming that several calls are made one after another.  The memory cache will be tapped for the 2nd and 3rd call.
    /// </summary>
    [TestMethod]
    public async Task
        CacheService_IfRedisContinuesToReturnNull_WeWillOnlyPullFromTheDatabaseOnceIfSeveralCallsAreMadeBackToBackAsync()
    {
        // Arrange
        _mockRedisService.Setup(s => s.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None))
            .ReturnsAsync(new HashEntry[0]);

        var mockDataRetriever = new Mock<IMockDataRetriever<string[]>>();
        var items = new string[] { "thing" };
        mockDataRetriever.Setup(t => t.GetDataAsync()).Returns(Task.FromResult(items));

        var classUnderTest = CreateClassUnderTest();

        // Act
        var firstCall = await classUnderTest.GetAsync<string[]>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        var secondCall = await classUnderTest.GetAsync<string[]>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        var thirdCall = await classUnderTest.GetAsync<string[]>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());

        // Assert
        Assert.IsNotNull(firstCall);
        Assert.AreEqual(items.Length, firstCall.Length);
        Assert.IsNotNull(secondCall);
        Assert.AreEqual(items.Length, secondCall.Length);
        Assert.IsNotNull(thirdCall);
        Assert.AreEqual(items.Length, thirdCall.Length);

        _mockRedisService.Verify(v => v.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None), Times.Once);
        mockDataRetriever.Verify(t => t.GetDataAsync(), Times.Once);
    }

    /// <summary>
    /// The second hit on the cache would normally read from cache and not go to the database, but
    /// here we are making redis return a different sync id each time, which should force it to go
    /// to the database a second time. The third time; however, should get from the cache!
    /// </summary>
    [TestMethod]
    public async Task CacheService_IfRedisProvidesASyncIdDifferentFromCacheWeShouldRequestDataAgainAsync()
    {
        // Arrange
        var workerType1 = new Employee("1", "Worker 1", 34);
        var workerType2 = new Employee("1", "Worker 2", 34);

        var hashResult1 = new HashEntry[]
        {
            new HashEntry("syncId", "EDAD9CB7-747F-4369-8CA6-54BD3CA826F0"),
            new HashEntry("data", JsonConvert.SerializeObject(workerType1)),
        };
        var hashResult2 = new HashEntry[]
        {
            new HashEntry("syncId", "D4E3B48D-9A73-4DF8-9AC0-271392E6E3D6"),
            new HashEntry("data", JsonConvert.SerializeObject(workerType2)),
        };

        _mockRedisService.SetupSequence(s => s.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None))
            .ReturnsAsync(hashResult1)
            .ReturnsAsync(hashResult2)
            .ReturnsAsync(new HashEntry[0]);

        var mockDataRetriever = new Mock<IMockDataRetriever<Employee>>();
        var workerType3 = new Employee("1", "Worker 3", 34);
        mockDataRetriever.Setup(t => t.GetDataAsync()).Returns(Task.FromResult(workerType3));

        var classUnderTest = CreateClassUnderTest();

        // Act
        var firstCall = await classUnderTest.GetAsync<Employee>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        ChangeTheTimeWeLastCheckedRedis<Employee>("testKey", CacheHashSetService.MinimumSecondsBetweenRedisChecks);
        var secondCall = await classUnderTest.GetAsync<Employee>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        ChangeTheTimeWeLastCheckedRedis<Employee>("testKey", CacheHashSetService.MinimumSecondsBetweenRedisChecks);
        var thirdCall = await classUnderTest.GetAsync<Employee>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());

        // Assert
        Assert.IsNotNull(firstCall);
        Assert.AreEqual("Worker 1", firstCall.Name);
        Assert.IsNotNull(secondCall);
        Assert.AreEqual("Worker 2", secondCall.Name);
        Assert.IsNotNull(thirdCall);
        Assert.AreEqual("Worker 3", thirdCall.Name);

        mockDataRetriever.Verify(t => t.GetDataAsync(), Times.Once); // Redis had a result the first two times, but not the last.
        _mockRedisService.Verify(v => v.HashSetAsync(It.IsAny<RedisKey>(), It.IsAny<HashEntry[]>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    /// <summary>
    /// Redis cache is holding a value, so we should not go to the database to get data.
    /// </summary>
    [TestMethod]
    public async Task CacheService_ShouldReadFromCacheAsync()
    {
        // Arrange
        var cacheWorkerType = new Employee("1", "Worker 1", 36);
        var hashResult = new HashEntry[]
        {
            new HashEntry("syncId", "EDAD9CB7-747F-4369-8CA6-54BD3CA826F0"),
            new HashEntry("data", JsonConvert.SerializeObject(cacheWorkerType)),
        };

        _mockRedisService.Setup(s => s.HashGetAllAsync(It.IsAny<RedisKey>(), CommandFlags.None))
            .ReturnsAsync(hashResult);

        var mockDataRetriever = new Mock<IMockDataRetriever<Employee>>();
        var employee = new Employee("1", "My Worker", 34);
        mockDataRetriever.Setup(t => t.GetDataAsync()).Returns(Task.FromResult(employee));

        var classUnderTest = CreateClassUnderTest();

        // Act
        var firstCall = await classUnderTest.GetAsync<Employee>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());
        var secondCall = await classUnderTest.GetAsync<Employee>("testKey", TimeSpan.FromMinutes(5), () => mockDataRetriever.Object.GetDataAsync());

        // Assert
        Assert.IsNotNull(firstCall);
        Assert.AreEqual("Worker 1", firstCall.Name);

        Assert.IsNotNull(secondCall);
        Assert.AreEqual("Worker 1", secondCall.Name);

        mockDataRetriever.Verify(t => t.GetDataAsync(), Times.Never);
    }

    /// <summary>
    /// Makes it look like time has passed since the last call so we don't have to make the unit
    /// test wait for X seconds before calling the cache again.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="seconds">
    /// The number of seconds that should pass before calling the service again.
    /// </param>
    private void ChangeTheTimeWeLastCheckedRedis<T>(string key, int seconds)
    {
        var cachedData = _memoryCache.Get<CachedData<T>>(key);
        if (cachedData != null)
        {
            cachedData.LastTimeOutOfSyncWasChecked -= TimeSpan.FromSeconds(seconds);
        }
    }

    private CacheHashSetService CreateClassUnderTest()
    {
        return new CacheHashSetService(_mockLogger.Object, _memoryCache, _mockRedisService.Object);
    }
}

public interface IMockDataRetriever<T>
{
    Task<T> GetDataAsync();
}