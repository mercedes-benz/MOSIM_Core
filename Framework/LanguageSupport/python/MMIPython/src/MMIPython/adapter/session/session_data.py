## SPDX-License-Identifier: MIT
## The content of this file has been developed in the context of the MOSIM research project.
## Original author(s): Jannes Lehwald, Janis Sprenger

# -*- coding: utf-8 -*-
"""
Class which contains the data of the sessions and MMUs
"""
from datetime import datetime
from MMIPython.adapter.session.avatar_content import AvatarContent
from MMIPython.adapter.session.session_content import SessionContent
from MMIStandard.core.ttypes import MBoolResponse

import MMIPython.core.scene.remote_scene_access as remote_scene_access
import MMIPython.core.skeleton.remote_skeleton_access as remote_skeleton_access

class SessionData(object):
    """
    Class which contains the data of the sessions and MMUs
    
    Attributes
    -----------
    service_access : ServiceAccess
        The service access
    session_contents : dict
        Dictionary which contains all sessions
    mmu_loading_properties
        list with file path and MMUDescription tuple
    mmu_descriptions
        list with MMUDescriptions
    last_access : datetime
        The time this object was last accessed
    start_time : datetime
        The time this object was started
    adapter_description : MAdapterDescription
    
    """
    session_contents = dict()
    mmu_loading_properties = list()
    mmu_descriptions = list()
    last_access = None
    start_time = datetime.now()
    adapter_description = None
    r_Address = None
    mmus = dict()

    def __init__(self, r_Address):
        self.r_Address = r_Address

    @staticmethod
    def get_splitted_IDs(sessionID):
        """
        Takes a session ID and splits it into scene and avatar ID
        
        Parameters
        -----------
        sessionID : str
            The session ID as string
        
        Returns
        ----------
        str
            The scene ID as string
        str
            The avatar ID as string
        """
        
        # Debug assertions
        assert (sessionID is not None), "sessionID is invalid."
        assert (isinstance(sessionID, str)), "sessionID is no string."
        
        # Split session ID
        splitted = sessionID.split(':')
        
        # Get scene ID
        sceneID = splitted[0];
        
        # Get avatar ID if available
        if len(splitted) > 1: 
            avatarID = splitted[1]
        else:
            avatarID = "0";
        
        return sceneID, avatarID

    def get_session_content(self, sessionID):
        """
        Returns a session content based on the given session Id
        
        Parameters
        ----------
        sessionID : str
            The session ID
        
        Returns
        -------
        SessionContent
            The found session content
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
    
        
        session_content = None
        
        # split session ID
        sceneID, avatarID = SessionData.get_splitted_IDs(sessionID)
        
        if sceneID in self.session_contents:
            
            # Get the session content
            session_content = self.session_contents[sceneID]
                    
        return session_content

    def getPossibleMMUs(self):
        return self.mmus

    def LoadMMU(self, sessionID, mmuID):
        """
        Add an initialized MMU to the session content. 

        Args:
            sessionID ([type]): session ID
            mmuID ([type]): MMU ID
            mmu ([type]): initialized MMU instance
        """
        if mmuID in self.mmus:
            mmu = self.mmus[mmuID](self.get_session_content(sessionID).service_access, self.get_session_content(sessionID).scene_access)
            self.get_session_content(sessionID).mmus[mmuID] = mmu
            return True
        else:
            return False

    def GetMMU(self, sessionID, mmuID):
        """
        Returns initialized MMU instance if existend.

        Args:
            sessionID ([type]): session ID
            mmuID ([type]): mmu ID

        Returns:
            MotionModelUnit - None if no initialized MMU found for this session
        """
        sc = self.get_session_content(sessionID)
        if not sc is None and mmuID in sc.mmus:
            return sc.mmus[mmuID]
        else:
            return None

    def GetLoadedMMUs(self, sessionID):
        """
        Returns list of loaded MMU IDs for session.

        Args:
            sessionID ([type]): [description]
        """
        return self.get_session_content(sessionID).mmus.keys()

    def get_avatar_content(self, sessionID, create = False):
        """
        Returns an avatar content based on the given session Id
        
        Parameters
        ----------
        sessionID : str
            The session ID
    
            
        Returns
        -----------
        AvatarContent
            The found Avatar content
        """
        
        assert (isinstance(sessionID, str)),"SessionID is no string"
        
        avatar_content = None
        
        # split session ID
        _, avatarID = SessionData.get_splitted_IDs(sessionID)
        
        # Get the session content
        session_content = self.get_session_content(sessionID)
        
        if session_content is not None:
            
            
            if avatarID in session_content.avatar_content:
                
                # Get avatar content
                avatar_content = session_content.avatar_content[avatarID]
                
            elif create:
                
                # Create avatar content
                avatar_content = AvatarContent(avatarID)
                
                # Add the created avatar content
                session_content.avatar_content[avatarID] = avatar_content
                    
        return avatar_content


    def create_session_content(self, sessionID):
        """
        Creates the session content for the given session ID and returns it
        
        Parameters
        -----------
        sessionID : str
            The session ID as string
    
        
        Returns
        --------
        SessionContent
            The session content
        """
        
        # Debug assertions
        assert (isinstance(sessionID, str)), "sessionID is no string."
        
        # Split the session ID
        sceneID, avatarID = SessionData.get_splitted_IDs(sessionID)
        
        session_content = None
        
        if sceneID in self.session_contents:
            # Session already avilable
            session_content = self.session_contents[sceneID]
            
            if avatarID not in session_content.avatar_content:
                
                # Avatar content missing -> Create and add it
                avatar_content = AvatarContent(avatarID)
                session_content.avatar_content[avatarID] = avatar_content
                
        else:
            # Create new session content
            session_content = SessionContent(sessionID, self.r_Address)
            
            # Add new avatar content
            avatar_content = AvatarContent(avatarID)
            session_content.avatar_content[avatarID] = avatar_content
            
            # Add the created session content to the session data object
            self.session_contents[sceneID] = session_content
        if session_content.scene_access is None:
            session_content.scene_access = remote_scene_access.initialize(self.r_Address)
        if session_content.skeleton_access is None:
            session_content.skeleton_access = remote_skeleton_access.initialize(self.r_Address)
        return session_content


    def removeSessionContent(self, sessionID):
        assert (isinstance(sessionID, str)),"SessionID is no string"
        result = MBoolResponse(True)
        session_content = None
        
        # split session ID
        sceneID, avatarID = SessionData.get_splitted_IDs(sessionID)
        
        if sceneID in self.session_contents:
            self.session_contents.pop(sceneID)
            return result
        else:
            result.LogData=["Can not find Session content with ID: " + sceneID]
            result.Successfull=False
            return result

        
        
        
        