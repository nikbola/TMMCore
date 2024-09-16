using System;

namespace TMMCore.Types
{
    internal abstract class UIAction
    {
        internal string label;
        internal string modName;
    }

    internal class ButtonAction : UIAction
    {
        internal Action onClick;

        public ButtonAction(string label, Action onClick)
        {
            this.label = label;
            this.onClick = onClick;
        }
    }

    internal class SliderAction : UIAction
    {
        internal float min;
        internal float max;
        internal Action<float> onValueChanged;

        public SliderAction(string label, float min, float max, Action<float> onValueChanged)
        {
            this.label = label;
            this.min = min;
            this.max = max;
            this.onValueChanged = onValueChanged;
        }
    }

    internal class ToggleAction : UIAction
    {
        internal Action<bool> onValueChanged;

        public ToggleAction(string label, Action<bool> onValueChanged)
        {
            this.label = label;
            this.onValueChanged = onValueChanged;
        }
    }
}