using SimpleStack.Orm.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestonsAndAnswers.Model
{
    [Alias("Questions")]
    public class SecurityQuestion
    {
        [PrimaryKey]
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }

    }
}
