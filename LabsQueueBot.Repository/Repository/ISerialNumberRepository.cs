using LabsQueueBot.DataAccess.Entities;

namespace LabsQueueBot.Repository.Repository;

public interface ISerialNumberRepository : IRepository<SerialNumber>
{
    public Task<IEnumerable<SerialNumber>> GetQueueBySubject(Subject subject, CancellationToken cancellationToken);
    public Task<IEnumerable<SerialNumber>> GetWaitingBySubject(Subject subject, CancellationToken cancellationToken);
    public Task SwapUsersInQueue(SerialNumber sn1, SerialNumber sn2, CancellationToken cancellationToken);
    public Task SaveRange(IEnumerable<SerialNumber> range, CancellationToken cancellationToken);
}