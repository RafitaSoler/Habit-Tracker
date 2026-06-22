using Habit_Tracker;

class Program
{
    static void Main(string[] args)
    {
        File.Delete("log.txt");
        File.Delete("habit-tracker.db");

        DatabaseManager.Start();
        bool endApp = false;

        Console.WriteLine("Habit Tracker");

        DatabaseManager.CreateHabit("drink water");
        DatabaseManager.LogHabit("drink water", "2026-06-18", 2);
        DatabaseManager.LogHabit("drink water", "2026-06-19", 1);
        DatabaseManager.LogHabit("drink water", "2026-06-21", 4);

        DatabaseManager.CreateHabit("brush teeth");
        DatabaseManager.LogHabit("brush teeth", "2026-06-19", 3);
        DatabaseManager.LogHabit("brush teeth", "2026-06-20", 1);

        DatabaseManager.CreateHabit("make bed");
        DatabaseManager.LogHabit("make bed", "2026-06-18", 1);
        DatabaseManager.LogHabit("make bed", "2026-06-19", 9);


        while (!endApp)
        {
            Console.WriteLine("Main Menu:");
            Console.WriteLine("\t0 - Close Application");
            Console.WriteLine("\t1 - View all habits");
            Console.WriteLine("\t2 - Record new habit");
            Console.WriteLine("\t3 - Delete habits");

            string? option = Console.ReadLine();

            switch(option)
            {
                case "0":
                    endApp = true;
                    break;
                case "1":
                    ViewAllHabits();
                    break;
                case "2":
                    break;
                case "3":
                    break;
            }
        }
    }

    static void ViewAllHabits()
    {
        List<(string Name, string Date, int Quantity)> habitsList = DatabaseManager.GetAllHabits();
        List<string> habitNames = new();
        List<int> habitTotals = new();
        foreach (var habit in habitsList)
        {
            if(!habitNames.Contains(habit.Name))
            {
                habitNames.Add(habit.Name);
            }
        }
        for (int i = 0; i < habitNames.Count; i++)
        {
            habitTotals.Add(0);
            foreach (var habit in habitsList)
            {
                if (habit.Name == habitNames[i])
                {
                    habitTotals[i] += habit.Quantity;
                }
            }
        }

        Console.WriteLine("--------------------------------\n");
        for (int i = 0; i < habitNames.Count; i++)
        {
            Console.WriteLine($"{habitNames[i]}\tTotal: {habitTotals[i]}");
        }
        Console.WriteLine("");
        Console.WriteLine("Name\t\tDate\t\tQuantity");
        Console.WriteLine("----------------------------------------");
        foreach (var habit in habitsList)
        {
            Console.WriteLine($"{habit.Name}\t{habit.Date}\t{habit.Quantity}");
        }
        Console.WriteLine("");
    }
}