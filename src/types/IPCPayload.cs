namespace TMMCore.Types
{
    [System.Serializable]
    internal struct IPCPayload
    {
        public string id;
        public string label;
        public string modName;
        public ActionType actionType;
        public float? min;
        public float? max;
    }

    internal enum ActionType
    {
        Slider,
        Button,
        Toggle
    }
}