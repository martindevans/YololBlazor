using Yolol.Grammar;

namespace BlazorYololEmulator.Client
{
    public class StateManager
    {
        private SerializedState _state = new SerializedState();
        public SerializedState State
        {
            get => _state;
            set
            {
                _state = value;

                var result = Parser.ParseProgram(_state.Code);
                Error = result.IsOk ? null : result.Err;
            }
        }

        public Parser.ParseError? Error { get; private set; }

        public bool IsRunning { get; set; }

        public void Pause()
        {
            IsRunning = false;
        }

        public void Run()
        {
            IsRunning = true;
        }

        public void Step()
        {

        }
    }
}
