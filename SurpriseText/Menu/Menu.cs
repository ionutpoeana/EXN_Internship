using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Contracts;
using ExtensionMethods;
using Microsoft.Extensions.Logging;
using static System.Console;

namespace SurpriseText
{
    public partial class Menu
    {
        private IRepository<Vehicle> _repository;
        private Type _vehicleType;
        private readonly IList<string> _files;
        private readonly ILogger<Menu> _logger;
        private readonly IDictionary<string, FileList> _fileOperations;

        public Menu(IList<string> files, ILogger<Menu> logger)
        {
            _files = files;
            _logger = logger;
            _fileOperations = new Dictionary<string, FileList>();

            var index = FileList.FIRST_FILE;

            foreach (var file in files)
            {
                _fileOperations.Add(file, index);
                ++index;
            }
        }
        public void Run()
        {
            _logger.LogInformation("Start of application!");
            var operation = MenuOperations.SELECT_FILE;
            Vehicle vehicle = null;


            while (operation != MenuOperations.EXIT)
            {   
                Clear();
                operation = GetOperation(_menuOperations);

                switch (operation)
                {
                    case MenuOperations.SELECT_FILE:
                        {
                            var filepath = SelectFile(_files);

                            _logger.LogInformation($"Selected file {filepath} for parsing!");

                            var showroom = new XmlParser<Vehicle>(_logger);
                            showroom.ReadFile(filepath);
                            _repository = new Repository<Vehicle>(showroom.GetAllVehicles(), filepath,
                                EntityContext<Vehicle>.Instance, _logger);
                            _vehicleType = showroom.GetType();

                            Clear();
                            WriteLine("File has been successfully selected!");
                            WriteLine($"You have a showroom full of {_vehicleType.Name}");
                            WriteLine("Press any key to continue!");
                            ReadKey();
                            break;
                        }
                    case MenuOperations.SELECT_VEHICLE:
                        {
                            if (IsRepositoryNull()) continue;

                            vehicle = SelectVehicle(_repository.GetAll());
                            Clear();
                            WriteLine($"You were the lucky {vehicle.ID}");
                            WriteLine("Press any key to do more operations!\n\n");
                            PrintVehicle(vehicle);
                            ReadKey();
                            break;
                        }
                    case MenuOperations.DELETE_VEHICLE:
                        {
                            if (IsRepositoryNull()) continue;
                            Clear();

                            if (vehicle == null)
                            {
                                WriteLine("You have to select a vehicle in order to delete it!");
                                WriteLine("Press any key to continue!");
                                ReadKey();
                                continue;
                            }

                            _repository.Delete(vehicle);
                            WriteLine($"Vehicle with ID {vehicle.ID} has been deleted!\nPress any key to continue!");
                            ReadKey();
                            vehicle = null;

                            break;
                        }
                    case MenuOperations.UPDATE_VEHICLE:
                        {
                            if (IsRepositoryNull()) continue;
                            Clear();
                            if (vehicle == null)
                            {
                                WriteLine("You have to select a vehicle in order to update it!");
                                WriteLine("Press any key to continue!");
                                ReadKey();
                                continue;
                            }
                            Clear();
                            var updateVehicle = UpdateVehicle(vehicle);
                            _repository.Update(updateVehicle);
                            WriteLine($"Vehicle with ID {vehicle.ID} has been updated!\nPress any key to continue!");
                            ReadKey();
                            break;
                        }
                    case MenuOperations.ADD_VEHICLE:
                        {
                            if (IsRepositoryNull()) continue;
                            vehicle = AddVehicle(_vehicleType);
                            Clear();
                            if (vehicle != null)
                            {
                                _repository.Add(vehicle);
                                WriteLine("The vehicle database has been successfully updated!");
                                WriteLine("Press any key to continue!");
                            }
                            else
                            {
                                WriteLine($"No new {_vehicleType.Name} has been added!");
                            }

                            ReadKey();
                            break;
                        }
                    case MenuOperations.SAVE:
                        {
                            if (IsRepositoryNull()) continue;
                            _repository.Commit();
                            Clear();
                            WriteLine("The vehicle database has been successfully updated!");
                            WriteLine("Press any key to continue!");
                            ReadKey();
                            break;
                        }
                    case MenuOperations.TEST_UNIT_OF_WORK:
                        TestUnitOfWork(_files);
                        break;
                    case MenuOperations.EXIT:
                        Clear();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _logger.LogInformation("End of application!");
        }

        private void TestUnitOfWork(IList<string> files)
        {
            var xmlParser = new XmlParser<Vehicle>(_logger);
            xmlParser.ReadFile(files[0]);
            var bikeRepository = new Repository<Vehicle>(xmlParser.GetAllVehicles(), files[0],
                EntityContext<Vehicle>.Instance, _logger);
            xmlParser.ReadFile(files[1]);
            var carRepository = new Repository<Vehicle>(xmlParser.GetAllVehicles(), files[1],
                EntityContext<Vehicle>.Instance, _logger);
            xmlParser.ReadFile(files[2]);
            var scooterRepository = new Repository<Vehicle>(xmlParser.GetAllVehicles(), files[2],
                EntityContext<Vehicle>.Instance, _logger);
            xmlParser.ReadFile(files[3]);
            var trainRepository = new Repository<Vehicle>(xmlParser.GetAllVehicles(), files[3],
                EntityContext<Vehicle>.Instance, _logger);


            var repositoryList = new List<IRepository<Vehicle>>
            {
                bikeRepository,
                carRepository,
                scooterRepository,
                trainRepository
            };

            IUnitOfWork unitOfWork = new UnitOfWork<Vehicle>(repositoryList, EntityContext<Vehicle>.Instance, _logger);

            var bike = new Bike
            {
                Color = "redish",
                CreatedOn = DateTime.Today,
                IsForChildren = true,
                ModelDescription = "ceva trotineata",
                Nickname = "trotineta",
                Price = 100
            };

            var minId = bikeRepository.GetAll().Min(b => b.ID);
            bikeRepository.Delete(bikeRepository.Get(v => v.ID == minId));
            bikeRepository.Add(bike);


            var car = new Car
            {
                Mileage = 200,
                Model = "Tesla model S",
                Price = 22,
                RegistrationDate = DateTime.Now,
                Vin = "212312213ew"
            };
            carRepository.Add(car);

            minId = carRepository.GetAll().Min(c => c.ID);
            var carToBeDeleted = carRepository.Get(p => p.ID == minId);
            carRepository.Delete(carToBeDeleted);

            unitOfWork.Commit();
        }

        private bool IsRepositoryNull()
        {
            if (_repository != null) return false;

            Clear();
            WriteLine("Please choose a file before doing any other operations!");
            WriteLine("Press any key to do more operations!");
            ReadKey();
            return true;

        }

        private Vehicle UpdateVehicle(Vehicle vehicle)
        {
            var copyOfVehicle = vehicle.DeepClone();

            Clear();
            WriteLine("Introduce values for vehicle properties!");
            WriteLine("Press ENTER to validate!");
            WriteLine("Press ESCAPE to exit without modifying the vehicle!\n");
            const int offset = 4;

            var index = offset;

            var vehicleProperties = vehicle.GetType().GetProperties();
            var leftOffset = vehicleProperties.Max(p => p.Name.Length + p.PropertyType.Name.Length) + 3;
            var left = new int[vehicleProperties.Length];

            for (int i = 0; i < vehicleProperties.Length; ++i)
            {
                var property = vehicleProperties[i];

                if (property.Name.ToLower() == "id")
                    continue;

                Write($"{property.Name}[{property.PropertyType.Name}]:");

                int j = property.Name.Length + property.PropertyType.Name.Length + 3;
                for (; j < leftOffset; ++j)
                {
                    Write(' ');
                }

                var propertyValue = property.GetValue(vehicle)?.ToString();
                if (propertyValue != null)
                {
                    left[i] = propertyValue.Length;
                }

                WriteLine(propertyValue);
            }

            SetCursorPosition(leftOffset + left[0], index);
            var consoleKeyInfo = new ConsoleKeyInfo();

            while (consoleKeyInfo.Key != ConsoleKey.Enter)
            {
                consoleKeyInfo = ReadKey();

                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.DownArrow:
                        ++index;
                        if (index >= (vehicleProperties.Length + offset - 1))
                            --index;
                        break;
                    case ConsoleKey.UpArrow:
                        --index;
                        if (index < offset)
                            ++index;
                        break;
                    case ConsoleKey.Backspace:
                        if (left[index - offset] > 0)
                        {
                            SetCursorPosition(leftOffset + left[index - offset], index);
                            Write(' ');
                            left[index - offset]--;
                        }
                        break;
                    case ConsoleKey.Enter:
                        index = offset;
                        int i = 0;
                        var sb = new StringBuilder();
                        var stdout = GetStdHandle(-11);
                        for (; i < vehicleProperties.Length; ++i)
                        {
                            if(vehicleProperties[i].Name.ToLower() == "id")
                                continue;

                            sb.Length = 0;
                            try
                            {
                                for (int j = leftOffset; j < leftOffset + left[i]; ++j)
                                {
                                    var coord = ((i + offset) << 16) | j;
                                    ReadConsoleOutputCharacterW(stdout, out char ch, 1, (uint)coord, out _);
                                    SetCursorPosition(i + offset, j);
                                    sb.Append(ch);
                                }

                                var smth = sb.ToString();

                                vehicleProperties[i].SetValue(vehicle, Convert.ChangeType(sb.ToString(), vehicleProperties[i].PropertyType));
                            }
                            catch (Exception e)
                            {
                                break;
                            }
                        }

                        if (i == vehicleProperties.Length)
                        {
                            SetCursorPosition(0, vehicleProperties.Length + offset);
                            WriteLine("Vehicle is valid!");
                            WriteLine("Press any key to continue!");
                            Read();
                            return vehicle;
                        }
                        else
                        {
                            SetCursorPosition(0, vehicleProperties.Length + offset);
                            WriteLine("Vehicle is not valid. Please introduce valid values for vehicle properties!");
                        }

                        break;
                    case ConsoleKey.Escape:
                        return copyOfVehicle;

                    default:
                        if (char.IsLetterOrDigit(consoleKeyInfo.KeyChar))
                        {
                            left[index - offset]++;
                        }
                        break;
                }

                SetCursorPosition(leftOffset + left[index - offset], index);
            }


            return null;
        }

        private static Vehicle AddVehicle(Type vehicleType)
        {
            Clear();
            WriteLine("Introduce values for vehicle properties!");
            WriteLine("Press ENTER to validate!");
            WriteLine("Press ESCAPE to exit!\n");
            const int offset = 4;

            var index = offset;

            var vehicleProperties = vehicleType.GetProperties();
            var leftOffset = vehicleProperties.Max(p => p.Name.Length + p.PropertyType.Name.Length)+ 3;

            foreach (var property in vehicleProperties)
            {
                if (property.Name.ToLower() != "id")
                    WriteLine($"{property.Name}[{property.PropertyType.Name}]:");
            }

            SetCursorPosition(leftOffset, index);
            var consoleKeyInfo = new ConsoleKeyInfo();

            var left = new int[vehicleProperties.Length];

            var vehicle = (Vehicle)Activator.CreateInstance(vehicleType);
            var isVehicleValid = false;

            while (consoleKeyInfo.Key != ConsoleKey.Enter || !isVehicleValid)
            {
                consoleKeyInfo = ReadKey();

                switch (consoleKeyInfo.Key)
                {
                    case ConsoleKey.DownArrow:
                        ++index;
                        if (index >= (vehicleProperties.Length + offset - 1))
                            --index;
                        break;
                    case ConsoleKey.UpArrow:
                        --index;
                        if (index < offset)
                            ++index;
                        break;
                    case ConsoleKey.Backspace:
                        if (left[index - offset] > 0)
                        {
                            SetCursorPosition(leftOffset + left[index - offset], index);
                            Write(' ');
                            left[index - offset]--;
                        }
                        break;
                    case ConsoleKey.Enter:
                        {
                            int i = 0;
                            var sb = new StringBuilder();
                            var stdout = GetStdHandle(-11);
                            for (; i < vehicleProperties.Length; ++i)
                            {
                                if (vehicleProperties[i].Name.ToLower() == "id")
                                    continue;

                                sb.Length = 0;
                                try
                                {
                                    for (var j = leftOffset; j < leftOffset + left[i]; ++j)
                                    {
                                        int coord = ((i + offset) << 16) | j;
                                        ReadConsoleOutputCharacterW(stdout, out char ch, 1, (uint)coord, out _);
                                        SetCursorPosition(i + offset, j);
                                        sb.Append(ch);
                                    }

                                    vehicleProperties[i].SetValue(vehicle,
                                        Convert.ChangeType(sb.ToString(), vehicleProperties[i].PropertyType));
                                }
                                catch (Exception e)
                                {
                                    break;
                                }
                            }

                            if (i == vehicleProperties.Length)
                            {
                                isVehicleValid = true;
                                SetCursorPosition(0, vehicleProperties.Length + offset);
                                WriteLine("Vehicle is valid!");
                                WriteLine("Press any key to continue!");
                                Read();
                                return vehicle;
                            }

                            SetCursorPosition(0, vehicleProperties.Length + offset);
                            WriteLine("Vehicle is not valid. Please introduce valid values for vehicle properties!");

                            break;
                        }
                    case ConsoleKey.Escape:
                        return null;
                    default:
                        if (char.IsLetterOrDigit(consoleKeyInfo.KeyChar) || consoleKeyInfo.KeyChar == '/')
                        {
                            left[index - offset]++;
                        }
                        break;
                }

                SetCursorPosition(leftOffset + left[index - offset], index);
            }


            return null;
        }

        private static Vehicle SelectVehicle(IEnumerable<Vehicle> vehicles)
        {
            var maxId = vehicles.Max(p => p.ID);
            var minId = vehicles.Min(p => p.ID);
            var vehicleId = int.MinValue;

            Clear();
            WriteLine($"Surprise box, take a number between [{minId},{maxId}]");
            Write("Number:");

            while (true)
            {
                int.TryParse(ReadLine(), out vehicleId);

                if (vehicleId < minId ||
                    vehicleId > maxId ||
                    vehicles.FirstOrDefault(v => v.ID == vehicleId) == null)
                {
                    WriteLine($"Not a valid number, take a number between [{minId},{maxId}]");
                    Write("Number:");
                    continue;
                }

                break;
            }

            return vehicles.FirstOrDefault(v => v.ID == vehicleId);
        }

        private string SelectFile(IList<string> files)
        {
            var file = GetOperation(_fileOperations);
            return _fileOperations.FirstOrDefault(p => p.Value == file).Key;
        }

        private static T GetOperation<T>(IDictionary<string, T> operations)
        {
            Clear();
            WriteLine("Showroom");
            WriteLine("Press ENTER to validate selection.\n");

            var operationsList = operations.Keys.ToList();
            PrintMenu(operationsList);
            var leftOffset = operations.Keys.Max(o => o.Length) + 1;
            var key = ConsoleKey.Escape;
            const int offset = 3;
            var index = offset;
            SetCursorPosition(leftOffset, index);
            while (key != ConsoleKey.Enter)
            {
                key = ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.DownArrow:
                        ++index;
                        if (index >= (operationsList.Count + offset))
                            --index;
                        break;
                    case ConsoleKey.UpArrow:
                        --index;
                        if (index < offset)
                            ++index;
                        break;
                }
                SetCursorPosition(leftOffset, index);
            }

            return operations[operationsList[index - offset]];
        }

        private static void PrintMenu(IList<string> menuItems)
        {
            if (menuItems == null) throw new ArgumentNullException(nameof(menuItems));

            foreach (var item in menuItems)
            {
                WriteLine(item);
            }
        }

        private static void PrintVehicle(Vehicle vehicle)
        {
            var vehicleProperties = vehicle.GetType().GetProperties();
            WriteLine($"Vehicle type: {vehicle.GetType().Name}");

            foreach (var prop in vehicleProperties)
            {
                WriteLine($"{prop.Name}: {prop.GetValue(vehicle)}");
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr GetStdHandle(int num);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)] //   ̲┌───────────────────^
        static extern bool ReadConsoleOutputCharacterW(
            IntPtr hStdout,   // result of 'GetStdHandle(-11)'
            out Char ch,      // U̲n̲i̲c̲o̲d̲e̲ character result
            uint c_in,        // (set to '1')
            uint coord_XY,    // screen location to read, X:loword, Y:hiword
            out uint c_out);  // (unwanted, discard)
    }
}