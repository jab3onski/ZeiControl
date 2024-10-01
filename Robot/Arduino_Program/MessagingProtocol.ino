//Move Commands
const byte Forward[4] = {0xFF, 0x00, 0x00, 0x00};
const byte NormalStop[4] = {0xFF, 0xFF, 0xFF, 0xFF};
const byte Left[4] = {0x00, 0xFF, 0x00, 0x00};
const byte Right[4] = {0x00, 0x00, 0xFF, 0x00};
const byte Reverse[4] = {0x00, 0x00, 0x00, 0xFF};
const byte ReverseLeft[4] = {0x00, 0xFF, 0x00, 0xFF};
const byte ReverseRight[4] = {0x00, 0x00, 0xFF, 0xFF};
const byte RelaysOpenP[4] = {0x00, 0x00, 0x00, 0x00};
const byte RelaysClosedP[4] = {0xFF, 0xFF, 0xFF, 0x00};

//Peripherals Commands
const byte PowerLEDOn[4] = {0x00, 0xFF, 0xFF, 0xFF}; 
const byte PowerLEDOff[4] = {0x00, 0xFF, 0x00, 0x00}; 
const byte BuzzerOn[4] = {0xFF, 0xFF, 0xFF, 0xFF}; 
const byte BuzzerOff[4] = {0x00, 0x00, 0x00, 0x00};

//Internal Commands
const byte WiFiReady[8] = {0x23, 0x53, 0x57, 0x00, 0x00, 0x5F, 0xFF, 0x23};
const byte WiFiDown[8] = {0x23, 0x53, 0x57, 0x00, 0x00, 0x5F, 0x00, 0x23};
const byte requestPictureESP[8] = {0x23, 0x53, 0x57, 0x00, 0xFF, 0x5F, 0x00, 0x23};
const byte readyToMoveTrue[8] = {0x23, 0x53, 0x57, 0x00, 0xFF, 0x5F, 0xFF, 0x23};

//Autonomous Driving Commands
const byte AutoDrivingOnConfirmation[8] = {0x23, 0x41, 0x5F, 0xFF, 0x00, 0x00, 0xFF, 0x23};
const byte AutoDrivingOffConfirmation[8] = {0x23, 0x41, 0x5F, 0xFF, 0x00, 0x00, 0x00, 0x23};

//Interval Set Commands
void SetTempInterval(byte intervalValue[])
{
  unsigned short value;
  value = (uint16_t) intervalValue[1] << 8;
  value |= (uint16_t) intervalValue[0];

  tempSensorInterval = value;
}

void ProcessIncomingData(byte byteData[]) 
{
  byte value32Bit[4] = {byteData[3], byteData[4], byteData[5], byteData[6]};
  byte value16Bit[2] = {byteData[5], byteData[6]};

  if (byteData[1] == 0x53 and byteData[2] == 0x57)
  {
    if (byteData[3] == 0x00 and byteData[4] == 0x00)
    {
      if (memcmp(byteData, WiFiReady, 8) == 0)
      {
        isWiFiConnected = true;
      }
      else
      {
        isWiFiConnected = false;
        if (autonomousDrivingEnabled)
        {
          autonomousDrivingEnabled = false;
          Stop();
        }
        tempSensorInterval = 0;
      }
    }
    else if (byteData[3] == 0x00 and byteData[4] == 0xFF)
    {
      if (memcmp(byteData, readyToMoveTrue, 8) == 0)
      {
        delay(500);
        digitalWrite(POWERLED, LOW);
        readyToMove = true;
        
        if(turnDelayAfterPhoto > 0)
        {
          bool randomTurn = random(0,2);
          if (randomTurn) MoveLeft();
          else MoveRight();
          delay(turnDelayAfterPhoto);
        }
      }
    }
  }
  
  else if (byteData[1] == 0x4D) 
  {
    if(memcmp(value32Bit, Forward, sizeof(value32Bit)) == 0)
    {
      MoveForward();
    }

    else if(memcmp(value32Bit, NormalStop, sizeof(value32Bit)) == 0)
    {
      Stop();
    }

    else if(memcmp(value32Bit, Left, sizeof(value32Bit)) == 0)
    {
      MoveLeft();
    }

    else if(memcmp(value32Bit, Right, sizeof(value32Bit)) == 0)
    {
      MoveRight();
    }

    else if(memcmp(value32Bit, Reverse, sizeof(value32Bit)) == 0)
    {
      MoveReverse();
    }

    else if(memcmp(value32Bit, ReverseLeft, sizeof(value32Bit)) == 0)
    {
      MoveReverseLeft();
    }

    else if(memcmp(value32Bit, ReverseRight, sizeof(value32Bit)) == 0)
    {
      MoveReverseRight();
    }
    
    else if(memcmp(value32Bit, RelaysOpenP, sizeof(value32Bit)) == 0)
    {
      RelaysOpen();
    }

    else if(memcmp(value32Bit, RelaysClosedP, sizeof(value32Bit)) == 0)
    {
      RelaysClosed();
    }
  }

  else if(byteData[1] == 0x4C)
  {
    if(memcmp(value32Bit, PowerLEDOn, sizeof(value32Bit)) == 0)
    {
      digitalWrite(POWERLED, HIGH);
    }

    else if(memcmp(value32Bit, PowerLEDOff, sizeof(value32Bit)) == 0)
    {
      digitalWrite(POWERLED, LOW);
    }
  }

  else if(byteData[1] == 0x42)
  {
    if(memcmp(value32Bit, BuzzerOn, sizeof(value32Bit)) == 0)
    {
      digitalWrite(BUZZER, HIGH);
    }

    else if(memcmp(value32Bit, BuzzerOff, sizeof(value32Bit)) == 0)
    {
      digitalWrite(BUZZER, LOW);
    }
  }

  else if(byteData[1] == 0x58)
  {
    unsigned int value;
    value = (uint32_t) value32Bit[3] << 24;
    value |= (uint32_t) value32Bit[2] << 16;
    value |= (uint32_t) value32Bit[1] << 8;
    value |= (uint32_t) value32Bit[0];

    XaxisServo.write(value);
  }

  else if(byteData[1] == 0x59)
  {
    unsigned int value;
    value = (uint32_t) value32Bit[3] << 24;
    value |= (uint32_t) value32Bit[2] << 16;
    value |= (uint32_t) value32Bit[1] << 8;
    value |= (uint32_t) value32Bit[0];

    YaxisServo.write(value);
    
  }

  else if(byteData[1] == 0x49)
  {
    if(byteData[3] == 0x00)
    {
      if(byteData[4] == 0x00) SetTempInterval(value16Bit);
    }
    else if(byteData[3] == 0xFF)
    {
      if(byteData[4] == 0x00 and byteData[5] == 0x00 and byteData[6] == 0x00)
      {
        byte byteArrayTempInterval[8];
        byteArrayTempInterval[0] = 0x23;
        byteArrayTempInterval[1] = 0x49;
        byteArrayTempInterval[2] = 0x5F;
        byteArrayTempInterval[3] = 0x00;
        byteArrayTempInterval[4] = 0x00;
        byteArrayTempInterval[5] = tempSensorInterval & 0x000000ff;
        byteArrayTempInterval[6] = (tempSensorInterval & 0x0000ff00) >> 8;
        byteArrayTempInterval[7] = 0x23;
        Serial.write(byteArrayTempInterval, 8);
      }
    }
  }

  else if(byteData[1] == 0x41)
  {
    if(byteData[6] == 0xFF)
    {
      autonomousDrivingEnabled = true;
      Serial.write(AutoDrivingOnConfirmation, 8);
    }
    else if(byteData[6] == 0x00)
    {
      autonomousDrivingEnabled = false;
      Stop();
      Serial.write(AutoDrivingOffConfirmation, 8);
    }
  }

  else if(byteData[1] == 0x46)
  {
    unsigned short value;
    value = (uint16_t) value16Bit[1] << 8;
    value |= (uint16_t) value16Bit[0];
      
    if(byteData[3] == 0x00 and byteData[4] == 0x00)
    {
      switch(value)
      {
        case 0:
          pictureInterval = 5000;
          break;
        case 1:
          pictureInterval = 10000;
          break;
        case 2:
          pictureInterval = 15000;
          break;
        case 3:
          pictureInterval = 20000;
          break;
        case 4:
          pictureInterval = 25000;
          break;
        case 5:
          pictureInterval = 30000;
          break;
      }
    }
    else if(byteData[3] == 0xFF and byteData[4] == 0x00)
    {
      switch(value)
      {
        case 0:
          turnDelayAfterPhoto = 2000;
          break;
        case 1:
          turnDelayAfterPhoto = 1000;
          break;
        case 2:
          turnDelayAfterPhoto = 500;
          break;
        case 3:
          turnDelayAfterPhoto = 0;
          break;
      }
    }
  }
}

void DataCheckProcess()
{
  byte dataBytes[8] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
  
  if(Serial.peek() == 0x23)
      {
        dataBytes[0] = Serial.read();
        if(Serial.peek() != 0x23)
        {
          for(byte i = 1; i < 8; i++)
          {
            dataBytes[i] = Serial.read();
          }
        }
        else
        {
          Serial.read();
          for(byte i = 1; i < 7; i++)
          {
            dataBytes[i] = Serial.read();
          }
          dataBytes[7] = 0x23;
        }
        ProcessIncomingData(dataBytes);
      }
  else Serial.read();
}
