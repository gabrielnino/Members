using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public abstract class EntityChecker<T>(DbContext context) : Read<T>(context) where T : class, IEntity
    {
        public virtual async Task<T?> HasEntity(string id)
        {
            var entityRepo = await ReadFilter(e => e.Id.Equals(id));
            var entityUnmodified = entityRepo?.FirstOrDefault();
            var hasEntity = entityUnmodified is not null;
            if (!hasEntity)
            {
                return null;
            }

            return entityUnmodified;
        }

        public virtual async Task<T?> HasId(string id)
        {

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var result = GuidValidator.HasGuid(id);
            if (!result)
            {
                return null;
            }

            return await HasEntity(id);
        }

        public class GuidValidator
        {
            public static bool HasGuid(string id)
            {
                bool isSuccess = Guid.TryParse(id, out _);
                if (!isSuccess)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
