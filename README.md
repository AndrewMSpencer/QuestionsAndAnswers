# README #

QuestionsAndAnswers AKA: The Inquisitor, is an example program to demonstrate best practices in navigating security prompting and basic user feedback loops regarding the prompting and storage of secrity info.
The built console app will run on windows and includes a simple sqlite data structure to hold the users, answers, and questions related to this example.  



### Getting Started ###

* Use Visual Studio w/ .Net 6.0 support to compile.
* Utilizes Nuget packages for Dapper.net, Sqlite, and SimpleStack.ORM
* Pre-configured SqlLite DB with three tables (Users, Questions, and UserAnsers) included.


### Features ###

This simple app will mimic a basic authentication process by prompting users for their ** Name ** and ** Password **.  

Once these are set, the user will be continuously prompted to answer at least 3 security questions out of a preset total of 12.  
Once enough questions are set, the system then prompts for at least one successful matching answer to finalize authentication.
Since this exercise is mainly focused on authentication best practices, upon authenticating, there's limited functionality.  
There is a surprise if one sets an answer for all questions and follows the onscreen prompt in authenticated area.

**Note** All user provided sensitive information is stored as SHA256 Hash with a preset salt and individual user-level salt to protect passwords and answers.  If you modify the existing app.config setting for the global salt key, all previously entered information will be unuseable.

### Test Data ###

* Existing Test User:   _Chuck Norris_
* Existing Test Answer for all user's prompts:  _punch_



### Caviats ###

There are a variety of limitations in this demonstration worth noting.  

* Obviously you would never use a console app with local storage for a security prompt system in a real world environment.
* This doesn't have any logic about preventing brute-force attacks via counting the number of attempts against a password or security question.  You should always consider the possibility that whoever is using an app is not authorized.  Consider tracking mechanisms to disable attempts after too many failures and/or consider multi-factor (SMS/email verify codes/RSA token, etc) as a much more secure alternative to secret questions.
* The database is not currently encrypted in source control.  Real world scenarios should always protect the data from prying eyes.  Most server-based dbs protect at least with a username/password but consider certificate based connections for even further protection of sensitive data access.
* Since the questions are entirely database stored, it would be quite easy to either change the texts as needed and/or implement user-defined questions.  _QuestionText_ is unique and it would be relatively trivial to produce this enhancement.  Since the entire database is locally stored, this feature is less useful than it would be in a real-world service-based implementation of authentication/authorization.


### Author ###

Andrew Spencer

