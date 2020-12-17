namespace StateMachineLib.Options
{
    public class TransitionOptions
    {
        public string StartState { get; set; }
        public string Token { get; set; }
        public string EndState { get; set; }
        public bool IsEpsilon { get; set; }
    }
}