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

class MJointPathConstraint
{
    static public $isValidate = false;

    static public $_TSPEC = array(
        1 => array(
            'var' => 'JointType',
            'isRequired' => true,
            'type' => TType::I32,
        ),
        2 => array(
            'var' => 'PathConstraint',
            'isRequired' => true,
            'type' => TType::STRUCT,
            'class' => '\MPathConstraint',
        ),
    );

    /**
     * @var int
     */
    public $JointType = null;
    /**
     * @var \MPathConstraint
     */
    public $PathConstraint = null;

    public function __construct($vals = null)
    {
        if (is_array($vals)) {
            if (isset($vals['JointType'])) {
                $this->JointType = $vals['JointType'];
            }
            if (isset($vals['PathConstraint'])) {
                $this->PathConstraint = $vals['PathConstraint'];
            }
        }
    }

    public function getName()
    {
        return 'MJointPathConstraint';
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
                    if ($ftype == TType::I32) {
                        $xfer += $input->readI32($this->JointType);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 2:
                    if ($ftype == TType::STRUCT) {
                        $this->PathConstraint = new \MPathConstraint();
                        $xfer += $this->PathConstraint->read($input);
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
        $xfer += $output->writeStructBegin('MJointPathConstraint');
        if ($this->JointType !== null) {
            $xfer += $output->writeFieldBegin('JointType', TType::I32, 1);
            $xfer += $output->writeI32($this->JointType);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->PathConstraint !== null) {
            if (!is_object($this->PathConstraint)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('PathConstraint', TType::STRUCT, 2);
            $xfer += $this->PathConstraint->write($output);
            $xfer += $output->writeFieldEnd();
        }
        $xfer += $output->writeFieldStop();
        $xfer += $output->writeStructEnd();
        return $xfer;
    }
}