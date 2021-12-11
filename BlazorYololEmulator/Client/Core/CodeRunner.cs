using Yolol.Execution;
using Yolol.Grammar.AST.Statements;

namespace BlazorYololEmulator.Client.Core
{
    public class CodeRunner
    {
        private readonly CodeContainer _code;
        private MachineState _machineState;

        public int ProgramCounter { get; set; }
        public IEnumerable<KeyValuePair<string, IVariable>> Values => _machineState;
        public string? RuntimeError { get; private set; }

        public CodeRunner(CodeContainer code)
        {
            _code = code;
            _machineState = new MachineState(new DeviceNetwork());
        }

        public void LoadValues(IReadOnlyDictionary<string, Value> values)
        {
            _machineState = new MachineState(new DeviceNetwork());

            foreach (var (k, v) in values)
                _machineState.GetVariable(k).Value = v;
        }

        public void Reset()
        {
            ProgramCounter = 0;
        }

        public void Step()
        {
            if (!_code.ParseResult.IsOk)
                return;

            var code = _code.ParseResult.Ok;

            if (ProgramCounter < code.Lines.Count)
            {
                try
                {
                    RuntimeError = null;
                    ProgramCounter = code.Lines[ProgramCounter].Evaluate(ProgramCounter, _machineState);
                    return;
                }
                catch (ExecutionException ex)
                {
                    RuntimeError = ex.Message;
                }
            }

            ProgramCounter++;
            if (ProgramCounter >= 20)
                ProgramCounter = 0;
        }

        private class DeviceNetwork
            : IDeviceNetwork
        {
            public IVariable Get(string name)
            {
                return new Variable();
            }
        }
    }
}
