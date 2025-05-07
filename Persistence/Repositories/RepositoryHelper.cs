namespace Persistence.Repositories
{
    /// <summary>
    /// The reposity helper
    /// </summary>
    public abstract class RepositoryHelper
    {
        /// <summary>
        /// Validate the arguments
        /// </summary>
        /// <typeparam name="E">The param to validate</typeparam>
        /// <param name="entity">The entity to vlidate</param>
        /// <returns>The entite validate.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static E ValidateArgument<E>(E? entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return entity;
        }
    }
}
