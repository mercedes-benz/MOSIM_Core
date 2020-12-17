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

class MSynchronizableScene_ApplyUpdates_args
{
    static public $isValidate = false;

    static public $_TSPEC = array(
        1 => array(
            'var' => 'sceneUpdates',
            'isRequired' => false,
            'type' => TType::STRUCT,
            'class' => '\MSceneUpdate',
        ),
    );

    /**
     * @var \MSceneUpdate
     */
    public $sceneUpdates = null;

    public function __construct($vals = null)
    {
        if (is_array($vals)) {
            if (isset($vals['sceneUpdates'])) {
                $this->sceneUpdates = $vals['sceneUpdates'];
            }
        }
    }

    public function getName()
    {
        return 'MSynchronizableScene_ApplyUpdates_args';
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
                        $this->sceneUpdates = new \MSceneUpdate();
                        $xfer += $this->sceneUpdates->read($input);
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
        $xfer += $output->writeStructBegin('MSynchronizableScene_ApplyUpdates_args');
        if ($this->sceneUpdates !== null) {
            if (!is_object($this->sceneUpdates)) {
                throw new TProtocolException('Bad type in structure.', TProtocolException::INVALID_DATA);
            }
            $xfer += $output->writeFieldBegin('sceneUpdates', TType::STRUCT, 1);
            $xfer += $this->sceneUpdates->write($output);
            $xfer += $output->writeFieldEnd();
        }
        $xfer += $output->writeFieldStop();
        $xfer += $output->writeStructEnd();
        return $xfer;
    }
}