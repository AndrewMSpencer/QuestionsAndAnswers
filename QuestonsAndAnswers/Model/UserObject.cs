using SimpleStack.Orm.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestonsAndAnswers.Model
{
    
    public class UserIdentityBase
    {
       [PrimaryKey]
       [AutoIncrement]
       public int UserId { get; set; }

       public string UserName { get; set; } 

       public string UserSalt { get; set; }



    }

    [Alias("Users") ]
    public class UserIdentity : UserIdentityBase
    {
        public string UserPasswordHash { get; set; }


    }




    public class User : UserIdentityBase
    {
        public QuestionAnswer[] Answers { get; set; }


    }
}
