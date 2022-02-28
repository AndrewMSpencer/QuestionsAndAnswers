using QuestonsAndAnswers.Model;
using SimpleStack.Orm;
using SimpleStack.Orm.Sqlite;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
/// <summary>
/// Welcome to the wonderful world of console apps.
/// 
/// So we're gonna ask for your name.  If it's new, we'll ask to create the account.  If it's recognized, we're gonna need to identify if you are who you say you are.  W/ three questions.  If you haven't created them yet, you need to.  Keep a list of sugestions and add to it if new.
/// Each user's answers need to be unique and specific to them and hopefully secured and protected.  At least three questions must be answered.
/// Just maybe you'll have some fun surprises possible as a 'special user' too.
/// </summary>




public class Program
{
    private static readonly string dbName = ConfigurationManager.AppSettings["storageDB"] ?? string.Empty;
    private static readonly int expectedAnswerThreshold = Int32.Parse(ConfigurationManager.AppSettings["expectedAnswers"] ?? "0");

    public static void Main(string[] args)
    {
        var inq = new Inquisitioner(dbName, expectedAnswerThreshold);

        inq.DisplayScreenPrompt();

        Console.Clear();
        Console.ReadKey();
    }
}
    







