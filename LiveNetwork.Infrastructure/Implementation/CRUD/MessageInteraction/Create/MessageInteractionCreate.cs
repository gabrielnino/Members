using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using LiveNetwork.Application.UseCases.CRUD.IMessageInteraction;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using Persistence.Context.Interface;

namespace LiveNetwork.Infrastructure.Implementation.CRUD.MessageInteraction.Create
{
    using MessageInteraction = Domain.MessageInteraction;
    public class MessageInteractionCreate(IUnitOfWork unitOfWork, IErrorHandler errorHandler, IErrorLogCreate errorLogCreate, IMessageInteractionRead profileRead) : CreateRepository<MessageInteraction>(unitOfWork), IMessageInteractionCreate
    {
        public async Task<Operation<MessageInteraction>> CreateMessageInteractionAsync(MessageInteraction entity)
        {
            try
            {
                await CreateEntity(entity);
                await unitOfWork.CommitAsync();
                return Operation<MessageInteraction>.Success(entity);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<MessageInteraction>(ex, errorLogCreate);
            }
        }
    }
}
