using FluentAssertions;
using FluentValidation;
using labs_lntu_web.DbContexts;
using labs_lntu_web.Models.DTO;
using labs_lntu_web.Services;
using Microsoft.EntityFrameworkCore;
using static labs_lntu_web.Models.DTO.ModifyHostRequest;

namespace labs_lntu_web.Tests {
    public class HostsServiceTests {
        protected ApplicationDbContext CreateDbContext() {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            return new ApplicationDbContext(options);
        }

        protected IValidator<ModifyHostRequest> CreateValidator() {
            return new ModifyHostRequestValidator();
        }

        protected HostsService CreateService(ApplicationDbContext context) {
            var fakeCache = new FakeHostsResultsTempStorage();
            var fakePingWorker = new FakePingWorker(null!, null!, fakeCache);
            return new HostsService(context, fakePingWorker, fakeCache, CreateValidator());
        }

        protected ModifyHostRequest CreateValidRequest() {
            return new ModifyHostRequest {
                HostName = "Test host",
                RemoteAddress = "127.0.0.1",
                Enabled = true
            };
        }
        protected ModifyHostRequest CreateInvalidRemoteAddressRequest() {
            return new ModifyHostRequest {
                HostName = "Invalid host",
                RemoteAddress = "--132dfs -- 213",
                Enabled = true
            };
        }

        [Fact]
        public async Task AddHostAsync_Should_Create_Host_When_Request_Is_Valid() {
            // arrange
            using var context = CreateDbContext();
            var service = CreateService(context);
            var request = CreateValidRequest();

            // act
            var created = await service.AddHostAsync(request);

            // assert
            created.Id.Should().BeGreaterThan(0);
            created.Name.Should().Be(request.HostName);
            created.RemoteAddress.Should().Be(request.RemoteAddress);
            created.Enabled.Should().BeTrue();

            var fromDb = await context.Hosts.FindAsync(created.Id);
            fromDb.Should().NotBeNull();
            fromDb!.Name.Should().Be(request.HostName);
        }

        [Fact]
        public async Task AddHostAsync_Should_Throw_Exception_When_Request_Is_Not_Valid() {
            // arrange
            using var context = CreateDbContext();
            var service = CreateService(context);
            var request = CreateInvalidRemoteAddressRequest();

            // act
            var act = async () => await service.AddHostAsync(request);

            // assert
            await act.Should().ThrowAsync<ValidationException>();
            (await context.Hosts.CountAsync()).Should().Be(0);
        }
    }
}
