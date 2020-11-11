#
# Autogenerated by Thrift Compiler (0.13.0)
#
# DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
#
#  options string: py
#

from thrift.Thrift import TType, TMessageType, TFrozenDict, TException, TApplicationException
from thrift.protocol.TProtocol import TProtocolException
from thrift.TRecursive import fix_spec

import sys
import MMIStandard.core.ttypes
import MMIStandard.avatar.ttypes
import MMIStandard.scene.ttypes
import MMIStandard.mmu.ttypes
import MMIStandard.constraints.ttypes

from thrift.transport import TTransport
all_structs = []


class MAdapterDescription(object):
    """
    Attributes:
     - Name
     - ID
     - Language
     - Addresses
     - Properties
     - Parameters

    """


    def __init__(self, Name=None, ID=None, Language=None, Addresses=None, Properties=None, Parameters=None,):
        self.Name = Name
        self.ID = ID
        self.Language = Language
        self.Addresses = Addresses
        self.Properties = Properties
        self.Parameters = Parameters

    def read(self, iprot):
        if iprot._fast_decode is not None and isinstance(iprot.trans, TTransport.CReadableTransport) and self.thrift_spec is not None:
            iprot._fast_decode(self, iprot, [self.__class__, self.thrift_spec])
            return
        iprot.readStructBegin()
        while True:
            (fname, ftype, fid) = iprot.readFieldBegin()
            if ftype == TType.STOP:
                break
            if fid == 1:
                if ftype == TType.STRING:
                    self.Name = iprot.readString().decode('utf-8') if sys.version_info[0] == 2 else iprot.readString()
                else:
                    iprot.skip(ftype)
            elif fid == 2:
                if ftype == TType.STRING:
                    self.ID = iprot.readString().decode('utf-8') if sys.version_info[0] == 2 else iprot.readString()
                else:
                    iprot.skip(ftype)
            elif fid == 3:
                if ftype == TType.STRING:
                    self.Language = iprot.readString().decode('utf-8') if sys.version_info[0] == 2 else iprot.readString()
                else:
                    iprot.skip(ftype)
            elif fid == 4:
                if ftype == TType.LIST:
                    self.Addresses = []
                    (_etype3, _size0) = iprot.readListBegin()
                    for _i4 in range(_size0):
                        _elem5 = MMIStandard.core.ttypes.MIPAddress()
                        _elem5.read(iprot)
                        self.Addresses.append(_elem5)
                    iprot.readListEnd()
                else:
                    iprot.skip(ftype)
            elif fid == 5:
                if ftype == TType.MAP:
                    self.Properties = {}
                    (_ktype7, _vtype8, _size6) = iprot.readMapBegin()
                    for _i10 in range(_size6):
                        _key11 = iprot.readString().decode('utf-8') if sys.version_info[0] == 2 else iprot.readString()
                        _val12 = iprot.readString().decode('utf-8') if sys.version_info[0] == 2 else iprot.readString()
                        self.Properties[_key11] = _val12
                    iprot.readMapEnd()
                else:
                    iprot.skip(ftype)
            elif fid == 6:
                if ftype == TType.LIST:
                    self.Parameters = []
                    (_etype16, _size13) = iprot.readListBegin()
                    for _i17 in range(_size13):
                        _elem18 = MMIStandard.core.ttypes.MParameter()
                        _elem18.read(iprot)
                        self.Parameters.append(_elem18)
                    iprot.readListEnd()
                else:
                    iprot.skip(ftype)
            else:
                iprot.skip(ftype)
            iprot.readFieldEnd()
        iprot.readStructEnd()

    def write(self, oprot):
        if oprot._fast_encode is not None and self.thrift_spec is not None:
            oprot.trans.write(oprot._fast_encode(self, [self.__class__, self.thrift_spec]))
            return
        oprot.writeStructBegin('MAdapterDescription')
        if self.Name is not None:
            oprot.writeFieldBegin('Name', TType.STRING, 1)
            oprot.writeString(self.Name.encode('utf-8') if sys.version_info[0] == 2 else self.Name)
            oprot.writeFieldEnd()
        if self.ID is not None:
            oprot.writeFieldBegin('ID', TType.STRING, 2)
            oprot.writeString(self.ID.encode('utf-8') if sys.version_info[0] == 2 else self.ID)
            oprot.writeFieldEnd()
        if self.Language is not None:
            oprot.writeFieldBegin('Language', TType.STRING, 3)
            oprot.writeString(self.Language.encode('utf-8') if sys.version_info[0] == 2 else self.Language)
            oprot.writeFieldEnd()
        if self.Addresses is not None:
            oprot.writeFieldBegin('Addresses', TType.LIST, 4)
            oprot.writeListBegin(TType.STRUCT, len(self.Addresses))
            for iter19 in self.Addresses:
                iter19.write(oprot)
            oprot.writeListEnd()
            oprot.writeFieldEnd()
        if self.Properties is not None:
            oprot.writeFieldBegin('Properties', TType.MAP, 5)
            oprot.writeMapBegin(TType.STRING, TType.STRING, len(self.Properties))
            for kiter20, viter21 in self.Properties.items():
                oprot.writeString(kiter20.encode('utf-8') if sys.version_info[0] == 2 else kiter20)
                oprot.writeString(viter21.encode('utf-8') if sys.version_info[0] == 2 else viter21)
            oprot.writeMapEnd()
            oprot.writeFieldEnd()
        if self.Parameters is not None:
            oprot.writeFieldBegin('Parameters', TType.LIST, 6)
            oprot.writeListBegin(TType.STRUCT, len(self.Parameters))
            for iter22 in self.Parameters:
                iter22.write(oprot)
            oprot.writeListEnd()
            oprot.writeFieldEnd()
        oprot.writeFieldStop()
        oprot.writeStructEnd()

    def validate(self):
        if self.Name is None:
            raise TProtocolException(message='Required field Name is unset!')
        if self.ID is None:
            raise TProtocolException(message='Required field ID is unset!')
        if self.Language is None:
            raise TProtocolException(message='Required field Language is unset!')
        if self.Addresses is None:
            raise TProtocolException(message='Required field Addresses is unset!')
        return

    def __repr__(self):
        L = ['%s=%r' % (key, value)
             for key, value in self.__dict__.items()]
        return '%s(%s)' % (self.__class__.__name__, ', '.join(L))

    def __eq__(self, other):
        return isinstance(other, self.__class__) and self.__dict__ == other.__dict__

    def __ne__(self, other):
        return not (self == other)
all_structs.append(MAdapterDescription)
MAdapterDescription.thrift_spec = (
    None,  # 0
    (1, TType.STRING, 'Name', 'UTF8', None, ),  # 1
    (2, TType.STRING, 'ID', 'UTF8', None, ),  # 2
    (3, TType.STRING, 'Language', 'UTF8', None, ),  # 3
    (4, TType.LIST, 'Addresses', (TType.STRUCT, [MMIStandard.core.ttypes.MIPAddress, None], False), None, ),  # 4
    (5, TType.MAP, 'Properties', (TType.STRING, 'UTF8', TType.STRING, 'UTF8', False), None, ),  # 5
    (6, TType.LIST, 'Parameters', (TType.STRUCT, [MMIStandard.core.ttypes.MParameter, None], False), None, ),  # 6
)
fix_spec(all_structs)
del all_structs
