namespace First_Console_shafi;

// A monthly spending limit for one category. At most one per Category;
// Program.cs enforces that by replacing in place instead of appending duplicates.
public record Budget(Category Category, decimal MonthlyLimit);
