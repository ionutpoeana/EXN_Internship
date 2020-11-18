﻿using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using SurpriseText.Command;

namespace SurpriseText
{
    public class EntityContext<T> where T : Vehicle 
    {
        private readonly IDictionary<string, string> _fileLocation = new Dictionary<string, string>();
        private readonly Stack<Command<T>> _operationStack = new Stack<Command<T>>();
        public IDictionary<string, IList<T>> Repositories { get; } = new Dictionary<string, IList<T>>();

        public void AddOperation(Command<T> operation)
        {
            _operationStack.Push(operation);
        }

        public void AddRepository(Type entityType, IList<T> entities, string filePath)
        {

            if(Repositories.ContainsKey(entityType.Name)) return;

            Repositories.Add(entityType.Name, entities);
            _fileLocation.Add(entityType.Name, filePath);
        }


        public void SaveChanges()
        {
            try
            {
                foreach (var (entityName, entities) in Repositories)
                {
                    XmlParser<T>.WriteToFile(_fileLocation[entityName],entities);
                }

            }
            catch (Exception e)
            {
                Rollback();
            }
           
        }

        private void Rollback()
        {
            while (_operationStack.Any())
            {
                _operationStack.Pop().Execute();
            }
        }
    }
}