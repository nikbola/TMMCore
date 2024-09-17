namespace TMMCore.Types 
{
    [System.Serializable]
    internal struct IPCResponsePayload 
    {
        public string id;
        public float sliderValue;
        public bool toggleValue;
        public ActionType type;
    }
}