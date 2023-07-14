using System;
using System.Collections.Generic;

namespace XXX
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CarService carService = new CarService(10, 200000, 25000, 5000);
            carService.Work();
        }
    }

    abstract class UserUtils
    {
        private static Random _random = new Random();

        public static int GenerateRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static int GenerateRandomNumber(int max)
        {
            return _random.Next(max);
        }
    }

    class CarService
    {
        private List<Part> _parts = new List<Part>();
        private Queue<Car> _cars = new Queue<Car>();
        private Database _databaseParts = new Database();
        private int _priceForfeit;
        private int _money;
        private int _freeSpaceInStorage;
        private bool _isBankrupt;
        private int _pricePartReplacement;

        public CarService(int freeSpaceInStorage, int startMoneyBalance, int priceForfeit, int pricePartReplacement)
        {
            _freeSpaceInStorage = freeSpaceInStorage;
            _money = startMoneyBalance;
            GenerateQueueCar();
            _priceForfeit = priceForfeit;
            _isBankrupt = false;
            _pricePartReplacement = pricePartReplacement;
        }

        public void Work()
        {
            while (_cars.Count > 0 && _isBankrupt == false)
            {
                ShowMenu();
                Console.Write($"В ваш сервис обратился клиент!\nНажмите чтобы произвести диагностику его авто!");
                Console.ReadKey();
                Part brokenPart = _cars.Peek().GetBrokenPart();

                Console.WriteLine($"Диагностика показала наличие следующей неисправности: {brokenPart.Name}");
                Console.WriteLine("Запчасти на вашем складе:");
                ShowPartsInfo(_parts);

                if (IsStorageContains(brokenPart))
                {
                    Console.Write("Введите индекс запчасти со склада которую желаете установить клиенту: ");

                    if (int.TryParse(Console.ReadLine(), out int index) && index <= _parts.Count && index > 0)
                    {
                        Part part = GetPartFromSrorage(index);

                        Console.WriteLine($"Запчасть {part.Name} выбрана! За установку вы получите: {_pricePartReplacement}!");
                        Console.WriteLine("Нажмите чтобы установить!");
                        Console.ReadKey();

                        MakeCalculation(RepairCar(part), brokenPart.Price, _pricePartReplacement);
                    }
                    else
                    {               
                        Console.WriteLine($"Вы не смогли разобраться на складе и найти запчасть! Вам возложен штраф {_priceForfeit}!");
                        PayForfeit();
                    }
                }
                else
                {
                    Console.WriteLine($"На вашем складе нет нужной запчасти! Вам возложен штраф {_priceForfeit}!");
                    PayForfeit();
                }
            }

            if (_cars.Count > 0 && _isBankrupt == false)
            {
                Console.WriteLine($"Вы успешно закончили рабочий день! Все машины обслужены!\nВаш баланс: {_money}");
            }
        }

        private bool RepairCar(Part part)
        {
            return _cars.Peek().ReplacementNeededPart(part);           
        }

        private void MakeCalculation(bool thisRepairCompletedSuccessfully, int partPrice, int repairPrice)
        {
            if (thisRepairCompletedSuccessfully)
            {
                Console.WriteLine("Вы успешно отремонтировали авто!");
                TakeMoney(partPrice + repairPrice);
                Console.Write($"Вы получаете: {partPrice + repairPrice}");
                Console.WriteLine($"Ваш баланс: {_money}");
                Console.ReadKey();
                Console.Clear();
                _cars.Dequeue();
            }
            else
            {
                Console.WriteLine($"Вы поменяли клиенту не ту запчасть! Вам возложен штраф в размере цены за ремонт: {_priceForfeit}");
                PayForfeit();
            }
        }

        private Part GetPartFromSrorage(int indexPart)
        {
            if (indexPart <= _parts.Count && indexPart > 0)
            {
                foreach (Part part in _parts)
                {
                    if (part.Index == indexPart)
                    {
                        _parts.Remove(part);
                        _freeSpaceInStorage++;

                        return part;
                    }
                }
            }

            return null;
        }

        private bool IsStorageContains(Part neededPart)
        {
            foreach (Part part in _parts)
            {
                if (part.Name == neededPart.Name)
                    return true;
            }

            return false;
        }

        private void BuyPart()
        {
            List<Part> allparts = _databaseParts.GetParts();

            Console.WriteLine($"Пополнение склада запчастей! Свободного места на вашем складе: {_freeSpaceInStorage} ячеек!\n");
            Console.WriteLine("Ассортимент:");
            _databaseParts.ShowAllParts();

            Console.Write("\nВведите индекс запчасти, которую желаете купить: ");

            if (int.TryParse(Console.ReadLine(), out int index))
            {
                if (_freeSpaceInStorage > 0)
                {
                    foreach (Part part in allparts)
                    {
                        if (part.Index == index)
                        {
                            if (PaymentMade(part.Price))
                            {
                                _parts.Add(part);
                                _freeSpaceInStorage--;
                                Console.WriteLine("Запчасть успешно приобретена!");
                                Console.WriteLine($"Осталось места в хранилище: {_freeSpaceInStorage}");
                            }
                            else
                            {
                                Console.WriteLine("Вам недостаточно денег!");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("На вашем складе нет свободного места!");
                }
            }
            else
            {
                Console.WriteLine("Не корректное число!");
            }
        }

        private void ShowMenu()
        {
            const string MenuBuyPart = "1";
            const string MenuShowPartsInStorage = "2";
            const string MenuExit = "3";

            bool inProcessShoping = true;

            while (inProcessShoping)
            {
                Console.WriteLine($"Магазин запчастей! Ваш баланс: {_money}\n");
                Console.WriteLine($"Введите {MenuBuyPart} чтобы совершить покупку!\nВведите {MenuShowPartsInStorage} чтобы показать запчасти в хранилище!\nВведите {MenuExit} чтобы выйти из меню и приступить к работе!");
                string userInput = Console.ReadLine();
                Console.Clear();

                switch (userInput)
                {
                    case MenuBuyPart:
                        BuyPart();
                        break;

                    case MenuExit:
                        inProcessShoping = false;
                        break;

                    case MenuShowPartsInStorage:
                        ShowPartsInfo(_parts);
                        break;

                    default:
                        Console.WriteLine("Такой команды нет в списке!");
                        break;
                }

                Console.ReadKey();
                Console.Clear();
            }

            Console.Clear();
        }

        private void ShowPartsInfo(List<Part> parts)
        {
            if (parts.Count > 0)
            {
                foreach (Part part in parts)
                {
                    Console.WriteLine($"{part.Name} стоимостью: {part.Price} индекс: {part.Index}");
                }
            }
            else
            {
                Console.WriteLine("Склад пуст!");
            }
        }

        private bool PaymentMade(int price)
        {
            if (_money >= price)
            {
                _money -= price;
                return true;
            }

            return false;
        }

        private void PayForfeit()
        {
            if (PaymentMade(_priceForfeit))
            {
                Console.WriteLine("Штраф оплачен!");
                Console.WriteLine("Клиент обиделся и ушел!");
                Console.WriteLine($"Ваш баланс: {_money}");
                Console.ReadKey();
                Console.Clear();
                _cars.Dequeue();
            }
            else
            {
                Console.WriteLine("Вам не хватает денег для оплаты штрафа! Вы банкрот! Игра окончена!");
                Console.ReadKey();
                _isBankrupt = true;
            }
        }

        private void TakeMoney(int price)
        {
            if (price >= 0)
                _money += price;           
        }

        private void GenerateQueueCar()
        {
            int minRandomNumber = 6;
            int maxRandomNumber = 25;
            int lengthQueue = UserUtils.GenerateRandomNumber(minRandomNumber, maxRandomNumber);

            for (int i = 0; i < lengthQueue; i++)
            {
                _cars.Enqueue(new Car());
            }
        }
    }

    class Part
    {
        private static int _indices;

        public Part(string name, int price)
        {
            Name = name;
            Price = price;
            IsServiceable = true;
            Index = ++_indices;
        }

        public string Name { get; private set; }
        public int Price { get; private set; }
        public bool IsServiceable { get; private set; }
        public int Index { get; private set; }

        public void Break()
        {
            IsServiceable = false;
        }
    }

    class Database
    {
        private List<Part> _parts;

        public Database()
        {
            CreateParts();
        }

        public List<Part> GetParts()
        {
            List<Part> parts = new List<Part>();

            foreach (Part part in _parts)
            {
                parts.Add(part);
            }

            return parts;
        }

        public void ShowAllParts()
        {
            foreach (Part part in _parts)
            {
                Console.WriteLine($"{part.Name} стоимостью: {part.Price} индекс: {part.Index}");
            }
        }

        private void CreateParts()
        {
            _parts = new List<Part>()
            {
                new Part("ремень ГРМ", 799),
                new Part("ступичный подшипник", 2299),
                new Part("прокладка ГБЦ", 380),
                new Part("головка ГБЦ", 14999),
                new Part("бензонасос", 4999),
                new Part("рулевые тяги", 6700)
            };
        }
    }

    class Car
    {
        private List<Part> _parts;
        private Database _dataBaseParts = new Database();

        public Car()
        {
            _parts = _dataBaseParts.GetParts();
            BrakeRandomPart();
        }

        public Part GetBrokenPart()
        {
            foreach (Part part in _parts)
            {
                if (part.IsServiceable == false)
                    return part;
            }

            return null;
        }

        public bool ReplacementNeededPart(Part newPart)
        {
            Part brokenPart = GetBrokenPart();

            if (brokenPart.Name == newPart.Name)
            {
                _parts.Remove(brokenPart);
                _parts.Add(newPart);
                return true;
            }

            return false;
        }

        private void BrakeRandomPart()
        {
            _parts[UserUtils.GenerateRandomNumber(_parts.Count)].Break();
        }
    }
}
