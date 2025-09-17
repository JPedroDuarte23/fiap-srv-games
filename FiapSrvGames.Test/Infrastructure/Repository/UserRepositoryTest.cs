using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiapSrvGames.Domain.Entities;
using FiapSrvGames.Infrastructure.Repository;
using MongoDB.Driver;
using Moq;
using Xunit;

public class UserRepositoryTest
{
    private readonly Mock<IMongoCollection<User>> _collectionMock;
    private readonly Mock<IMongoDatabase> _databaseMock;
    private readonly UserRepository _repository;

    public UserRepositoryTest()
    {
        _collectionMock = new Mock<IMongoCollection<User>>();
        _databaseMock = new Mock<IMongoDatabase>();

        var indexManagerMock = new Mock<IMongoIndexManager<User>>();
        _collectionMock.SetupGet(c => c.Indexes).Returns(indexManagerMock.Object);

        _databaseMock.Setup(db => db.GetCollection<User>("users", null)).Returns(_collectionMock.Object);

        _repository = new UserRepository(_databaseMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CallsInsertOneAsync()
    {
        var user = new Player { Id = Guid.NewGuid(), Email = "test@test.com" };
        _collectionMock.Setup(c => c.InsertOneAsync(user, null, default)).Returns(Task.CompletedTask);

        await _repository.CreateAsync(user);

        _collectionMock.Verify(c => c.InsertOneAsync(user, null, default), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser()
    {
        var user = new Player { Id = Guid.NewGuid(), Email = "test@test.com" };

        var cursorMock = new Mock<IAsyncCursor<User>>();
        cursorMock.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(new List<User> { user });

        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        var result = await _repository.GetByEmailAsync(user.Email);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
    }


    [Fact]
    public async Task GetByIdAsync_ReturnsUser()
    {
        var user = new Player { Id = Guid.NewGuid(), Email = "test@test.com" };

        var cursorMock = new Mock<IAsyncCursor<User>>();
        cursorMock.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(new List<User> { user });

        _collectionMock
            .Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }



    [Fact]
    public async Task UpdateAsync_CallsReplaceOneAsync()
    {
        var user = new Player { Id = Guid.NewGuid(), Email = "test@test.com" };
        _collectionMock.Setup(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<User>>(),
            user,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ReplaceOneResult>());

        await _repository.UpdateAsync(user);

        _collectionMock.Verify(c => c.ReplaceOneAsync(
            It.IsAny<FilterDefinition<User>>(),
            user,
            It.IsAny<ReplaceOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOneAsync()
    {
        var userId = Guid.NewGuid();
        _collectionMock.Setup(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<User>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<DeleteResult>());

        await _repository.DeleteAsync(userId);

        _collectionMock.Verify(c => c.DeleteOneAsync(
            It.IsAny<FilterDefinition<User>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
