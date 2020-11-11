## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger


# -*- coding: utf-8 -*-
"""

"""

# from MMIPython.adapter.scene.mmi_scene import mmi_scene
from MMIPython.core.services import service_access
from MMIPython.core.services.service_access import ServiceAccess


class SessionContent(object):
    
    """
    The session content for a specified session ID.
    
    Arguments
    -----------
    _sessionID : str
        The assigned session ID
    scene_access : SceneAccess
        The scene access of this session
    avatar_content : dict
        Dictionary to for the avatar content
    last_access : datetime
        The last time the session has been used
    """
    
    def __init__(self, sessionID, r_Address):
        """
        Initializes the session content for a given session ID
        
        Parameters
        ----------
        sessionID : str
            The session ID as string
        """
        
        assert (sessionID is not None), "sessionID is invalid."
        assert (isinstance(sessionID, str)), "sessionID is no string."
        
        self._sessionID = sessionID
        self.scene_access = None
        self.service_access = ServiceAccess(r_Address, sessionID)
        self.skeleton_access = None
        self.avatar_content = dict()
        self.last_access = None
        self.mmus = dict()
