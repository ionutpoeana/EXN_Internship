using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using ExtensionMethods;
using Microsoft.Extensions.Logging;

namespace SurpriseText
{
    public class Repository<T> : IRepository<T> where T : Vehicle
    {
        private readonly IList<T> _vehicles;
        private readonly string _filePath;
        private readonly EntityContext<T> _context;
        private readonly ILogger<Menu> _logger;

        public Repository(IList<T> vehicles, string filePath, EntityContext<T> context, ILogger<Menu> logger)
        {
            _vehicles = vehicles;
            _filePath = filePath;
            _context = context;
            _logger = logger;
        }

        public T Get(Func<T, bool> func) => _vehicles.FirstOrDefault(func);

        public IEnumerable<T> GetAll() => _vehicles;

        public void Delete(T entity)
        {
            if (entity == null) return;

            var deletedEntity = _vehicles.FirstOrDefault(p => p.ID == entity.ID);
            if (deletedEntity == null)
                return;

            _vehicles.Remove(deletedEntity);

            _context.AddLastVersion(deletedEntity, DmlOperation.DELETE);
        }

        public void Update(T entity)
        {
            var entityToBeUpdated = _vehicles.FirstOrDefault(p => p.ID == entity.ID);

            if (entityToBeUpdated == null) return;

            _context.AddLastVersion(entityToBeUpdated.DeepClone(), DmlOperation.UPDATE);

            var entityProperties = entity.GetType().GetProperties();
            foreach (var prop in entityProperties)
            {
                prop.SetValue(entityToBeUpdated, prop.GetValue(entity));
            }
        }

        public void Add(T entity)
        {
            if(entity.ID == 0)
                entity.ID = _vehicles.Max(p => p.ID) + 1;

            _vehicles.Add(entity);

            _context.AddLastVersion(entity, DmlOperation.CREATE);
        }

        public int Commit()
        {
            try
            {
                var changedEntities = _context.GetLastVersion(_vehicles.FirstOrDefault()?.GetType());
                
                if (changedEntities == null) return 0;
               
                XmlParser<T>.WriteToFile(_filePath, _vehicles);
                return changedEntities.Count;

            }
            catch (Exception e)
            {
                _logger.LogCritical(e,$"Exception occurred when committing changes for {_vehicles[0].GetType().Name} repository!");
                return -1;
            }
        }
    }
}