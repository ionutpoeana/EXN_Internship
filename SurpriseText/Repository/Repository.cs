using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using ExtensionMethods;
using SurpriseText.Command;

namespace SurpriseText
{
    public class Repository<T> : IRepository<T> where T : Vehicle
    {
        private readonly EntityContext<T> _context;
        private readonly string _typeName;
        public Repository( Type type, EntityContext<T> context)
        {
            _typeName = type.Name;
            _context = context;
        }

        public T Get(Func<T, bool> func) => _context.Repositories[_typeName].FirstOrDefault(func);

        public IEnumerable<T> GetAll() => _context.Repositories[_typeName];

        public void Delete(T entity)
        {
            if (entity == null) return;

            var deletedEntity = _context.Repositories[_typeName].FirstOrDefault(p => p.ID == entity.ID);
            if (deletedEntity == null) return;

            _context.Repositories[_typeName].Remove(deletedEntity);
            _context.AddOperation(new AddCommand<T>(_context.Repositories[_typeName],deletedEntity.DeepClone()));
        }

        public void Update(T entity)
        {
            var entityToBeUpdated = _context.Repositories[_typeName].FirstOrDefault(p => p.ID == entity.ID);

            if (entityToBeUpdated == null) return;

            _context.AddOperation(new UpdateCommand<T>(_context.Repositories[_typeName],entityToBeUpdated.DeepClone()));

            var entityProperties = entity.GetType().GetProperties();
            foreach (var prop in entityProperties)
            {
                prop.SetValue(entityToBeUpdated, prop.GetValue(entity));
            }

        }

        public void Add(T entity)
        {
            if(entity.ID == 0)
                entity.ID = _context.Repositories[_typeName].Max(p => p.ID) + 1;

            _context.Repositories[_typeName].Add(entity);

            _context.AddOperation(new DeleteCommand<T>(_context.Repositories[_typeName],entity.DeepClone()));
        }

    }
}