using UnityEngine;
using System.Collections;

public static class RestrictionManager {	
	public static bool pauseMenu = false;
	public static bool devConsole = false;
	public static bool allInput = false;

	private static bool m_Restricted = false;
	public static bool restricted {
		get {
			m_Restricted = (pauseMenu || devConsole || allInput);
			return m_Restricted;
		}
		set {
			//You can only set restriction = false, which will disable all current restrictions.
			//If you want to set the restriction = true, then enable one of the above.
			if(value == false) {
				pauseMenu = false;
				devConsole = false;
				allInput = false;
				m_Restricted = false;
			}
		}
	}

    //Independent from 'restricted'
    public static bool mpMatchRestrict {
        get {
            bool restrict = false;
            if(Topan.Network.isConnected && GeneralVariables.Networking != null) {
                restrict = !GeneralVariables.Networking.matchStarted;
            }

            return restrict;
        }
    }
}