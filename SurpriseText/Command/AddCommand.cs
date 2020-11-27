using System.Collections.Generic;
using Contracts;

namespace SurpriseText.Command
{
    public class AddCommand<T> : Command<T> where T : Vehicle
    {
        public AddCommand(IList<T> repository, T entity) : base(repository, entity)
        {
        }

        public override void Execute()
        {
            Repository.Add(Entity);
        }
    }
}