namespace ArcaneBrigade
{
    public sealed class RunPauseGate
    {
        public bool Manual;
        public bool Details;
        public bool Navigation;
        public bool FocusLost;
        public bool Backgrounded;
        public bool Story;
        public bool Paused => Manual || Details || Navigation || FocusLost || Backgrounded || Story;
        public void ResetSession() { Manual = Details = Navigation = Story = false; }
    }
}
