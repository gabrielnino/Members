using Application.Result;
using LiveNetwork.Domain;

namespace LiveNetwork.Application.UseCases.CRUD.IMessageInteraction
{
    public interface IMessageInteractionCreate
    {
        Task<Operation<MessageInteraction>> CreateMessageInteractionAsync(MessageInteraction entity);
    }
}
