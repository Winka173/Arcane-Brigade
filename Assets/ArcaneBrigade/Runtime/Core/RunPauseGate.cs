namespace ArcaneBrigade
{
    public sealed class RunPauseGate
    {
        public bool Manual;
        public bool Details;
        public bool Navigation;
        public bool FocusLost;
        public bool Backgrounded;
        public bool Paused => Manual || Details || Navigation || FocusLost || Backgrounded;
        public void ResetSession() { Manual = Details = Navigation = false; }
    }
}
