using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bakery
{
    public class Program
    {
        private static LinkedList<int> Days = new LinkedList<int>();
        private static SortedDictionary<int, (SortedDictionary<int, Salary> Salaries, List<Payment> Payments)> listOfTransfers = new SortedDictionary<int, (SortedDictionary<int, Salary> Salaries, List<Payment> Payments)>();

        private static void Main(string[] args)
        {
            string filePath = string.Empty;
            while (!filePath.Equals("end"))
            {
                Days = new LinkedList<int>();
                listOfTransfers = new SortedDictionary<int, (SortedDictionary<int, Salary> Salaries, List<Payment> Payments)>();

                Console.WriteLine("Provide path to input file (or end to quit):");
                filePath = Console.ReadLine();
                var input = File.ReadAllText(filePath);

                var splittedInput = input.Split(' ');

                // iterate over inputs and create elements
                for (int i = 0; i < splittedInput.Length; i++)
                {
                    if (splittedInput[i].Equals("F"))
                    {
                        var salary = new Salary
                        {
                            Day = Int32.Parse(splittedInput[i + 1]),
                            Amount = Int32.Parse(splittedInput[i + 4]),
                            TimeWindow = Int32.Parse(splittedInput[i + 3]),
                            Driver = Int32.Parse(splittedInput[i + 2])
                        };

                        CheckDictionaryForSalary(salary);
                        i = i + 4;
                        continue;
                    }

                    if (splittedInput[i].Equals("B"))
                    {
                        var payment = new Payment
                        {
                            Day = Int32.Parse(splittedInput[i + 1]),
                            Amount = Int32.Parse(splittedInput[i + 2]),
                            AmountUsed = false
                        };

                        CheckDictionaryForPayment(payment);
                        i = i + 2;
                        continue;
                    }
                }

                // create linkedlist
                Days = new LinkedList<int>(listOfTransfers.Keys);

                var x = new List<Payment>();
                foreach (var item in listOfTransfers)
                {
                    x.AddRange(item.Value.Payments);
                }
                DayLooper(Days, x);

                Console.WriteLine();
                Console.WriteLine("Finished");
            }
        }

        private static void DayLooper(LinkedList<int> days, List<Payment> listOfPayments)
        {
            var day = days.First;
            while (day != null)
            {
                var dayToCheck = listOfTransfers[day.Value];
                if (dayToCheck.Salaries == null)
                {
                    day = day.Next;
                    continue;
                }

                foreach (var dailyDriverSalary in dayToCheck.Salaries)
                {
                    if (dailyDriverSalary.Value != null)
                    {
                        var usablePayments = listOfPayments
                            .Where(p => p.Day >= day.Value && p.Day <= day.Value + dailyDriverSalary.Value.TimeWindow)
                            .Where(p => !p.AmountUsed)
                            .ToList();
                        var result = false;
                        for (int i = 0; i < usablePayments.Count && result == false; i++)
                        {
                            result = SumUp(usablePayments, i, dailyDriverSalary.Value.Amount, 1);
                        }
                        if (!result)
                        {
                            Console.Write($"{day.Value}:{dailyDriverSalary.Key} ");
                        }
                    }
                }

                day = day.Next;
            }
        }

        private static bool SumUp(List<Payment> payments, int outerIndex, int valueToFind, int iterationDepth)
        {
            if (outerIndex == payments.Count) // at end of list already
                return false;

            if (iterationDepth == 4) // max split payments reached
            {
                if (valueToFind == payments.ElementAt(outerIndex).Amount)
                {
                    payments.ElementAt(outerIndex).AmountUsed = true;
                    return true;
                }

                return false;
            }

            if (valueToFind > payments.ElementAt(outerIndex).Amount) // calculate further
            {
                var result = false;
                for (var innerIndex = outerIndex + 1; innerIndex < payments.Count() && result == false; innerIndex++)
                {
                    result = SumUp(payments, innerIndex, valueToFind - payments.ElementAt(outerIndex).Amount, iterationDepth + 1);
                    payments.ElementAt(outerIndex).AmountUsed = result;
                }

                return result;
            }

            if (valueToFind == payments.ElementAt(outerIndex).Amount)
            {
                payments.ElementAt(outerIndex).AmountUsed = true;
                return true;
            }

            if (valueToFind == 0)
            {
                return true;
            }

            return false;
        }

        private static void CheckDictionaryForSalary(Salary salary)
        {
            // check if day already exists
            if (listOfTransfers.TryGetValue(salary.Day, out (SortedDictionary<int, Salary> Salaries, List<Payment> Payments) dailyList))
            {
                dailyList.Salaries.Add(salary.Driver, salary);
            }
            else
            {
                var temp = new SortedDictionary<int, Salary>
                {
                    { salary.Driver, salary }
                };
                listOfTransfers.Add(salary.Day, (temp, new List<Payment>()));
            }
        }

        private static void CheckDictionaryForPayment(Payment payment)
        {
            // check if day already exists
            if (listOfTransfers.TryGetValue(payment.Day, out (SortedDictionary<int, Salary> Salaries, List<Payment> Payments) dailyList))
            {
                dailyList.Payments.Add(payment);
            }
            else
            {
                listOfTransfers.Add(payment.Day, (null, new List<Payment> { payment }));
            }
        }
    }

    public class Salary
    {
        public int Day { get; set; }
        public int Amount { get; set; }
        public int TimeWindow { get; set; }
        public int Driver { get; set; }
    }

    public class Payment
    {
        public int Day { get; set; }
        public int Amount { get; set; }
        public bool AmountUsed { get; set; }
    }
}