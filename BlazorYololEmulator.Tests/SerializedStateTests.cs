using System.Collections.Generic;
using BlazorYololEmulator.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yolol.Execution;

namespace BlazorYololEmulator.Tests
{
    [TestClass]
    public class SerializedStateTests
    {
        [TestMethod]
        public void RoundTrip()
        {
            var state1 = new SerializedState("code=1", new Dictionary<string, Value> { { "a", Number.One } }, 10);
            var serialized = state1.Serialize();
            var state2 = SerializedState.Deserialize(serialized);

            Assert.AreEqual(state1.ProgramCounter, state2.ProgramCounter);
            Assert.AreEqual(state1.Code, state2.Code);
        }
    }
}