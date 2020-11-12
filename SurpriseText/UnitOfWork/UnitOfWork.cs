using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Contracts;
using Microsoft.Extensions.Logging;

namespace SurpriseText
{
    public class UnitOfWork<T> : IUnitOfWork where T : Vehicle
    {
        private const int MaxRetryNumber = 3;
        private readonly IList<IRepository<T>> _repositories;
        private readonly EntityContext<T> _context;
        private readonly ILogger<Menu> _logger;

        public UnitOfWork(IList<IRepository<T>> repositories, EntityContext<T> context, ILogger<Menu> logger)
        {
            _repositories = repositories;
            _context = context;
            _logger = logger;
        }

        public void Commit()
        {
            _logger.LogInformation("Starting commit for unit of work!");
            var index = 0;
            for (; index < _repositories.Count; ++index)
            {
                try
                {
                    if ((index + 1) % 2 == 0 && DateTime.Now.Minute % 2 == 0)
                        throw new IOException();

                    if (_repositories[index].Commit() < 0)
                        break;
                }
                catch (Exception e)
                {
                    var repositoryName = _repositories[index].GetAll().FirstOrDefault()?.GetType().Name;
                    _logger.LogCritical(e,$"Exception commiting for {repositoryName} repository!");
                    break;
                }
            }

            if (index == _repositories.Count)
            {
                _logger.LogInformation("Changes committed successfully");
                return;
            }


            for (var i = index; i >= 0; --i)
            {
                if (!_repositories[i].GetAll().Any() ||
                    _context.GetLastVersion(_repositories[i].GetAll().First().GetType()) == null)
                    continue;

                var repositoryType = _repositories[index].GetAll().FirstOrDefault()?.GetType();
                _logger.LogCritical($"Rollback changes for {repositoryType?.Name} repository!");

                var lastVersion = _context.GetLastVersion(repositoryType).ToList();

                Rollback(_repositories[i], lastVersion);
            }

            _context.EraseLastVersion();
        }

        private void Rollback(IRepository<T> repository, IList<(T, DmlOperation)> lastVersion)
        {
            foreach (var (entity, crudOperation) in lastVersion)
            {
                switch (crudOperation)
                {
                    case DmlOperation.CREATE:
                        repository.Delete(entity);
                        break;
                    case DmlOperation.UPDATE:
                        repository.Update(entity);
                        break;
                    case DmlOperation.DELETE:
                        repository.Add(entity);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            var repositoryName = repository.GetAll().FirstOrDefault()?.GetType().Name;

            for (int retryNumber = 0; retryNumber < MaxRetryNumber; ++retryNumber)
            {
                try
                {
                    repository.Commit();
                    return;
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);
                    _logger.LogCritical(e, $"Exception occurred when rolling-back changes for {repositoryName} repository!");
                }

            }

            var backupFilename = $"{repositoryName}_backup.xml";
            XmlParser<Vehicle>.WriteToFile(backupFilename, repository.GetAll());
            _logger.LogCritical($"Unable to rollback changes for {repositoryName} repository!");
        }
    }
}