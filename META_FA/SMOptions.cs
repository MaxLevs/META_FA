using System.Collections.Generic;
using System.Linq;

namespace META_FA
{
    public class SMOptions
    {
        public string InitialState { get; set; }
        public List<string> FinalStates { get; set; }
        public List<TransitionOptions> Transitions { get; set; }

        public List<string> GetStates()
        {
            var firstPart = Transitions.Select(options => options.StartState).ToList();
            var secondPart = Transitions.Select(options => options.EndState).ToList();
            firstPart.AddRange(secondPart);
            return firstPart.Distinct().ToList();
        }
    }
}