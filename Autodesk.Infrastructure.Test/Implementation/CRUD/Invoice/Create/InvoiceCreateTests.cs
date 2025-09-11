using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Persistence.Context.Interface;
using Application.Result;
using Application.Result.Error;
using Application.UseCases.Repository.UseCases.CRUD;
using Domain;

namespace Autodesk.Infrastructure.Implementation.CRUD.Invoice.Create.Tests
{
    public class InvoiceCreateTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DataContext _context;
        private readonly UnitOfWork _uow;
        private readonly InvoiceCreate _service;
        private readonly Mock<IErrorHandler> _mockErrorHandler;
        private readonly Mock<IErrorLogCreate> _mockErrorLogCreate;

        public InvoiceCreateTests()
        {
            // Set up in-memory SQLite for EF Core
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new DataContext(options, new SQLite());
            _context.Database.EnsureCreated();

            _uow = new UnitOfWork(_context);

            _mockErrorHandler     = new Mock<IErrorHandler>();
            _mockErrorLogCreate   = new Mock<IErrorLogCreate>();

            _service = new InvoiceCreate(
                _uow,
                _mockErrorHandler.Object,
                _mockErrorLogCreate.Object
            );
        }

        [Fact]
        public async Task CreateInvoiceAsync_Success_PersistsAndReturnsEntity()
        {
            // Arrange
            var invoice = new Domain.Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = "INV-1001",
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = "Acme Corp",
                TotalAmount   = 250m
            };

            // Act
            var result = await _service.CreateInvoiceAsync(invoice);

            // Assert: operation is successful and returns the same entity
            Assert.True(result.IsSuccessful);
            Assert.Equal(invoice, result.Data);

            // Assert: invoice was persisted in the database
            var persisted = await _context.Set<Domain.Invoice>().FindAsync(invoice.Id);
            Assert.NotNull(persisted);
            Assert.Equal("INV-1001", persisted.InvoiceNumber);
        }

        [Fact]
        public async Task CreateInvoiceAsync_DatabaseError_InvokesErrorHandler()
        {
            // Arrange: stub error handler to return a known failure
            var expectedFailure = Operation<Domain.Invoice>.Failure("db error", ErrorTypes.Database);
            _mockErrorHandler
                .Setup(h => h.Fail<Domain.Invoice>(It.IsAny<Exception>(), _mockErrorLogCreate.Object))
                .Returns(expectedFailure);

            // Create an invoice missing a required property (InvoiceNumber) to force a NOT NULL violation
            var badInvoice = new Domain.Invoice(Guid.NewGuid().ToString())
            {
                InvoiceNumber = null!,    // null will violate NOT NULL
                InvoiceDate   = DateTime.UtcNow,
                CustomerName  = "Test",
                TotalAmount   = 100m
            };

            // Act
            var result = await _service.CreateInvoiceAsync(badInvoice);

            // Assert: operation is the stubbed failure, and error handler was invoked exactly once
            Assert.False(result.IsSuccessful);
            Assert.Equal(expectedFailure, result);
            _mockErrorHandler.Verify(
                h => h.Fail<Domain.Invoice>(It.IsAny<Exception>(), _mockErrorLogCreate.Object),
                Times.Once
            );
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Close();
        }
    }
}
