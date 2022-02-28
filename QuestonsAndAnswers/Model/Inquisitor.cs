using Microsoft.Data.Sqlite;
using SimpleStack.Orm;
using SimpleStack.Orm.Sqlite;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QuestonsAndAnswers.Model
{

    public enum ScreenPromptType
    {
        InitialPrompt,
        UserVerifyQuestions,
        UserNeedsAnswers,
        UserCompletedAnswers,
        Authenticated,
        GoodByePrompt,

        UserIsChuckNorris
    }

    /// <summary>
    /// Welcome to my interpretation of the simple question and answer prompter.  
    /// Design principles:  Secure, portable data, and conceptually similar to any containerized app.  
    /// Traditionally, one would not use a console app for such a feature.  
    /// Best methodology is to split model/view/control so that functionality is easily extensible via a middleware that a UI could interact with.  
    /// As is, it's still relatively straight forward to severe the internal nature of this and utilize more remote storage when ready to advance.
    /// </summary>
    public class Inquisitioner
    {
        public Inquisitioner(string inputDB, int inputAnswerNo)
        {
            dbName = inputDB;
            expectedAnswerThreshold = inputAnswerNo;
            instantiateQuestionsData();
        }

        //pull in configuration based values.
        //I'm hoping I have enough time to encrypt the app.config, but that's down the line of priorities at this point.

        private static string dbName = string.Empty;
        private static int expectedAnswerThreshold = 0;

        private static OrmConnectionFactory dbInstance = null;

      

        //There can only ever be one user ever during execution so I can depend on this object after instantiation.
        private static User userInfo = new User();

        //I'll pre-load the records that remain unanswered
        private static SecurityQuestion[] questions = null;

        //I need to instantiate the db connectivity.
        #region pre-config methods
        public static OrmConnectionFactory Db
        {
            get
            {
                if (dbInstance == null)
                {
                    var folderName = AppDomain.CurrentDomain.BaseDirectory;
                    var fileLocation = System.IO.Path.Combine(folderName, dbName);
                   
                    dbInstance = new OrmConnectionFactory(new SqliteDialectProvider(), string.Format(@"Data Source={0};", fileLocation));
   
                    
                }


                return dbInstance;
            }
        }

        private static bool instantiateQuestionsData()
        {
            questions = GetQuestions().ToArray();

            return true;
        }

        #endregion

        #region console helper functions
        public static string AwaitInput()
        {
            var response = Console.ReadLine();

            return response;
        }


        public static void OminousPause(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }

        #region presetTextPatterns
        private static readonly string line = "-------------------------------------------------------------";
        private static readonly string indentWBar = "      | ";
        private static readonly string indentNoBar = "        ";




        #endregion



        /// <summary>
        /// reuseable chunk to hold information status header.
        /// </summary>
        private static void renderStatus()
        {
            var answerTotal = userInfo == null || userInfo.Answers == null ? 0 : userInfo.Answers.Count();
            var differnceToThreshold = expectedAnswerThreshold - answerTotal;
            var remainingQuestions = questions.Count() - answerTotal;

            var isComplete = differnceToThreshold <= 0;
            Console.WriteLine(line);
            if (isComplete)
                Console.WriteLine(String.Format("{0}, you had {1} questions answered.  That's great! There are still {2} questions left unanswered, however.", userInfo.UserName, answerTotal, remainingQuestions));
            else
                Console.WriteLine(String.Format("{0}, you had {1} questions answered and at least {2} more required.", userInfo.UserName, answerTotal, differnceToThreshold));

            Console.WriteLine(line);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);

        }


        /// <summary>
        /// Block for rendering out the status of each question.  Togglable to expect user input vs just act as information.
        /// </summary>
        /// <param name="awaitInput"></param>
        private static void renderQuestionStatus(bool awaitInput = false)
        {
            Console.WriteLine(line);
            foreach (var question in questions.OrderBy(a => a.QuestionId))
            {
                var isAnswered = userInfo == null || userInfo.Answers == null ? false : userInfo.Answers.Any(a => a.QuestionId == question.QuestionId);

                if (isAnswered)
                {
                    var curConsoleColor = Console.ForegroundColor;

                    Console.Write(string.Format("{0}{1}:  {2}", indentWBar, question.QuestionId, question.QuestionText));

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(string.Format("{0}{1}", indentNoBar, "(Completed)"));
                    Console.ForegroundColor = curConsoleColor;

                }
                else
                {
                    if (awaitInput)
                    {
                        var curConsoleColor = Console.ForegroundColor;
                        Console.Clear();
                        renderStatus();
                        Console.Write(string.Format("{0}{1}:  {2}", indentWBar, question.QuestionId, question.QuestionText));

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        var response = AwaitInput();

                        if (string.IsNullOrWhiteSpace(response))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(string.Format("{0}{1}", indentNoBar, "** SKIPPED **"));
                        }
                        else
                        {
                            AnswerQuestion(question.QuestionId, response);

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(string.Format("{0}{1}", indentNoBar, "(Completed)"));
                            userInfo = GetUserById(userInfo.UserId);
                        }

                        Console.ForegroundColor = curConsoleColor;
                    }
                    else
                    {
                        Console.Write(string.Format("{0}{1}:  {2}", indentWBar, question.QuestionId, question.QuestionText));
                    }
                }

                Console.WriteLine(Environment.NewLine);


            }

            Console.WriteLine(line);

            //TODO:  Now I decide what I wanna do with the rest of the process..
        }

        #endregion

        /// <summary>
        /// Handle rendering to the console for the various preset types.
        /// 
        /// </summary>
        public void DisplayScreenPrompt(ScreenPromptType type = ScreenPromptType.InitialPrompt)
        {
            Console.Clear();

            switch (type)
            {

                case ScreenPromptType.InitialPrompt:
                    Console.WriteLine("Welcome to the Inquisitor.  I have some... questions for you.");
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(Environment.NewLine);
                    OminousPause(2);
                    Console.WriteLine("...");
                    OminousPause(1);
                    Console.WriteLine("Anyway...");
                    OminousPause(1);
                    Console.Clear();
                    Console.WriteLine(line);
                    Console.WriteLine(indentNoBar);
                    Console.WriteLine("{0}{1}", indentNoBar, "Hi, What's your name?");

                    var userName = AwaitInput();

                    while (string.IsNullOrWhiteSpace(userName))
                    {
                        Console.WriteLine("You didn't enter anything!  Please enter your name.");
                        userName = AwaitInput();
                    }
                    var isNewUser = !IsUserRecognized(userName);



                    // Use this to create or retrieve the user object.
                    if (isNewUser)
                    {
                        Console.WriteLine(string.Format("Welcome, {0}.  Since, you're new. Let's make things nice and secure...", userName));


                        var passwordMatches = false;
                        var password = string.Empty;
                        var passwordMatch = "DO NOT MATCH";

                        while (!passwordMatches)
                        {
                            Console.WriteLine("Please enter a new password.");

                            password = AwaitInput();

                            if (string.IsNullOrWhiteSpace(password))
                            {
                                Console.WriteLine("Seriously.  Blank's not allowed.  Try again.");
                            }
                            else
                            {

                                Console.WriteLine("Please type that password again, just to make sure you didn't make a typo.");

                                passwordMatch = AwaitInput();

                                if (password != passwordMatch)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Something's not right, those didn't match.  Let's try again.");

                                }
                                else
                                {
                                    Console.WriteLine("Thanks!");
                                    passwordMatches = true;

                                    var id = CreateUser(userName, password);
                                    userInfo = GetUserById(id);
                                    DisplayScreenPrompt(ScreenPromptType.UserVerifyQuestions);
                                }
                            }
                        }



                    }
                    else
                    {

                        while (userInfo == null || userInfo.UserId == 0)
                        {

                            Console.WriteLine(String.Format("Welcome, {0}!", userName));
                            Console.WriteLine(indentNoBar);
                            Console.WriteLine("Please enter your password.");
                            var password = AwaitInput();

                            userInfo = GetUserByNameAndPass(userName, password);

                            if (userInfo == null)
                            {
                                Console.WriteLine("Hmmm, that's not the right password.  Press any key to retry.");
                                Console.ReadKey();
                            }

                        }

                        if (userInfo.Answers == null || userInfo.Answers.Count() <= expectedAnswerThreshold)
                            DisplayScreenPrompt(ScreenPromptType.UserNeedsAnswers);
                        else
                            DisplayScreenPrompt(ScreenPromptType.UserCompletedAnswers);

                    }
                    break;
                case ScreenPromptType.UserVerifyQuestions:
                    renderStatus();

                    if (userInfo.Answers == null || !userInfo.Answers.Any())
                    {
                        Console.WriteLine("You have not set any security questions.  For your identity protection you must do that now.  Press any key to continue.");
                        Console.ReadKey();
                        DisplayScreenPrompt(ScreenPromptType.UserNeedsAnswers);
                    }
                    else
                    {

                        Console.WriteLine("As an added security precaution, please confirm at least one of your asnwered security questions,.");

                        var success = false;
                        while (!success)
                        {
                            var defaultColor = Console.ForegroundColor;
                            foreach (var answer in userInfo.Answers)
                            {
                                Console.ForegroundColor = defaultColor;

                                var questionText = questions.FirstOrDefault(a => a.QuestionId == answer.QuestionId).QuestionText;

                                Console.Write(string.Format("{0}{1}", questionText, indentNoBar));
                                var result = AwaitInput();

                                if (string.IsNullOrEmpty(result)) //skip question
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.Write("(Skipped)");

                                    continue;
                                }
                                else if (result.ToHash(userInfo.UserSalt) == answer.AnswerHash)
                                {
                                    success = true;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(" Correct!");
                                    Console.ForegroundColor = defaultColor;
                                    Console.WriteLine(line);
                                    Console.Write("Press any key to continue.");
                                    DisplayScreenPrompt(ScreenPromptType.Authenticated);
                                    return;
                                }
                                else if (result.ToHash(userInfo.UserSalt) != answer.AnswerHash)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("Incorrect.");

                                }

                            }
                            //uh oh.  we got through all the questions and none were answered correctly.
                            Console.WriteLine("You didn't answer a single one of your security questions correctly.  We could not verify your identity.");
                            Console.ReadKey();
                            DisplayScreenPrompt(ScreenPromptType.GoodByePrompt);
                        }

                    }

                    break;



                case ScreenPromptType.UserNeedsAnswers:

                    renderQuestionStatus(awaitInput: true);

                    if (userInfo.Answers == null || userInfo.Answers.Length < expectedAnswerThreshold)
                        DisplayScreenPrompt(ScreenPromptType.UserNeedsAnswers);
                    else
                        DisplayScreenPrompt(ScreenPromptType.UserCompletedAnswers);
                    break;

                case ScreenPromptType.UserCompletedAnswers:
                    renderStatus();
                    renderQuestionStatus(awaitInput: false);

                    if (userInfo.Answers != null && userInfo.Answers.Length < questions.Length)
                    {
                        //TODO: this is where I give a choice to answer more if they are not all answered.
                        Console.WriteLine("Do you want to enter any more question answers? (Yes or No)");

                        var input = AwaitInput();
                        var expectedAnswers = new[] { "Y", "Yes", "N", "No" };
                        while (!expectedAnswers.Any(a => input.Equals(a, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Console.WriteLine("Appropriate Answers are Y, Yes, N or No. So Let's try again.");
                            DisplayScreenPrompt(ScreenPromptType.UserCompletedAnswers);
                        }

                        if (new[] { "Yes", "Y" }.Any(a => input.Equals(a, StringComparison.InvariantCultureIgnoreCase)))
                            DisplayScreenPrompt(ScreenPromptType.UserNeedsAnswers);
                        else
                            DisplayScreenPrompt(ScreenPromptType.UserVerifyQuestions);
                    }
                    else
                        DisplayScreenPrompt(ScreenPromptType.UserVerifyQuestions);

                    break;

                case ScreenPromptType.Authenticated:
                    renderStatus();
                    Console.WriteLine(line);
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Identity verified.");

                    OminousPause(2);
                    Console.WriteLine("...");
                    OminousPause(2);

                    if (userInfo.Answers.Length < questions.Length)
                    {
                        Console.WriteLine("Soooo, wanna answer some more questions?");
                        var key = Console.ReadKey();

                        if (key.KeyChar == 'Y' || key.KeyChar == 'y')
                            DisplayScreenPrompt(ScreenPromptType.UserNeedsAnswers);
                        else
                            DisplayScreenPrompt(ScreenPromptType.GoodByePrompt);

                    }
                    else
                    {
                        Console.WriteLine("Soooo, there's really not much left to see here.");
                        OminousPause(1);
                        Console.WriteLine("Really...");
                        OminousPause(2);
                        Console.WriteLine("Fine, if you really want more, just type Y");
                        var key = Console.ReadKey();

                        if (key.KeyChar == 'Y' || key.KeyChar == 'y')
                        {
                            executeOrder66();
                            DisplayScreenPrompt(ScreenPromptType.GoodByePrompt);
                        }
                        else
                            DisplayScreenPrompt(ScreenPromptType.GoodByePrompt);
                    }


                    break;
                case ScreenPromptType.GoodByePrompt:
                    Console.WriteLine("Thanks for Participating.  Goodbye.");

                    return;
                    break;




            }



        }


        #region funzies
        /// <summary>
        /// For those looking through this code, I hope many of these cultural references amuse and delight rather than befuddle and confuse.  This has been a fun diversion.
        /// </summary>
        private static void executeOrder66()
        {
            //Never gonna let you down.  Never gonna give you up.......
            openBrowser("https://www.youtube.com/watch?v=xvFZjo5PgG0");
            return;
        }


        /// <summary>
        /// copied as is from: https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
        /// credit where credit is due.
        /// </summary>
        /// <param name="url"></param>
        private static void openBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = false });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion


        #region database methods
        public static bool IsUserRecognized(string userName)
        {
            using (var conn = Db.OpenConnection())
            {
                var identityResult = conn.Select<UserIdentity>(a => a.UserName == userName).FirstOrDefault();

                return identityResult != null;
            }

        }

        public static bool AnswerQuestion(int questionId, string answerText)
        {
            using (var conn = Db.OpenConnection())
            {
                var answeredResult = conn.Insert<UserQuestionAnswer>(new UserQuestionAnswer() { QuestionId = questionId, UserId = userInfo.UserId, AnswerHash = answerText.ToHash(userInfo.UserSalt) });
            }

            return true;
        }

        private static User hydrateUserObject(UserIdentity identityResult)
        {
            if (identityResult == null)
                return null;

            using (var conn = Db.OpenConnection())
            {
                if (identityResult != null)
                {
                    var answerResult = conn.Select<UserQuestionAnswer>(a => a.UserId == identityResult.UserId);

                    var user = new User()
                    {
                        UserName = identityResult.UserName,
                        UserId = identityResult.UserId,
                        UserSalt = identityResult.UserSalt,
                        Answers = !answerResult.Any() ? Enumerable.Empty<QuestionAnswer>().ToArray() : answerResult.Select(a => new QuestionAnswer() { AnswerHash = a.AnswerHash, QuestionId = a.QuestionId }).ToArray(),
                    };

                    return user;
                }
            }
            return null;
        }

        public static User GetUserByNameAndPass(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return null; // usually this would be a throw, but meh, this is an isolated console app, and this shouldn't really be possible to get to.

            UserIdentity identityResult = null;
            using (var conn = Db.OpenConnection())
            {
                //this ORM doesn't seem to like inline use of data so splitting this into two steps...
                //identityResult = conn.Select<UserIdentity>(a => a.UserName == userName && a.UserPasswordHash == password.ToHash(a.UserSalt)).FirstOrDefault();

                identityResult = conn.Select<UserIdentity>(a => a.UserName == userName).FirstOrDefault();

                //verify password separately due to ORM oddities... this is a good example of why I generally prefer Dapper w/ parameterized hand-written sql instead of an ORM code-writer.
                if (identityResult == null || identityResult.UserPasswordHash != password.ToHash(identityResult.UserSalt))
                    return null;
            }
            return hydrateUserObject(identityResult);
        }

        public static User GetUserById(int id)
        {
            UserIdentity identityResult = null;
            using (var conn = Db.OpenConnection())
            {
                identityResult = conn.Select<UserIdentity>(a => a.UserId == id).FirstOrDefault();
            }
            return hydrateUserObject(identityResult);
        }

        public static int CreateUser(string userName, string password)
        {
            var salt = Guid.NewGuid().ToString();
            using (var conn = Db.OpenConnection())
            {
                var result = conn.Insert<UserIdentity>(new UserIdentity() { UserName = userName, UserSalt = salt, UserPasswordHash = password.ToHash(salt) });
                return result;
            }

        }



        public static IEnumerable<SecurityQuestion> GetQuestions()
        {
            using (var conn = Db.OpenConnection())
            {
                var results = conn.Select<SecurityQuestion>();

                return results;
            }
        }

        #endregion


    }


}
