using System.Collections.Generic;
using Contracts;

namespace SurpriseText.Command
{
    public class DeleteCommand<T> : Command<T> where T : Vehicle
    {
    

        public DeleteCommand(IList<T> repository, T entity) : base(repository, entity)
        {
        }

        public override void Execute()
        {
            Repository.Remove(Entity);
        }
    }
}