using BlazorYololEmulator.Shared;
using Microsoft.AspNetCore.Components;
using Yolol.Execution;
using Yolol.Grammar;

namespace BlazorYololEmulator.Client.Core
{
    public class StateManager
    {
        private readonly NavigationManager _navManager;

        private readonly CodeContainer _code;
        private readonly CodeRunner _runner;
        public Parser.Result<Yolol.Grammar.AST.Program, Parser.ParseError> ParseResult => _code.ParseResult;
        public string YololCode
        {
            get => _code.YololCode;
            set
            {
                _code.YololCode = value;
                UpdateUrl();
            }
        }
        public string? RuntimeError => _runner.RuntimeError;

        public bool IsRunning { get; set; }

        public int ProgramCounter => _runner.ProgramCounter;
        public IEnumerable<(string, Value)> Values => _runner.Values.Select(a => (a.Key, a.Value.Value));

        public event Action? OnStateChange;

        public StateManager(NavigationManager navManager, CodeContainer code, CodeRunner runner)
        {
            _navManager = navManager;
            _code = code;
            _runner = runner;
        }

        public void Load(SerializedState state)
        {
            YololCode = state.Code;
            _runner.ProgramCounter = state.ProgramCounter;
            _runner.LoadValues(state.Values);
        }

        private SerializedState Save()
        {
            return new SerializedState(_code.YololCode, Values.ToDictionary(a => a.Item1, a => a.Item2), ProgramCounter);
        }

        /// <summary>
        /// Store serialised state in URL
        /// </summary>
        private void UpdateUrl()
        {
            var serialised = Save().Serialize();
            var uri = _navManager.GetUriWithQueryParameter("state",
                string.IsNullOrEmpty(serialised) ? null : serialised);
            _navManager.NavigateTo(uri);
        }

        public void Pause()
        {
            if (!IsRunning)
                return;
            IsRunning = false;

            // todo: stop execution
        }

        public void Run()
        {
            if (IsRunning)
                return;
            IsRunning = true;

            // todo: start execution
        }

        public void Step()
        {
            if (!ParseResult.IsOk)
                return;
            Pause();

            _runner.Step();
            UpdateUrl();
            OnStateChange?.Invoke();
        }

        public void Reset()
        {
            Pause();

            _runner.Reset();
            OnStateChange?.Invoke();
            UpdateUrl();
        }
    }
}
