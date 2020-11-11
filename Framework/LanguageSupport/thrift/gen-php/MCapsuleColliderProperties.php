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

class MCapsuleColliderProperties
{
    static public $isValidate = false;

    static public $_TSPEC = array(
        1 => array(
            'var' => 'Radius',
            'isRequired' => true,
            'type' => TType::DOUBLE,
        ),
        2 => array(
            'var' => 'Height',
            'isRequired' => true,
            'type' => TType::DOUBLE,
        ),
        3 => array(
            'var' => 'MainAxis',
            'isRequired' => false,
            'type' => TType::STRUCT,
            'class' => '\MVector3',
        ),
    );

    /**
     * @var double
     */
    public $Radius = null;
    /**
     * @var double
     */
    public $Height = null;
    /**
     * @var \MVector3
     */
    public $MainAxis = null;

    public function __construct($vals = null)
    {
        if (is_array($vals)) {
            if (isset($vals['Radius'])) {
                $this->Radius = $vals['Radius'];
            }
            if (isset($vals['Height'])) {
                $this->Height = $vals['Height'];
            }
            if (isset($vals['MainAxis'])) {
                $this->MainAxis = $vals['MainAxis'];
            }
        }
    }

    public function getName()
    {
        return 'MCapsuleColliderProperties';
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
                    if ($ftype == TType::DOUBLE) {
                        $xfer += $input->readDouble($this->Radius);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 2:
                    if ($ftype == TType::DOUBLE) {
                        $xfer += $input->readDouble($this->Height);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 3:
                    if ($ftype == TType::STRUCT) {
                        $this->MainAxis = new \MVector3();
                        $xfer += $this->MainAxis->read($input);
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
        $xfer += $output->writeStructBegin('MCapsuleColliderProperties');
        if ($this->Radius !== null) {
            $xfer += $output->writeFieldBegin('Radius', TType::DOUBLE, 1);
            $xfer += $output->writeDouble($this->Radius);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->Height !== null) {
            $xfer += $output->writeFieldBegin('Height', TType::DOUBLE, 2);
            $xfer += $output->writeDouble($this->Height);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->MainAxis !== null) {
            if (!is_object($this->MainAxis)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('MainAxis', TType::STRUCT, 3);
            $xfer += $this->MainAxis->write($output);
            $xfer += $output->writeFieldEnd();
        }
        $xfer += $output->writeFieldStop();
        $xfer += $output->writeStructEnd();
        return $xfer;
    }
}
