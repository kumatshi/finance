using System;
using System.Collections.Generic;
using System.Globalization;

namespace finance_calculator.src
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                DisplayMenu();

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreditCalculator();
                        break;
                    case "2":
                        CurrencyConverter();
                        break;
                    case "3":
                        DepositCalculator();
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        static void DisplayMenu()
        {
            Console.WriteLine("=================================");
            Console.WriteLine("      ФИНАНСОВЫЙ КАЛЬКУЛЯТОР     ");
            Console.WriteLine("=================================");
            Console.WriteLine("1. Расчет кредита");
            Console.WriteLine("2. Конвертер валют");
            Console.WriteLine("3. Калькулятор вкладов");
            Console.WriteLine("4. Выход");
            Console.WriteLine("=================================");
            Console.Write("Выберите опцию: ");
        }

        static void CreditCalculator()
        {
            Console.Clear();
            Console.WriteLine("=== РАСЧЕТ КРЕДИТА ===");

            try
            {
                Console.Write("Введите сумму кредита (руб): ");
                decimal amount = decimal.Parse(Console.ReadLine());

                Console.Write("Введите срок кредита (месяцев): ");
                int months = int.Parse(Console.ReadLine());

                Console.Write("Введите процентную ставку (% годовых): ");
                decimal annualRate = decimal.Parse(Console.ReadLine());

                if (amount <= 0 || amount > 10000000)
                    throw new ArgumentException("Сумма кредита должна быть от 1 до 10,000,000 руб");

                if (months <= 0 || months > 360)
                    throw new ArgumentException("Срок кредита должен быть от 1 до 360 месяцев");

                if (annualRate <= 0 || annualRate >= 100)
                    throw new ArgumentException("Процентная ставка должна быть от 0.1 до 99.9%");

                decimal monthlyRate = annualRate / 100 / 12;
                decimal coefficient = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months) /
                                    ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1);
                decimal monthlyPayment = amount * coefficient;
                decimal totalAmount = monthlyPayment * months;
                decimal overpayment = totalAmount - amount;

                Console.WriteLine("\n=== РЕЗУЛЬТАТЫ РАСЧЕТА ===");
                Console.WriteLine($"Ежемесячный платеж: {monthlyPayment:F2} руб");
                Console.WriteLine($"Общая сумма выплат: {totalAmount:F2} руб");
                Console.WriteLine($"Переплата по кредиту: {overpayment:F2} руб");
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: Введены некорректные данные. Используйте числа.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static void CurrencyConverter()
        {
            Console.Clear();
            Console.WriteLine("=== КОНВЕРТЕР ВАЛЮТ ===");
            var exchangeRates = new Dictionary<string, decimal>
            {
                {"USD_RUB", 90.0m},
                {"EUR_RUB", 98.5m},
                {"EUR_USD", 1.09m},
                {"RUB_USD", 1/90.0m},
                {"RUB_EUR", 1/98.5m},
                {"USD_EUR", 1/1.09m}
            };

            try
            {
                Console.WriteLine("Доступные валюты: RUB, USD, EUR");
                Console.Write("Введите исходную валюту: ");
                string fromCurrency = Console.ReadLine().ToUpper();

                Console.Write("Введите целевую валюту: ");
                string toCurrency = Console.ReadLine().ToUpper();

                Console.Write("Введите сумму для конвертации: ");
                decimal amount = decimal.Parse(Console.ReadLine());

                string[] validCurrencies = { "RUB", "USD", "EUR" };
                if (Array.IndexOf(validCurrencies, fromCurrency) == -1 ||
                    Array.IndexOf(validCurrencies, toCurrency) == -1)
                {
                    throw new ArgumentException("Неверный код валюты. Используйте RUB, USD или EUR");
                }

                if (amount <= 0)
                    throw new ArgumentException("Сумма должна быть положительным числом");

                decimal result;
                if (fromCurrency == toCurrency)
                {
                    result = amount;
                }
                else
                {
                    string key = $"{fromCurrency}_{toCurrency}";
                    if (!exchangeRates.ContainsKey(key))
                        throw new ArgumentException("Конвертация для данной пары валют не поддерживается");

                    result = amount * exchangeRates[key];
                }

                Console.WriteLine($"\n=== РЕЗУЛЬТАТ КОНВЕРТАЦИИ ===");
                Console.WriteLine($"{amount:F2} {fromCurrency} = {result:F2} {toCurrency}");
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: Введены некорректные данные. Используйте числа для суммы.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static void DepositCalculator()
        {
            Console.Clear();
            Console.WriteLine("=== КАЛЬКУЛЯТОР ВКЛАДОВ ===");

            try
            {
                Console.Write("Введите сумму вклада (руб): ");
                decimal amount = decimal.Parse(Console.ReadLine());

                Console.Write("Введите срок вклада (месяцев): ");
                int months = int.Parse(Console.ReadLine());

                Console.Write("Введите процентную ставку (% годовых): ");
                decimal annualRate = decimal.Parse(Console.ReadLine());

                Console.Write("Тип вклада (1 - без капитализации, 2 - с капитализацией): ");
                string depositType = Console.ReadLine();
                if (amount <= 0 || amount > 10000000)
                    throw new ArgumentException("Сумма вклада должна быть от 1 до 10,000,000 руб");

                if (months <= 0 || months > 360)
                    throw new ArgumentException("Срок вклада должен быть от 1 до 360 месяцев");

                if (annualRate <= 0 || annualRate >= 100)
                    throw new ArgumentException("Процентная ставка должна быть от 0.1 до 99.9%");

                decimal income, totalAmount;

                if (depositType == "1")
                {
                    income = amount * annualRate * months / 12 / 100;
                    totalAmount = amount + income;
                }
                else if (depositType == "2") 
                {
                    decimal monthlyRate = annualRate / 100 / 12;
                    totalAmount = amount * (decimal)Math.Pow((double)(1 + monthlyRate), months);
                    income = totalAmount - amount;
                }
                else
                {
                    throw new ArgumentException("Неверный тип вклада. Выберите 1 или 2");
                }

                Console.WriteLine("\n=== РЕЗУЛЬТАТЫ РАСЧЕТА ===");
                Console.WriteLine($"Доход по вкладу: {income:F2} руб");
                Console.WriteLine($"Итоговая сумма: {totalAmount:F2} руб");
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: Введены некорректные данные. Используйте числа.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}