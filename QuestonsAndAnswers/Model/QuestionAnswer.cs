using SimpleStack.Orm.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestonsAndAnswers.Model
{

    public class QuestionAnswer
    {
        public int QuestionId { get; set; }
        public string AnswerHash { get; set; }

    }

    [Alias("UserAnswers")]
    public class UserQuestionAnswer : QuestionAnswer
    {
        public int UserId { get; set; }

    }
}
