using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace MEAME2
{
  using Mcs.Usb;

  public partial class DSPComms {

    private int connectCounter = 0;

    public void basicStimTest(int period){
      if(connect()){
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x1);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x0);

        disconnect();
      }

      issueStim(0);


      if(connect()){
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period*3);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x40);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x0);

        disconnect();
      }

      issueStim(1);


      if(connect()){
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period*7);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x400);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x0);

        disconnect();
      }

      issueStim(2);


      if(connect()){
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period*11);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x200000);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x0);

        disconnect();
      }

      issueStim(3);
    }


    private bool connect(){
      if(connectCounter != 0){
        log.err("Attempted to connect before disconnecting");
        dspDevice.Disconnect();
        return false;
      }
      else if(dspDevice.Connect(dspPort, lockMask) == 0){
        connectCounter++;
        return true;
      }
      else{
        log.err("Unable to connect to DSP");
        return false;
      }
    }

    private void disconnect(){
      if(connectCounter != 1){
        log.err("Attempted to disconnect already disconnected device");
        dspDevice.Disconnect();
        return;
      }
      dspDevice.Disconnect();
      connectCounter--;
    }

    private void nextCommsBuffer(){
      if(instructionIndex == (instructionQsize - 1))
        instructionIndex = 0;
      else
        instructionIndex++;

      opTypeAddress = COMMS_BUFFER_START + ((4*instructionIndex)*wordsPerInstruction) + 0x0;
      op1Address = COMMS_BUFFER_START    + ((4*instructionIndex)*wordsPerInstruction) + 0x4;
      op2Address = COMMS_BUFFER_START    + ((4*instructionIndex)*wordsPerInstruction) + 0x8;
    }

    public void resetMail(){
      log.info("resetting mail");
      if(connect())
        {

          uint opTypeAddress = this.opTypeAddress;
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.RESET);

          bool success = false;

          for(int ii = 0; ii < 10; ii++){
            if(dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX) == 0x0){
              success = true;
              break;
            }
            Thread.Sleep(100);
          }

          if(success){
            log.ok("successfully reset device mailbox");
          }
          else{
            log.err("failed to reset device mailbox");
          }

          nextCommsBuffer();
          instructionIndex = 0;
          disconnect();
        }
      else {
        log.err("failed to connect to device");
      }
    }


    public bool writeReg(uint addr, uint val){
      if(connect())
        {
          dspDevice.WriteRegister(ERROR, 0x0);
          dspDevice.WriteRegister(ERROR_VAL, 0x0);

          uint opTypeAddress           = this.opTypeAddress;
          uint writeAddressAddress     = this.op1Address;
          uint valueToBeWrittenAddress = this.op2Address;

          // addr val
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.WRITE);
          dspDevice.WriteRegister(writeAddressAddress, addr);
          dspDevice.WriteRegister(valueToBeWrittenAddress, val);

          uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
          uint dbgRead = dspDevice.ReadRegister(addr);

          nextCommsBuffer();

          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);
          bool success = checkRead();

          disconnect();

          if(!success){
            log.err("write failure");
          }
          return success;
        }
      else{
        log.err("Write unable to connect to device");
        return false;
      }
    }


    public uint readReg(uint addr){
      if(connect())
        {
          // log.info("Performing read reg");

          uint opTypeAddress       = this.opTypeAddress;
          uint readAddressAddress  = this.op1Address;
          uint deviceResultAddress = this.op2Address;

          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.READ);
          dspDevice.WriteRegister(readAddressAddress, addr);

          uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
          uint returnAddress = readAddressAddress;

          nextCommsBuffer();

          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

          uint rval = 0xDEAD;
          bool success = checkRead();

          rval = dspDevice.ReadRegister(deviceResultAddress);

          disconnect();

          if(!success){
            log.err("read failure");
          }

          return rval;
        }
      else{
        log.err("read is Unable to connect to device");
        return 0xDEAD;
      }
    }


    public void stimReq(StimReq s){
      log.err("stimReq is NOT IMPLEMENTED");
    }


    private bool issueStim(uint stimgroup){
      if(connect())
        {
          dspDevice.WriteRegister(ERROR, 0x0);
          dspDevice.WriteRegister(ERROR_VAL, 0x0);

          uint opTypeAddress           = this.opTypeAddress;
          uint DAC                     = this.op1Address;

          // addr val
          dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.STIMPACK);
          dspDevice.WriteRegister(DAC, stimgroup);

          uint oldBufferIdx = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);

          nextCommsBuffer();

          dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);
          bool success = checkRead();

          disconnect();

          if(!success){
            log.err("stim failure");
          }
          return success;
        }
      else{
        log.err("Write unable to connect to device");
        return false;
      }
    }


    private bool checkRead(){
      bool success = false;
      for(int ii = 0; ii < 10; ii++){
        uint tmp = dspDevice.ReadRegister(COMMS_BUFFER_SLAVE_IDX);
        if(tmp == instructionIndex){
          success = true;
          break;
        }
        Thread.Sleep(100);
        log.err($"retrying comms slave read");
      }
      uint err = dspDevice.ReadRegister(ERROR);
      if(err == 0x1){
        uint errVal = dspDevice.ReadRegister(ERROR_VAL);
        uint errOp1 = dspDevice.ReadRegister(ERROR_OP1);
        uint errOp2 = dspDevice.ReadRegister(ERROR_OP2);
        log.err($"DSP reports error {err} with error val {errVal:X}, {errOp1:X}, {errOp2:X}");
        return false;
      }

      return success;
    }
  }
}
