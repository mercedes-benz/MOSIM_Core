## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald

# -*- coding: utf-8 -*-
"""

"""

class AvatarContent(object):
    """
    Avatar specific content (e.g. the MMU instances)
    
    Attributes
    ----------
    _avatar_id : str
        The avatar id
    mmus : dict
        The dictionary of MMUs of the session
    intermediate_posture : MAvatarPosture
        The posture of the reference avatar
    """
    
    
    def __init__(self, avatarID):
        """
        Constructor which takes an ID for the Avatar as input
        
        Parameters
        ----------
        avatarID : str
            The avatar id as string
        """
        
        assert (isinstance(avatarID, str)), "avatarID is no string."
        
        self._avatar_id = avatarID
        self.mmus = dict()
     