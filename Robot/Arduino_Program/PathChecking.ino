unsigned short reverseDelay = 1500;
unsigned short turnDelayAfterReverse;
unsigned short turnDelay = 75;
byte distanceTrigger = 20;

unsigned short turnDelayAfterPicture;

float GetAccurateReading(int pinNumber)
{
  unsigned short sum = 0;
  for (int i = 0; i < 10; i++)
  {
    sum += analogRead(pinNumber);
  }

  return sum / 10;
}

void StopForPicture()
{
  Stop();
  digitalWrite(POWERLED, HIGH);
  delay(1000);
  Serial.write(requestPictureESP, 8);
}

void AvoidObstacles()
{
  float analogDistanceReading = GetAccurateReading(DISTANCESENSOR);
  float analogLeftProxReading = GetAccurateReading(LEFTPROXIMITY);
  float analogRightProxReading = GetAccurateReading(RIGHTPROXIMITY);
  
  float DIST = pow(analogDistanceReading * 3.15 / 675.0, -1.173) * 29.988;
  if (DIST > 80) DIST = 80;
  if (DIST < 10) DIST = 10;
  
  bool PROXL = (analogLeftProxReading > 256) ? 0 : 1;
  bool PROXR = (analogRightProxReading > 256) ? 0 : 1;

  turnDelayAfterReverse = random(500, 1251);

  if(DIST < distanceTrigger and PROXL == 0 and PROXR == 0)
  {
    bool randomTurn = random(0,2);
    if (randomTurn) MoveReverseLeft();
    else MoveReverseRight();
    delay(turnDelayAfterReverse);
  }

  else if (DIST < distanceTrigger and PROXL == 1 and PROXR == 0)
  {
    MoveReverse();
    delay(reverseDelay);
    MoveRight();
    delay(turnDelayAfterReverse);
  }

  else if (DIST < distanceTrigger and PROXL == 0 and PROXR == 1)
  {
    MoveReverse();
    delay(reverseDelay);
    MoveLeft();
    delay(turnDelayAfterReverse);
  }

  else if (DIST >= distanceTrigger and PROXL == 1 and PROXR == 0)
  {
    MoveRight();
    delay(turnDelay);
  }

  else if (DIST >= distanceTrigger and PROXL == 0 and PROXR == 1)
  {
    MoveLeft();
    delay(turnDelay);
  }

  else if (DIST >= distanceTrigger and PROXL == 1 and PROXR == 1)
  {
    PWM_DutyCycle = 70;
    MoveForward();
  }

  else if (DIST < distanceTrigger and PROXL == 1 and PROXR == 1)
  {
    PWM_DutyCycle = 70;
    MoveReverse();
    delay(reverseDelay);
  }

  else
  {
    PWM_DutyCycle = 128;
    MoveForward();
  }
}
