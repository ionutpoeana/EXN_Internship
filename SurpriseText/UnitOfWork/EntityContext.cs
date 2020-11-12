using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;

namespace SurpriseText
{
    public class EntityContext<T> where T : Vehicle
    {
        private readonly IDictionary<string, IList<(T entity, DmlOperation operation)>> _lastVersion;

        private static EntityContext<T> _entityContextInstance = null;

        public static EntityContext<T> Instance => _entityContextInstance ?? (_entityContextInstance = new EntityContext<T>());

        private EntityContext()
        {
            _lastVersion = new Dictionary<string, IList<(T entity, DmlOperation operation)>>();
        }

        public void AddLastVersion(T entity, DmlOperation operation)
        {
            var nameOfType = entity.GetType().Name;

            if (!_lastVersion.ContainsKey(nameOfType))
                _lastVersion[nameOfType] = new List<(T, DmlOperation)>();

            var entitiesVersionList = _lastVersion[nameOfType];

            switch (operation)
            {
                case DmlOperation.CREATE:
                    entitiesVersionList.Add((entity, operation));
                    break;
                case DmlOperation.UPDATE:
                    if (entitiesVersionList.Any(p => p.entity.ID == entity.ID &&
                                                     p.operation == DmlOperation.CREATE))
                    {
                        entitiesVersionList.Remove(entitiesVersionList.FirstOrDefault(p => p.entity.ID == entity.ID));
                        entitiesVersionList.Add((entity, DmlOperation.CREATE));
                        break;
                    }

                    entitiesVersionList.Add((entity,operation));
                    break;
                case DmlOperation.DELETE:
                    var vehicle = entitiesVersionList.FirstOrDefault(p => p.entity.ID == entity.ID);
                    if (vehicle == default(ValueTuple<T, DmlOperation>))
                    {
                        entitiesVersionList.Add((entity, operation));
                        break;
                    }

                    if (vehicle.operation == DmlOperation.CREATE)
                    {
                        entitiesVersionList.Remove(vehicle);
                        break;
                    }

                    if (vehicle.operation == DmlOperation.UPDATE)
                        vehicle.operation = operation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        public IList<( T entity, DmlOperation operation)> GetLastVersion(Type type)
        {
            return _lastVersion.ContainsKey(type.Name) ? _lastVersion[type.Name] : null;
        }

        public void EraseLastVersion()
        {
            _lastVersion.Clear();
        }
    }
}