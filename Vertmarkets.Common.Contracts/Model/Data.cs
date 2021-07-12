using System;
using System.Collections.Generic;
using System.Text;

namespace Vertmarkets.Common.Contracts.Model
{
    public class Data
    {
        public string TotalTime { get; set; }
        public bool AnswerCorrect { get; set; }
        public List<string> ShouldBe { get; set; }
    }
}
