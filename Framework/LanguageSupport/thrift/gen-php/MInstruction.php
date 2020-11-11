<?php
/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
use Thrift\Base\TBase;
use Thrift\Type\TType;
use Thrift\Type\TMessageType;
use Thrift\Exception\TException;
use Thrift\Exception\TProtocolException;
use Thrift\Protocol\TProtocol;
use Thrift\Protocol\TBinaryProtocolAccelerated;
use Thrift\Exception\TApplicationException;

class MInstruction
{
    static public $isValidate = false;

    static public $_TSPEC = array(
        1 => array(
            'var' => 'ID',
            'isRequired' => true,
            'type' => TType::STRING,
        ),
        2 => array(
            'var' => 'Name',
            'isRequired' => true,
            'type' => TType::STRING,
        ),
        3 => array(
            'var' => 'MotionType',
            'isRequired' => true,
            'type' => TType::STRING,
        ),
        4 => array(
            'var' => 'Properties',
            'isRequired' => false,
            'type' => TType::MAP,
            'ktype' => TType::STRING,
            'vtype' => TType::STRING,
            'key' => array(
                'type' => TType::STRING,
            ),
            'val' => array(
                'type' => TType::STRING,
                ),
        ),
        5 => array(
            'var' => 'Constraints',
            'isRequired' => false,
            'type' => TType::LST,
            'etype' => TType::STRUCT,
            'elem' => array(
                'type' => TType::STRUCT,
                'class' => '\MConstraint',
                ),
        ),
        6 => array(
            'var' => 'StartCondition',
            'isRequired' => false,
            'type' => TType::STRING,
        ),
        7 => array(
            'var' => 'EndCondition',
            'isRequired' => false,
            'type' => TType::STRING,
        ),
        8 => array(
            'var' => 'Action',
            'isRequired' => false,
            'type' => TType::STRING,
        ),
        9 => array(
            'var' => 'Instructions',
            'isRequired' => false,
            'type' => TType::LST,
            'etype' => TType::STRUCT,
            'elem' => array(
                'type' => TType::STRUCT,
                'class' => '\MInstruction',
                ),
        ),
    );

    /**
     * @var string
     */
    public $ID = null;
    /**
     * @var string
     */
    public $Name = null;
    /**
     * @var string
     */
    public $MotionType = null;
    /**
     * @var array
     */
    public $Properties = null;
    /**
     * @var \MConstraint[]
     */
    public $Constraints = null;
    /**
     * @var string
     */
    public $StartCondition = null;
    /**
     * @var string
     */
    public $EndCondition = null;
    /**
     * @var string
     */
    public $Action = null;
    /**
     * @var \MInstruction[]
     */
    public $Instructions = null;

    public function __construct($vals = null)
    {
        if (is_array($vals)) {
            if (isset($vals['ID'])) {
                $this->ID = $vals['ID'];
            }
            if (isset($vals['Name'])) {
                $this->Name = $vals['Name'];
            }
            if (isset($vals['MotionType'])) {
                $this->MotionType = $vals['MotionType'];
            }
            if (isset($vals['Properties'])) {
                $this->Properties = $vals['Properties'];
            }
            if (isset($vals['Constraints'])) {
                $this->Constraints = $vals['Constraints'];
            }
            if (isset($vals['StartCondition'])) {
                $this->StartCondition = $vals['StartCondition'];
            }
            if (isset($vals['EndCondition'])) {
                $this->EndCondition = $vals['EndCondition'];
            }
            if (isset($vals['Action'])) {
                $this->Action = $vals['Action'];
            }
            if (isset($vals['Instructions'])) {
                $this->Instructions = $vals['Instructions'];
            }
        }
    }

    public function getName()
    {
        return 'MInstruction';
    }


    public function read($input)
    {
        $xfer = 0;
        $fname = null;
        $ftype = 0;
        $fid = 0;
        $xfer += $input->readStructBegin($fname);
        while (true) {
            $xfer += $input->readFieldBegin($fname, $ftype, $fid);
            if ($ftype == TType::STOP) {
                break;
            }
            switch ($fid) {
                case 1:
                    if ($ftype == TType::STRING) {
                        $xfer += $input->readString($this->ID);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 2:
                    if ($ftype == TType::STRING) {
                        $xfer += $input->readString($this->Name);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 3:
                    if ($ftype == TType::STRING) {
                        $xfer += $input->readString($this->MotionType);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 4:
                    if ($ftype == TType::MAP) {
                        $this->Properties = array();
                        $_size116 = 0;
                        $_ktype117 = 0;
                        $_vtype118 = 0;
                        $xfer += $input->readMapBegin($_ktype117, $_vtype118, $_size116);
                        for ($_i120 = 0; $_i120 < $_size116; ++$_i120) {
                            $key121 = '';
                            $val122 = '';
                            $xfer += $input->readString($key121);
                            $xfer += $input->readString($val122);
                            $this->Properties[$key121] = $val122;
                        }
                        $xfer += $input->readMapEnd();
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 5:
                    if ($ftype == TType::LST) {
                        $this->Constraints = array();
                        $_size123 = 0;
                        $_etype126 = 0;
                        $xfer += $input->readListBegin($_etype126, $_size123);
                        for ($_i127 = 0; $_i127 < $_size123; ++$_i127) {
                            $elem128 = null;
                            $elem128 = new \MConstraint();
                            $xfer += $elem128->read($input);
                            $this->Constraints []= $elem128;
                        }
                        $xfer += $input->readListEnd();
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 6:
                    if ($ftype == TType::STRING) {
                        $xfer += $input->readString($this->StartCondition);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 7:
                    if ($ftype == TType::STRING) {
                        $xfer += $input->readString($this->EndCondition);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 8:
                    if ($ftype == TType::STRING) {
                        $xfer += $input->readString($this->Action);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 9:
                    if ($ftype == TType::LST) {
                        $this->Instructions = array();
                        $_size129 = 0;
                        $_etype132 = 0;
                        $xfer += $input->readListBegin($_etype132, $_size129);
                        for ($_i133 = 0; $_i133 < $_size129; ++$_i133) {
                            $elem134 = null;
                            $elem134 = new \MInstruction();
                            $xfer += $elem134->read($input);
                            $this->Instructions []= $elem134;
                        }
                        $xfer += $input->readListEnd();
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                default:
                    $xfer += $input->skip($ftype);
                    break;
            }
            $xfer += $input->readFieldEnd();
        }
        $xfer += $input->readStructEnd();
        return $xfer;
    }

    public function write($output)
    {
        $xfer = 0;
        $xfer += $output->writeStructBegin('MInstruction');
        if ($this->ID !== null) {
            $xfer += $output->writeFieldBegin('ID', TType::STRING, 1);
            $xfer += $output->writeString($this->ID);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->Name !== null) {
            $xfer += $output->writeFieldBegin('Name', TType::STRING, 2);
            $xfer += $output->writeString($this->Name);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->MotionType !== null) {
            $xfer += $output->writeFieldBegin('MotionType', TType::STRING, 3);
            $xfer += $output->writeString($this->MotionType);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->Properties !== null) {
            if (!is_array($this->Properties)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('Properties', TType::MAP, 4);
            $output->writeMapBegin(TType::STRING, TType::STRING, count($this->Properties));
            foreach ($this->Properties as $kiter135 => $viter136) {
                $xfer += $output->writeString($kiter135);
                $xfer += $output->writeString($viter136);
            }
            $output->writeMapEnd();
            $xfer += $output->writeFieldEnd();
        }
        if ($this->Constraints !== null) {
            if (!is_array($this->Constraints)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('Constraints', TType::LST, 5);
            $output->writeListBegin(TType::STRUCT, count($this->Constraints));
            foreach ($this->Constraints as $iter137) {
                $xfer += $iter137->write($output);
            }
            $output->writeListEnd();
            $xfer += $output->writeFieldEnd();
        }
        if ($this->StartCondition !== null) {
            $xfer += $output->writeFieldBegin('StartCondition', TType::STRING, 6);
            $xfer += $output->writeString($this->StartCondition);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->EndCondition !== null) {
            $xfer += $output->writeFieldBegin('EndCondition', TType::STRING, 7);
            $xfer += $output->writeString($this->EndCondition);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->Action !== null) {
            $xfer += $output->writeFieldBegin('Action', TType::STRING, 8);
            $xfer += $output->writeString($this->Action);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->Instructions !== null) {
            if (!is_array($this->Instructions)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('Instructions', TType::LST, 9);
            $output->writeListBegin(TType::STRUCT, count($this->Instructions));
            foreach ($this->Instructions as $iter138) {
                $xfer += $iter138->write($output);
            }
            $output->writeListEnd();
            $xfer += $output->writeFieldEnd();
        }
        $xfer += $output->writeFieldStop();
        $xfer += $output->writeStructEnd();
        return $xfer;
    }
}
