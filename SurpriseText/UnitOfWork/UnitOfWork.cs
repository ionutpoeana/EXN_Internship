using System;
using System.Collections.Generic;
using Contracts;

namespace SurpriseText.UnitOfWork
{
    public class UnitOfWork<T> : IUnitOfWork where T : Vehicle
    {
        private readonly EntityContext<T> _entityContext = new EntityContext<T>();
        public IDictionary<string, IRepository<T>> Repositories { get; } = new Dictionary<string, IRepository<T>>();

        public void AddRepository(Type entityType, IList<T> entities, string filePath)
        {
            Repositories.TryAdd(entityType.Name,new Repository<T>(entityType,_entityContext));
            _entityContext.AddRepository(entityType,entities,filePath);
        }
            
        public void Commit()
        {
            _entityContext.SaveChanges();
        }
    }
}
