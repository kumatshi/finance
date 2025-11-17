using Moq;
using Xunit;
using System;

namespace FinancialCalculator.Tests
{
    public class FinancialCalculatorTests
    {
        private readonly FinancialCalculator _calculator;

        public FinancialCalculatorTests()
        {
            _calculator = new FinancialCalculator();
        }

        // Тест 1:  расчет кредита с валидными данными
        [Fact]
        public void CalculateCredit_WithValidData_ReturnsCorrectResults()
        {
            decimal amount = 100000m;
            int months = 12;
            decimal annualRate = 10m;

            var result = _calculator.CalculateCredit(amount, months, annualRate);

            Assert.Equal(8791.59m, result.MonthlyPayment);
            Assert.Equal(5499.08m, result.Overpayment);
            Assert.Equal(105499.08m, result.TotalAmount);
        }

        // Тест 2: Теория с РЕАЛЬНЫМИ значениями из онлайн калькуляторов
        [Theory]
        [InlineData(100000, 12, 10, 8791.59, 5499.08, 105499.08)]
        [InlineData(50000, 6, 5, 8455.28, 731.68, 50731.68)]  
        [InlineData(200000, 24, 15, 9697.33, 32735.92, 232735.92)]  
        public void CalculateCredit_WithVariousData_ReturnsCorrectResults(
            decimal amount, int months, decimal annualRate,
            decimal expectedMonthly, decimal expectedOverpayment, decimal expectedTotal)
        {
            var result = _calculator.CalculateCredit(amount, months, annualRate);

            Assert.Equal(expectedMonthly, result.MonthlyPayment);
            Assert.Equal(expectedOverpayment, result.Overpayment);
            Assert.Equal(expectedTotal, result.TotalAmount);
        }

        // Тест 3: тест с Mock
        [Fact]
        public void CalculateCredit_WithMockLogger_LogsCorrectInformation()
        {
            var mockLogger = new Mock<ICalculationLogger>();
            var calculator = new FinancialCalculator(mockLogger.Object);

            decimal amount = 100000m;
            int months = 12;
            decimal annualRate = 10m;
            var result = calculator.CalculateCredit(amount, months, annualRate);

            mockLogger.Verify(x => x.LogCalculation(
                It.Is<string>(s => s.Contains("Кредитный расчет")),
                It.Is<decimal>(v => v == amount),
                It.Is<decimal>(v => v == 0m)      
            ), Times.Once);

            mockLogger.Verify(x => x.LogSuccess("Расчет завершен успешно"), Times.Once);
        }

        // Тест 4: Тест обработки исключений для некорректных данных
        [Theory]
        [InlineData(-1000, 12, 10)]
        [InlineData(100000, 0, 10)]
        [InlineData(100000, 12, -5)]
        [InlineData(10000001, 12, 10)]
        [InlineData(100000, 361, 10)]
        [InlineData(100000, 12, 101)]
        public void CalculateCredit_WithInvalidData_ThrowsArgumentException(
            decimal amount, int months, decimal annualRate)
        {
            Assert.Throws<ArgumentException>(() =>
                _calculator.CalculateCredit(amount, months, annualRate));
        }

        // Тест 5: Комплексный тест конвертера валют с Mock
        [Fact]
        public void ConvertCurrency_WithMockExchangeService_ReturnsCorrectResult()
        {
            var mockExchangeService = new Mock<IExchangeRateService>();
            mockExchangeService.Setup(x => x.GetExchangeRate("USD", "RUB"))
                .Returns(90.0m);

            var converter = new CurrencyConverter(mockExchangeService.Object);
            decimal amount = 100m;
            string fromCurrency = "USD";
            string toCurrency = "RUB";

            var result = converter.ConvertCurrency(amount, fromCurrency, toCurrency);

            Assert.Equal(9000m, result);
            mockExchangeService.Verify(x => x.GetExchangeRate("USD", "RUB"), Times.Once);
        }
    }

    public class CreditCalculationResult
    {
        public decimal MonthlyPayment { get; set; }
        public decimal Overpayment { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public interface ICalculationLogger
    {
        void LogCalculation(string operation, decimal input, decimal result);
        void LogSuccess(string message);
        void LogError(string error);
    }

    public interface IExchangeRateService
    {
        decimal GetExchangeRate(string fromCurrency, string toCurrency);
    }

    public class FinancialCalculator
    {
        private readonly ICalculationLogger _logger;

        public FinancialCalculator() : this(null) { }

        public FinancialCalculator(ICalculationLogger logger)
        {
            _logger = logger;
        }

        public CreditCalculationResult CalculateCredit(decimal amount, int months, decimal annualRate)
        {
            if (amount <= 0 || amount > 10000000)
                throw new ArgumentException("Сумма кредита должна быть от 1 до 10,000,000 руб");

            if (months <= 0 || months > 360)
                throw new ArgumentException("Срок кредита должен быть от 1 до 360 месяцев");

            if (annualRate <= 0 || annualRate >= 100)
                throw new ArgumentException("Процентная ставка должна быть от 0.1 до 99.9%");

            _logger?.LogCalculation("Кредитный расчет", amount, 0);

            double monthlyRate = (double)annualRate / 100 / 12;
            double amountDouble = (double)amount;

            double monthlyPayment = amountDouble * (monthlyRate * Math.Pow(1 + monthlyRate, months))
                                  / (Math.Pow(1 + monthlyRate, months) - 1);

            decimal monthlyPaymentDecimal = (decimal)monthlyPayment;
            monthlyPaymentDecimal = Math.Round(monthlyPaymentDecimal, 2, MidpointRounding.ToEven);

            decimal totalAmount = monthlyPaymentDecimal * months;
            decimal overpayment = totalAmount - amount;

            var result = new CreditCalculationResult
            {
                MonthlyPayment = monthlyPaymentDecimal,
                Overpayment = Math.Round(overpayment, 2, MidpointRounding.ToEven),
                TotalAmount = Math.Round(totalAmount, 2, MidpointRounding.ToEven)
            };

            _logger?.LogSuccess("Расчет завершен успешно");

            return result;
        }
    }

    public class CurrencyConverter
    {
        private readonly IExchangeRateService _exchangeService;

        public CurrencyConverter(IExchangeRateService exchangeService)
        {
            _exchangeService = exchangeService;
        }

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            decimal rate = _exchangeService.GetExchangeRate(fromCurrency, toCurrency);
            return Math.Round(amount * rate, 2, MidpointRounding.ToEven);
        }
    }
}