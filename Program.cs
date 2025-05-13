using System;
using System.Globalization;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        try
        {
            // Ввод первого комплексного числа
            Console.Write("z1 модуль (r >= 0): ");
            if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double modulus1))
                throw new ArgumentException("Модуль z1 должен быть числом.");
            if (modulus1 < 0)
                throw new ArgumentException("Модуль z1 должен быть неотрицательным числом.");

            Console.Write("z1 аргумент (θ, радианы): ");
            if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double argument1))
                throw new ArgumentException("Аргумент z1 должен быть числом.");

            var z1 = new TrigonometricComplex(modulus1, argument1);

            // Ввод второго комплексного числа
            Console.Write("\nz2 модуль (r >= 0): ");
            if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double modulus2))
                throw new ArgumentException("Модуль z2 должен быть числом.");
            if (modulus2 < 0)
                throw new ArgumentException("Модуль z2 должен быть неотрицательным числом.");

            Console.Write("z2 аргумент (θ, радианы): ");
            if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double argument2))
                throw new ArgumentException("Аргумент z2 должен быть числом.");

            var z2 = new TrigonometricComplex(modulus2, argument2);

            // Вывод результатов операций
            Console.WriteLine($"\nz1 = {z1}");
            Console.WriteLine($"z2 = {z2}");
            Console.WriteLine($"Сумма: {z1 + z2}");
            Console.WriteLine($"Разность: {z1 - z2}");
            Console.WriteLine($"Произведение: {z1 * z2}");
            Console.WriteLine($"Частное: {z1 / z2}");
            Console.WriteLine($"z1 == z2: {z1 == z2}");
            Console.WriteLine($"z1 > z2 (по модулю): {z1.CompareTo(z2) > 0}");

            // Парсинг строки для z3
            var z3 = TrigonometricComplex.Parse("2.50(cos(1.57) + i*sin(1.57))");
            Console.WriteLine($"z3 из строки: {z3}");

            // Алгебраическая форма z1
            var (real, imag) = z1.ToAlgebraicForm();
            Console.WriteLine($"z1 в алгебраической форме: {real:F2} + {imag:F2}i");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        Console.ReadLine();
    }
}

/// <summary>
/// Класс для представления комплексного числа в тригонометрической форме r(cos(θ) + i*sin(θ)).
/// </summary>
public class TrigonometricComplex : IComparable<TrigonometricComplex>, IEquatable<TrigonometricComplex>
{
    public double Modulus { get; }
    public double Argument { get; }

    // Конструктор: создаёт комплексное число с заданным модулем и аргументом
    public TrigonometricComplex(double modulus, double argument)
    {
        if (modulus < 0)
            throw new ArgumentException("Модуль не может быть отрицательным.");
        Modulus = modulus;
        Argument = NormalizeAngle(argument);
    }

    // Нормализует угол в диапазон [-π, π]
    private static double NormalizeAngle(double angle)
    {
        while (angle > Math.PI) angle -= 2 * Math.PI;
        while (angle <= -Math.PI) angle += 2 * Math.PI;
        return angle;
    }

    // Сложение: перевод в алгебраическую форму, сложение, возврат в тригонометрическую
    public static TrigonometricComplex operator +(TrigonometricComplex a, TrigonometricComplex b)
    {
        var realA = a.Modulus * Math.Cos(a.Argument);
        var imagA = a.Modulus * Math.Sin(a.Argument);
        var realB = b.Modulus * Math.Cos(b.Argument);
        var imagB = b.Modulus * Math.Sin(b.Argument);

        var realSum = realA + realB;
        var imagSum = imagA + imagB;

        double modulus = Math.Sqrt(realSum * realSum + imagSum * imagSum);
        double argument = Math.Atan2(imagSum, realSum);

        return new TrigonometricComplex(modulus, argument);
    }

    // Вычитание: аналогично сложению
    public static TrigonometricComplex operator -(TrigonometricComplex a, TrigonometricComplex b)
    {
        var realA = a.Modulus * Math.Cos(a.Argument);
        var imagA = a.Modulus * Math.Sin(a.Argument);
        var realB = b.Modulus * Math.Cos(b.Argument);
        var imagB = b.Modulus * Math.Sin(b.Argument);

        var realDiff = realA - realB;
        var imagDiff = imagA - imagB;

        double modulus = Math.Sqrt(realDiff * realDiff + imagDiff * imagDiff);
        double argument = Math.Atan2(imagDiff, realDiff);

        return new TrigonometricComplex(modulus, argument);
    }

    // Умножение: модули перемножаются, аргументы складываются
    public static TrigonometricComplex operator *(TrigonometricComplex a, TrigonometricComplex b)
    {
        return new TrigonometricComplex(a.Modulus * b.Modulus, a.Argument + b.Argument);
    }

    // Деление: модули делятся, аргументы вычитаются
    public static TrigonometricComplex operator /(TrigonometricComplex a, TrigonometricComplex b)
    {
        if (b.Modulus == 0)
            throw new DivideByZeroException("Деление на ноль.");
        return new TrigonometricComplex(a.Modulus / b.Modulus, a.Argument - b.Argument);
    }

    // Сравнение по модулю
    public int CompareTo(TrigonometricComplex other)
    {
        return other == null ? 1 : Modulus.CompareTo(other.Modulus);
    }

    // Проверка равенства с учетом погрешности
    public static bool operator ==(TrigonometricComplex a, TrigonometricComplex b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return Math.Abs(a.Modulus - b.Modulus) < 1e-10 && Math.Abs(a.Argument - b.Argument) < 1e-10;
    }

    public static bool operator !=(TrigonometricComplex a, TrigonometricComplex b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj) => Equals(obj as TrigonometricComplex);
    public bool Equals(TrigonometricComplex other) => this == other;

    // Хэш-код для использования в коллекциях
    public override int GetHashCode()
    {
        return Modulus.GetHashCode() ^ Argument.GetHashCode();
    }

    // Строковое представление числа
    public override string ToString()
    {
        return $"{Modulus:F2}(cos({Argument:F2}) + i*sin({Argument:F2}))";
    }

    // Парсинг строки вида r(cos(θ) + i*sin(θ))
    public static TrigonometricComplex Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s));

        string normalizedString = s.Replace(",", ".");

        var pattern = @"^(\d+\.?\d*)\(cos\((-?\d+\.?\d*)\)\s*\+\s*i\*sin\((-?\d+\.?\d*)\)\)$";
        var match = Regex.Match(normalizedString, pattern);

        if (!match.Success)
            throw new FormatException("Неправильный формат строки. Ожидается: r(cos(θ) + i*sin(θ))");

        string modulusStr = match.Groups[1].Value;
        string argumentStr = match.Groups[2].Value;
        string argument2Str = match.Groups[3].Value;

        if (argumentStr != argument2Str)
            throw new FormatException("Аргументы cos и sin не совпадают.");

        if (!double.TryParse(modulusStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double modulus))
            throw new FormatException("Невозможно преобразовать модуль.");

        if (!double.TryParse(argumentStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double argument))
            throw new FormatException("Невозможно преобразовать аргумент.");

        return new TrigonometricComplex(modulus, argument);
    }

    // Преобразование в алгебраическую форму
    public (double Real, double Imaginary) ToAlgebraicForm()
    {
        return (Modulus * Math.Cos(Argument), Modulus * Math.Sin(Argument));
    }
}