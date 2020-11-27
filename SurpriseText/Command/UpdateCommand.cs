using System.Collections.Generic;
using System.Linq;
using Contracts;

namespace SurpriseText.Command
{
    public class UpdateCommand<T> : Command<T> where T : Vehicle
    {
        public UpdateCommand(IList<T> repository, T entity) : base(repository, entity)
        {
        }

        public override void Execute()
        {
            var entityToBeDeleted = Repository.FirstOrDefault(e => e.ID == Entity.ID);
            Repository.Remove(entityToBeDeleted);
            Repository.Add(Entity);
        }
    }
}