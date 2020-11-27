using System.Collections.Generic;
using Contracts;

namespace SurpriseText.Command
{
    public abstract class Command<T> where T : Vehicle
    {
        protected readonly IList<T> Repository;
        protected readonly T Entity;

        protected Command( IList<T> repository, T entity)
        {
            Entity = entity;
            Repository = repository;
        }

        public abstract void Execute();
    }
}
