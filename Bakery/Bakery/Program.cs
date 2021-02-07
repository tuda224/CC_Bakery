using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bakery
{
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

    public class Program
    {
        private static LinkedList<int> Days = new LinkedList<int>();
        private static SortedDictionary<int, (SortedDictionary<int, Salary> Salaries, List<Payment> Payments)> listOfTransfers = new SortedDictionary<int, (SortedDictionary<int, Salary> Salaries, List<Payment> Payments)>();

        private static void Main(string[] args)
        {
            while (true)
            {
                // reset lists
                Days = new LinkedList<int>();
                listOfTransfers = new SortedDictionary<int, (SortedDictionary<int, Salary> Salaries, List<Payment> Payments)>();

                // get input
                var input = GetInputFromFile();
                var splittedInput = input.Split(' ');

                // iterate over inputs and create elements
                CreateElementsFromInput(splittedInput);

                // create linkedlist to navigate through days more easily
                Days = new LinkedList<int>(listOfTransfers.Keys);

                // combine all payments into one list
                var payments = new List<Payment>();
                foreach (var item in listOfTransfers)
                {
                    payments.AddRange(item.Value.Payments);
                }

                // handle data
                DayLooper(Days, payments);

                Console.WriteLine();
                Console.WriteLine("Finished");
            }
        }

        private static string GetInputFromFile()
        {
            Console.WriteLine("Provide path to input file:");
            var filePath = Console.ReadLine();
            var input = File.ReadAllText(filePath);
            return input;
        }

        private static void CreateElementsFromInput(string[] splittedInput)
        {
            for (int i = 0; i < splittedInput.Length; i++)
            {
                // Salary
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
                    i += 4; // jump over number of properties to next element
                    continue;
                }

                // Payment
                if (splittedInput[i].Equals("B"))
                {
                    var payment = new Payment
                    {
                        Day = Int32.Parse(splittedInput[i + 1]),
                        Amount = Int32.Parse(splittedInput[i + 2]),
                        AmountUsed = false
                    };

                    CheckDictionaryForPayment(payment);
                    i += 2; // jump over number of properties to next element
                    continue;
                }
            }
        }

        private static void DayLooper(LinkedList<int> days, List<Payment> listOfPayments)
        {
            // get first day
            var day = days.First;
            // process calculation as long as there are days
            while (day != null)
            {
                // save current day to variable for easier access afterwards
                var dayToCheck = listOfTransfers[day.Value];
                // if there aren't any salaries we can't calculate anything
                if (dayToCheck.Salaries == null)
                {
                    day = day.Next;
                    continue;
                }

                // iterate through each driver salary of the day
                foreach (var dailyDriverSalary in dayToCheck.Salaries)
                {
                    // only calculate if the driver really has a salary
                    if (dailyDriverSalary.Value != null)
                    {
                        // generate a list of payments that are still not used
                        var usablePayments = listOfPayments
                            .Where(p => p.Day >= day.Value && p.Day <= day.Value + dailyDriverSalary.Value.TimeWindow) // payments in time window
                            .Where(p => !p.AmountUsed) // unused payments
                            .ToList();

                        // iterate over each payment if we haven't found the correct calculation
                        var result = false;
                        for (int i = 0; i < usablePayments.Count && result == false; i++)
                        {
                            result = SumUp(usablePayments, i, dailyDriverSalary.Value.Amount, 1);
                        }

                        // output driver that didn't pay
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
                // check if current payment is the last missing one
                if (valueToFind == payments.ElementAt(outerIndex).Amount)
                {
                    payments.ElementAt(outerIndex).AmountUsed = true;
                    return true;
                }

                return false;
            }

            if (valueToFind > payments.ElementAt(outerIndex).Amount) // calculate further
            {
                // iterate through subsequent payments
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
}