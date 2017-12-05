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

    // Lol no monads
    private bool uploadAndTest(){

      log.info("STARTING TESTS & UPLOAD");

      bool success = true;
      log.info("uploading DSP firmware");
      success = (success && uploadMeameBinary());
      resetMail();

      // success = (success && test());
      log.err("running test method that is currently commented out");
      return success;
    }


    public void basicStimTest(int period){
      if(connect()){
        dspDevice.WriteRegister(STIMPACK_SAMPLE, 0x0);
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x1);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x0);

        disconnect();
      }

      issueStim(0);


      if(connect()){
        dspDevice.WriteRegister(STIMPACK_SAMPLE, 0x1);
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period*3);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x0);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x1);

        disconnect();
      }

      issueStim(1);


      if(connect()){
        dspDevice.WriteRegister(STIMPACK_SAMPLE, 0x2);
        dspDevice.WriteRegister(STIMPACK_PERIOD, (uint)period*9);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES0, 0x0);
        dspDevice.WriteRegister(STIMPACK_ELECTRODES1, 0x1000000);

        disconnect();
      }

      issueStim(2);

    }


    public void basicReadTest(){
      if(connect()){

        dspDevice.WriteRegister(DEBUG2, 0x125);
        dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.READ);
        dspDevice.WriteRegister(op1Address, DEBUG2);

        uint return_addr = op2Address;

        nextCommsBuffer();
        dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

        checkRead();

        uint ret = dspDevice.ReadRegister(return_addr);
        log.info($"{ret:X} was read at DEBUG2");
        disconnect();
      }
    }


    public void readTest(){

      Random rnd = new Random();
      uint rval1 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval2 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval3 = 0x123 + (uint)(rnd.Next(1,10));
      uint rval4 = 0x123 + (uint)(rnd.Next(1,10));

      if(connect()){
        dspDevice.WriteRegister(DEBUG1, rval1);
        dspDevice.WriteRegister(DEBUG2, rval2);
        dspDevice.WriteRegister(DEBUG3, rval3);
        dspDevice.WriteRegister(DEBUG4, rval4);

        disconnect();
      }

      uint test1 = readReg(DEBUG1);
      uint test2 = readReg(DEBUG2);
      uint test3 = readReg(DEBUG3);
      uint test4 = readReg(DEBUG4);


      if( (rval1 == test1) &&
          (rval2 == test2) &&
          (rval3 == test3) &&
          (rval4 == test4) ){

        log.ok("Basic read test successful");
      }
      else{
        log.err("Basic read test failed");

        log.info("Directly wrote:");
        log.info($"DEBUG1 set to {rval1:X}");
        log.info($"DEBUG2 set to {rval2:X}");
        log.info($"DEBUG3 set to {rval3:X}");
        log.info($"DEBUG4 set to {rval4:X}");

        log.info("\nIssued reads:");
        log.info($"DEBUG1 read as {test1:X}");
        log.info($"DEBUG2 read as {test2:X}");
        log.info($"DEBUG3 read as {test3:X}");
        log.info($"DEBUG4 read as {test4:X}");
      }
    }


    public void basicWriteTest(){
      if(connect()){

        dspDevice.WriteRegister(DEBUG2, 0x125);

        dspDevice.WriteRegister(opTypeAddress, (uint)DspOps.WRITE);
        dspDevice.WriteRegister(op1Address, DEBUG2);
        dspDevice.WriteRegister(op2Address, 0xFEF);

        nextCommsBuffer();

        dspDevice.WriteRegister(COMMS_BUFFER_MASTER_IDX, instructionIndex);

        checkRead();

        uint ret = dspDevice.ReadRegister(DEBUG2);

        log.info($"{ret:X} was read at DEBUG2");

        disconnect();
      }
    }

    public void writeTest(){
      Random rnd = new Random();

      uint wval1 = 0x323 + (uint)(rnd.Next(1,10));
      uint wval2 = 0x323 + (uint)(rnd.Next(1,10));
      uint wval3 = 0x323 + (uint)(rnd.Next(1,10));
      uint wval4 = 0x323 + (uint)(rnd.Next(1,10));

      writeReg(DEBUG1, wval1);
      writeReg(DEBUG2, wval2);
      writeReg(DEBUG3, wval3);
      writeReg(DEBUG4, wval4);

      uint rval1 = 0xffff;
      uint rval2 = 0xffff;
      uint rval3 = 0xffff;
      uint rval4 = 0xffff;

      if(connect()){
        rval1 = dspDevice.ReadRegister(DEBUG1);
        rval2 = dspDevice.ReadRegister(DEBUG2);
        rval3 = dspDevice.ReadRegister(DEBUG3);
        rval4 = dspDevice.ReadRegister(DEBUG4);

        disconnect();
      }

      if( (wval1 == rval1) &&
          (wval2 == rval2) &&
          (wval3 == rval3) &&
          (wval4 == rval4) ){

        log.ok("Basic write test succesfull");
      }
      else{
        log.err("Basic write test failed");
        log.info("Issued writes:");
        log.info($"DEBUG1 set to {wval1:X}");
        log.info($"DEBUG2 set to {wval2:X}");
        log.info($"DEBUG3 set to {wval3:X}");
        log.info($"DEBUG4 set to {wval4:X}");

        log.info("\nDirecly read");
        log.info($"DEBUG1 read as {rval1:X}");
        log.info($"DEBUG2 read as {rval2:X}");
        log.info($"DEBUG3 read as {rval3:X}");
        log.info($"DEBUG4 read as {rval4:X}");
      }
    }


    public void barfDebug(){
      if(connect()){
        uint[] debug = new uint[10];
        for(int ii = 0; ii < 10; ii++){
          uint addr = (uint)(DEBUG1 + ii*4);
          debug[ii] = dspDevice.ReadRegister(addr);
        }
        for(int ii = 0; ii < 9; ii++){
          log.info($"DEBUG{ii+1} = {debug[ii]:X}");
        }
        disconnect();
      }
    }

    public void barfComms(){
      if(connect()){
        uint c_buf_slave = dspDevice.ReadRegister(COMMS1);
        uint c_buf_master = dspDevice.ReadRegister(COMMS2);
        uint ops_exec = dspDevice.ReadRegister(COMMS3);
        uint last_op_type = dspDevice.ReadRegister(COMMS4);
        uint last_op1 = dspDevice.ReadRegister(COMMS5);
        uint last_op2 = dspDevice.ReadRegister(COMMS6);

        var s = "Barfing comms debug\n"+
          $"comms slave index: {c_buf_slave:X}\n" +
          $"comms master index: {c_buf_master:X}\n" +
          $"comms ops exec: {ops_exec:X}\n" +
          $"comms last op type: {last_op_type:X}\n" +
          $"comms last op1: {last_op1:X}\n" +
          $"comms lats op2: {last_op2:X}\n";

        log.info(s);

        disconnect();
      }
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

    public void disconnect(){
      if(connectCounter != 1){
        log.err("Attempted to disconnect already disconnected device");
        dspDevice.Disconnect();
        return;
      }
      dspDevice.Disconnect();
      connectCounter--;
    }
  }
}
