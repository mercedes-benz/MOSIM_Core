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

class MCoordinateSystemMapper_VectorFromMMI_L_args
{
    static public $isValidate = false;

    static public $_TSPEC = array(
        1 => array(
            'var' => 'quat',
            'isRequired' => false,
            'type' => TType::STRUCT,
            'class' => '\MVector3',
        ),
        2 => array(
            'var' => 'coordinateSystem',
            'isRequired' => false,
            'type' => TType::LST,
            'etype' => TType::I32,
            'elem' => array(
                'type' => TType::I32,
                ),
        ),
    );

    /**
     * @var \MVector3
     */
    public $quat = null;
    /**
     * @var int[]
     */
    public $coordinateSystem = null;

    public function __construct($vals = null)
    {
        if (is_array($vals)) {
            if (isset($vals['quat'])) {
                $this->quat = $vals['quat'];
            }
            if (isset($vals['coordinateSystem'])) {
                $this->coordinateSystem = $vals['coordinateSystem'];
            }
        }
    }

    public function getName()
    {
        return 'MCoordinateSystemMapper_VectorFromMMI_L_args';
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
                    if ($ftype == TType::STRUCT) {
                        $this->quat = new \MVector3();
                        $xfer += $this->quat->read($input);
                    } else {
                        $xfer += $input->skip($ftype);
                    }
                    break;
                case 2:
                    if ($ftype == TType::LST) {
                        $this->coordinateSystem = array();
                        $_size359 = 0;
                        $_etype362 = 0;
                        $xfer += $input->readListBegin($_etype362, $_size359);
                        for ($_i363 = 0; $_i363 < $_size359; ++$_i363) {
                            $elem364 = null;
                            $xfer += $input->readI32($elem364);
                            $this->coordinateSystem []= $elem364;
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
        $xfer += $output->writeStructBegin('MCoordinateSystemMapper_VectorFromMMI_L_args');
        if ($this->quat !== null) {
            if (!is_object($this->quat)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('quat', TType::STRUCT, 1);
            $xfer += $this->quat->write($output);
            $xfer += $output->writeFieldEnd();
        }
        if ($this->coordinateSystem !== null) {
            if (!is_array($this->coordinateSystem)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('coordinateSystem', TType::LST, 2);
            $output->writeListBegin(TType::I32, count($this->coordinateSystem));
            foreach ($this->coordinateSystem as $iter365) {
                $xfer += $output->writeI32($iter365);
            }
            $output->writeListEnd();
            $xfer += $output->writeFieldEnd();
        }
        $xfer += $output->writeFieldStop();
        $xfer += $output->writeStructEnd();
        return $xfer;
    }
}
