using System;
using Valve.VR;

namespace sh_akira.OVRTracking
{
    public class OVREventArgs : EventArgs
    {
        private VREvent_t _pEvent;
        public VREvent_t pEvent { get { return _pEvent; } }

        public OVREventArgs(VREvent_t pevent) : base()
        {
            _pEvent = pevent;
        }
    }
}