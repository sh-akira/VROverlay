using System;
namespace sh_akira.OVRTracking
{
    public class OVRConnectedEventArgs : EventArgs
    {
        private bool connected;
        public bool Connected { get { return connected; } }

        public OVRConnectedEventArgs(bool connected) : base()
        {
            this.connected = connected;
        }
    }
}