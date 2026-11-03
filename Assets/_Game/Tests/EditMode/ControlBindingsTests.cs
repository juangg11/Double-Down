using DoubleDown.Presentation;
using NUnit.Framework;
using UnityEngine.InputSystem;
namespace DoubleDown.Tests
{
    public class ControlBindingsTests
    {
        [Test] public void DuplicateKeySwapsActionsWithoutLosingEitherBinding()
        {
            var bindings = new ControlBindings();
            Assert.IsTrue(bindings.Rebind(TableAction.Hit, Key.S));
            Assert.AreEqual(Key.S, bindings[TableAction.Hit]);
            Assert.AreEqual(Key.H, bindings[TableAction.Stand]);
        }
        [Test] public void CustomKeysSurviveSerialization()
        {
            var bindings = new ControlBindings(); bindings.Rebind(TableAction.Hit, Key.K);
            var restored = ControlBindings.FromJson(bindings.ToJson());
            foreach (var action in ControlBindings.Actions) Assert.AreEqual(bindings[action], restored[action]);
        }
        [TestCase("{broken")]
        [TestCase("{\"version\":99,\"keys\":[]}")]
        [TestCase("{\"version\":1,\"keys\":[1,1,1,1,1,1,1,1,1,1,1]}")]
        public void InvalidSavesRestoreDefaults(string json)
        {
            var restored = ControlBindings.FromJson(json); var defaults = new ControlBindings();
            foreach (var action in ControlBindings.Actions) Assert.AreEqual(defaults[action], restored[action]);
        }
        [TestCase(Key.Escape)] [TestCase(Key.None)] [TestCase((Key)999)]
        public void ReservedOrUnknownKeysCannotReplaceAnAction(Key key)
        {
            var bindings = new ControlBindings(); Assert.IsFalse(bindings.Rebind(TableAction.Hit, key));
            Assert.AreEqual(Key.H, bindings[TableAction.Hit]);
        }
    }
}
