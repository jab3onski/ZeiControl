const byte CameraEnable[4] = {0xFF, 0xFF, 0xFF, 0xFF};
const byte CameraDisable[4] = {0x00, 0x00, 0x00, 0x00};
const byte cameraConfirmedSD[4] = {0x00, 0x00, 0xFF, 0x00};
const byte cameraConfirmedHD[4] = {0x00, 0x00, 0xFF, 0xFF};

const byte WiFiReady[8] = {0x23, 0x53, 0x57, 0x00, 0x00, 0x5F, 0xFF, 0x23};
const byte WiFiDown[8] = {0x23, 0x53, 0x57, 0x00, 0x00, 0x5F, 0x00, 0x23};
const byte requestPictureESP[8] = {0x23, 0x53, 0x57, 0x00, 0xFF, 0x5F, 0x00, 0x23};
const byte EMStReq[8] = {0x23, 0x53, 0x57, 0xFF, 0x00, 0x5F, 0x00, 0x23};

void ApplyNewConfig(unsigned short switchValue)
{
  sensor_t * s = esp_camera_sensor_get();

  switch(switchValue)
      {
        case 0:
          s->set_framesize(s, FRAMESIZE_XGA);
          //s->set_quality(s, 12);
          cameraQualityPreset = 0;
          break;
        case 1:
          s->set_framesize(s, FRAMESIZE_VGA);
          //s->set_quality(s, 25);
          cameraQualityPreset = 1;
          break;
        case 2:
          s->set_framesize(s, FRAMESIZE_QVGA);
          //s->set_quality(s, 25);
          cameraQualityPreset = 2;
          break;
      }
}

void SendWiFiReadyMessage()
{
  Serial.write(WiFiReady, 8);
}

void SendWiFiDownMessage()
{
  Serial.write(WiFiDown, 8);
}

void FlushSocketBuffer(WiFiClient client)
{
  if(client.available() > 0)
  {
    client.flush();
  }
}

void ProcessIncomingSerialData(byte byteData[], WiFiClient client)
{
  byte errorPacket[8] = {0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x23};
  
  if(byteData[0] == 0x23 and byteData[7] == 0x23)
  {
    if(byteData[1] == 0x53 and byteData[2] == 0x57)
    {
      ImageSavePrepare();
    }
    else client.write(byteData, 8);
  }
  else client.write(errorPacket, 8);
}

void ProcessIncomingWiFiData(byte byteData[], WiFiClient client)
{ 
  if(byteData[0] == 0x23 and byteData[7] == 0x23)
  {
    byte value16Bit[2] = {byteData[5], byteData[6]};
    byte value32Bit[4] = {byteData[3], byteData[4], byteData[5], byteData[6]};
  
    //If the command code refers to this device, process without sending to Arduino Uno
    if(byteData[1] == 0x43)
    {
      if(memcmp(value32Bit, CameraEnable, sizeof(value32Bit)) == 0)
      {
        ApplyNewConfig(1);
        cameraEnable = true;
      }
      else if(memcmp(value32Bit, CameraDisable, sizeof(value32Bit)) == 0)
      {
        cameraEnable = false;
      }
      else if(memcmp(value32Bit, cameraConfirmedSD, sizeof(value32Bit)) == 0)
      {
        ImageRequestCapture(client, true);
      }
      else if(memcmp(value32Bit, cameraConfirmedHD, sizeof(value32Bit)) == 0)
      {
        ImageRequestCapture(client, false);
      }
    }
    
    else if(byteData[1] == 0x46 and byteData[3] == 0x00 and byteData[4] == 0xFF)
    {
      unsigned short value;
      value = (uint16_t) value16Bit[1] << 8;
      value |= (uint16_t) value16Bit[0];
      ApplyNewConfig(value);
    }
    //If not, reroute to Arduino Uno for further processing
    else Serial.write(byteData, 8);
  }
}

byte DataCheckProcess(WiFiClient client)
{
  byte serialDataBytes[8] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
  
  if(Serial.peek() == 0x23)
      {
        serialDataBytes[0] = Serial.read();
        if(Serial.peek() != 0x23)
        {
          for(byte i = 1; i < 8; i++)
          {
            serialDataBytes[i] = Serial.read();
          }
        }
        else
        {
          Serial.read();
          for(byte i = 1; i < 7; i++)
          {
            serialDataBytes[i] = Serial.read();
          }
          serialDataBytes[7] = 0x23;
        }
        ProcessIncomingSerialData(serialDataBytes, client);
      }
  else Serial.read();
}
