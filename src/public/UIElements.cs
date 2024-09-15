using System;
using System.Diagnostics;
using System.Reflection;

namespace TMMCore
{
    public static class UIElements
    {
        public static void Button(string label, Action onClick)
        {
            var trace = new StackTrace();
            StackFrame frame = trace.GetFrame(1);
            MethodBase method = frame.GetMethod();
            Assembly callingAssembly = method.DeclaringType.Assembly;
            Plugin.RegisterUIAction(callingAssembly.GetName().Name, new ButtonAction(label, onClick));
        }

        public static void Slider(string label, float min, float max, Action<float> onValueChanged)
        {
            var trace = new StackTrace();
            StackFrame frame = trace.GetFrame(1);
            MethodBase method = frame.GetMethod();
            Assembly callingAssembly = method.DeclaringType.Assembly;
            Plugin.RegisterUIAction(callingAssembly.GetName().Name, new SliderAction(label, min, max, onValueChanged));
        }

        public static void Toggle(string label, Action<bool> onValueChanged)
        {
            var trace = new StackTrace();
            StackFrame frame = trace.GetFrame(1);
            MethodBase method = frame.GetMethod();
            Assembly callingAssembly = method.DeclaringType.Assembly;
            Plugin.RegisterUIAction(callingAssembly.GetName().Name, new ToggleAction(label, onValueChanged));
        }
    }
}