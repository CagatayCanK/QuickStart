namespace QuickStart.Api.Tests.Security;

public class AuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<AuthenticationConfiguration>> _options;
    private readonly AuthenticationHandler _authenticationHandler;

    public AuthenticationHandlerTests()
    {
        var authenticationConfiguration = new AuthenticationConfiguration
        {
            ClaimsIssuer = "QuickStart",
            Subject = "QuickStart",
            ClientSecret = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJRdWlja1N0YXJ0IiwiaWF0IjpudWxsLCJleHAiOm51bGwsImF1ZCI6IiIsInN1YiI6IlF1aWNrU3RhcnQiLCJBZG1pbiI6IkFkbWluIn0.0atX-jHdAvPWo_6nZaARUAPihZK1lrEgByrB8VbZ8Wk"
        };
        _options = new Mock<IOptionsMonitor<AuthenticationConfiguration>>();
        _options
            .Setup(x => x.Get(AuthenticationConfiguration.AuthenticationScheme))
            .Returns(authenticationConfiguration);

        var encoder = new Mock<UrlEncoder>();
        var clock = new Mock<ISystemClock>();
        var logger = new Mock<ILogger<AuthenticationHandler>>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        _authenticationHandler = new AuthenticationHandler(_options.Object, loggerFactory.Object, encoder.Object, clock.Object);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithNoAuthorizationHeader_ShouldReturnNoResult()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues(String.Empty);
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Ticket);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithEmptyScheme_ShouldReturnNoResult()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("TestToken");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Ticket);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithInvalidToken_ShouldReturnNoResult()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Bearer TestToken");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Ticket);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithValidToken_ShouldReturnTicket()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJRdWlja1N0YXJ0IiwiaWF0IjpudWxsLCJleHAiOm51bGwsImF1ZCI6IiIsInN1YiI6IlF1aWNrU3RhcnQiLCJBZG1pbiI6IkFkbWluIn0.0atX-jHdAvPWo_6nZaARUAPihZK1lrEgByrB8VbZ8Wk");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.True(result.Succeeded);
        Assert.Equal(AuthenticationConfiguration.AuthenticationScheme, result.Ticket.AuthenticationScheme);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithInvalidIssuer_ShouldReturnNoResult()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJUZXN0IiwiaWF0IjpudWxsLCJleHAiOm51bGwsImF1ZCI6IiIsInN1YiI6IkJvaWxlcnBsYXRlIiwiQWRtaW4iOiJBZG1pbiJ9.DMfmvbeIT5TSWsHJc9EnvHqOut1YWg3Sd-DWdIfDwF0");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Ticket);
        Assert.Equal("Expected 'QuickStart' issuer, but got 'Test'.", result.Failure!.Message);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithInvalidSubject_ShouldReturnNoResult()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJRdWlja1N0YXJ0IiwiaWF0IjpudWxsLCJleHAiOm51bGwsImF1ZCI6IiIsInN1YiI6IlRlc3QiLCJBZG1pbiI6IkFkbWluIn0.r1WzUmOAINncwGKtcZwTENlSCQi0eyv8KOD9N8EbNjw");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Ticket);
        Assert.Equal("Expected 'QuickStart' subject, but got 'Test'.", result.Failure!.Message);
    }

    [Fact]
    public async Task AuthenticationHandler_HandleAuthenticateAsync_WithInvalidParameter_ShouldReturnNoResult()
    {
        //Arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJCb2lsZXJwbGF0ZSIsImlhdCI6bnVsbCwiZXhwIjpudWxsLCJhdWQiOiIiLCJzdWIiOiJCb2lsZXJwbGF0ZSIsIkFkbWluIjoiQWRtaW4ifQ.neLiDg-W93hngAoMltxDPyg-fdQZoUUYKN2tU5bc8_A");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);

        //Act
        await _authenticationHandler.InitializeAsync(new AuthenticationScheme(AuthenticationConfiguration.AuthenticationScheme, null, typeof(AuthenticationHandler)), context);
        var result = await _authenticationHandler.AuthenticateAsync();

        //Assert
        Assert.False(result.Succeeded);
        Assert.Null(result.Ticket);
    }
}
