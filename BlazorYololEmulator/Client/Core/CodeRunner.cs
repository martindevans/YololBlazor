using System.Collections;
using Yolol.Execution;

namespace BlazorYololEmulator.Client.Core
{
    public class CodeRunner
    {
        private readonly CodeContainer _code;

        private DeviceNetwork _devices;
        private MachineState _machineState;

        /// <summary>
        /// Zero based program counter
        /// </summary>
        public int ProgramCounter { get; set; }

        public IEnumerable<KeyValuePair<string, IVariable>> Values => _machineState.Concat(_devices);
        public string? RuntimeError { get; private set; }

        public CodeRunner(CodeContainer code)
        {
            _code = code;

            _devices = new DeviceNetwork();
            _machineState = new MachineState(_devices);
        }

        public void LoadValues(IReadOnlyDictionary<string, Value> values)
        {
            _devices = new DeviceNetwork();
            _machineState = new MachineState(_devices);

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

            RuntimeError = null;
            if (ProgramCounter < code.Lines.Count)
            {
                try
                {
                    ProgramCounter = code.Lines[ProgramCounter].Evaluate(ProgramCounter, _machineState);
                }
                catch (ExecutionException ex)
                {
                    RuntimeError = ex.Message;
                    ProgramCounter++;
                }
            }
            else
            {
                ProgramCounter++;
            }

            if (ProgramCounter >= 20)
                ProgramCounter = 0;
        }

        private class DeviceNetwork
            : IDeviceNetwork, IEnumerable<KeyValuePair<string, IVariable>>
        {
            private readonly Dictionary<string, Variable> _variables = new();

            public IVariable Get(string name)
            {
                name = name.ToLowerInvariant();
                if (!_variables.TryGetValue(name, out var value))
                {
                    value = new Variable
                    {
                        Value = Number.Zero
                    };
                    _variables[name] = value;
                }
                return value;
            }

            public IEnumerator<KeyValuePair<string, IVariable>> GetEnumerator()
            {
                return _variables.Select(a => new KeyValuePair<string, IVariable>($":{a.Key}", a.Value)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
