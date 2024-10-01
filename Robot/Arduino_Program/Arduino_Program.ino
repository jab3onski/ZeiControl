#include <Servo.h>

#define BUZZER 2
#define POWERLED 4
#define XSERVO 5
#define YSERVO 6
#define MOTOR_IN1 7
#define MOTOR_IN2 8
#define MOTOR_IN3 9
#define MOTOR_IN4 10
#define MOTOR_PWM 11
#define MOTOR_RELAY1 12
#define MOTOR_RELAY2 13

#define LEFTPROXIMITY A0
#define RIGHTPROXIMITY A1
#define TEMPSENSOR A2
#define DISTANCESENSOR A3
#define CELLVOLTAGEPROBE A4

Servo XaxisServo;
Servo YaxisServo;

byte XaxisServoValue;
byte YaxisServoValue;
byte PWM_DutyCycle;

bool isWiFiConnected;
bool autonomousDrivingEnabled;
bool readyToMove;

unsigned short turnDelayAfterPhoto;

unsigned short tempSensorInterval;
unsigned long tempSensorBaseMillis;

unsigned short proxSensorInterval;
unsigned long proxSensorBaseMillis;

unsigned short diagnosticsUpdateInterval;
unsigned long diagnosticsUpdateBaseMillis;

unsigned short pictureInterval;
unsigned long pictureBaseMillis;

unsigned short NetworkResponseInterval;
unsigned long NetworkResponseBaseMillis;

void setup() {
  Serial.begin(115200);
  delay(2000);

  XaxisServo.attach(XSERVO);
  YaxisServo.attach(YSERVO);

  XaxisServoValue = 90;
  YaxisServoValue = 90;

  XaxisServo.write(XaxisServoValue);
  YaxisServo.write(YaxisServoValue);

  pinMode(BUZZER, OUTPUT);
  pinMode(POWERLED, OUTPUT);
  pinMode(XSERVO, OUTPUT);
  pinMode(YSERVO, OUTPUT);
  pinMode(MOTOR_IN1, OUTPUT);
  pinMode(MOTOR_IN2, OUTPUT);
  pinMode(MOTOR_IN3, OUTPUT);
  pinMode(MOTOR_IN4, OUTPUT);
  pinMode(MOTOR_PWM, OUTPUT);
  pinMode(MOTOR_RELAY1, OUTPUT);
  pinMode(MOTOR_RELAY2, OUTPUT);

  PWM_DutyCycle = 128;

  isWiFiConnected = false;
  autonomousDrivingEnabled = false;
  readyToMove = true;

  tempSensorInterval = 0;
  proxSensorInterval = 300;
  diagnosticsUpdateInterval = 1000;
  pictureInterval = 0;

  tempSensorBaseMillis = millis();
  proxSensorBaseMillis = millis();
  diagnosticsUpdateBaseMillis = millis();
  pictureBaseMillis = millis();

  NetworkResponseInterval = 10;
  NetworkResponseBaseMillis = millis();
}

void loop() {
  if(autonomousDrivingEnabled and readyToMove)
  {
    if((millis() - pictureBaseMillis >= pictureInterval) and pictureInterval != 0)
    {
      readyToMove = false;
      StopForPicture();
      pictureBaseMillis = millis();
    }
    AvoidObstacles();
  }

  if(isWiFiConnected)
  {
    if((millis() - tempSensorBaseMillis >= tempSensorInterval) and tempSensorInterval > 0)
    {
      unsigned short analogReadingTemp = GetAccurateReading(TEMPSENSOR);
  
      byte byteArrayTemp[8];
      byteArrayTemp[0] = 0x23;
      byteArrayTemp[1] = 0x54;
      byteArrayTemp[2] = 0x5F;
      byteArrayTemp[3] = 0x00;
      byteArrayTemp[4] = 0x00;
      byteArrayTemp[5] = analogReadingTemp & 0x000000ff;
      byteArrayTemp[6] = (analogReadingTemp & 0x0000ff00) >> 8;
      byteArrayTemp[7] = 0x23;
  
      Serial.write(byteArrayTemp, 8);
  
      tempSensorBaseMillis = millis();
    }

    if(millis() - proxSensorBaseMillis >= proxSensorInterval)
    {
      unsigned short analogReadingLeftProx = GetAccurateReading(LEFTPROXIMITY);
      unsigned short analogReadingRightProx = GetAccurateReading(RIGHTPROXIMITY);
      unsigned short analogReadingDistSens = GetAccurateReading(DISTANCESENSOR);
  
      byte byteArrayLeftProx[8];
      byteArrayLeftProx[0] = 0x23;
      byteArrayLeftProx[1] = 0x50;
      byteArrayLeftProx[2] = 0x5F;
      byteArrayLeftProx[3] = 0x00;
      byteArrayLeftProx[4] = 0x00;
      byteArrayLeftProx[5] = analogReadingLeftProx & 0x000000ff;
      byteArrayLeftProx[6] = (analogReadingLeftProx & 0x0000ff00) >> 8;
      byteArrayLeftProx[7] = 0x23;
      
      byte byteArrayRightProx[8];
      byteArrayRightProx[0] = 0x23;
      byteArrayRightProx[1] = 0x50;
      byteArrayRightProx[2] = 0x5F;
      byteArrayRightProx[3] = 0x00;
      byteArrayRightProx[4] = 0xFF;
      byteArrayRightProx[5] = analogReadingRightProx & 0x000000ff;
      byteArrayRightProx[6] = (analogReadingRightProx & 0x0000ff00) >> 8;
      byteArrayRightProx[7] = 0x23;
      
      byte byteArrayDistSens[8];
      byteArrayDistSens[0] = 0x23;
      byteArrayDistSens[1] = 0x50;
      byteArrayDistSens[2] = 0x5F;
      byteArrayDistSens[3] = 0xFF;
      byteArrayDistSens[4] = 0x00;
      byteArrayDistSens[5] = analogReadingDistSens & 0x000000ff;
      byteArrayDistSens[6] = (analogReadingDistSens & 0x0000ff00) >> 8;
      byteArrayDistSens[7] = 0x23;
  
      Serial.write(byteArrayLeftProx, 8);
      Serial.write(byteArrayRightProx, 8);
      Serial.write(byteArrayDistSens, 8);
  
      proxSensorBaseMillis = millis();
    }
  
    if(millis() - diagnosticsUpdateBaseMillis >= diagnosticsUpdateInterval)
    {
      unsigned short analogReadingCellVoltage = GetAccurateReading(CELLVOLTAGEPROBE);
      unsigned long diagnosticsUpdateMillis = millis();
      
      byte byteArrayVehicleUptime[8];
      byteArrayVehicleUptime[0] = 0x23;
      byteArrayVehicleUptime[1] = 0x55;
      byteArrayVehicleUptime[2] = 0x5F;
      byteArrayVehicleUptime[3] = diagnosticsUpdateMillis & 0x000000ff;
      byteArrayVehicleUptime[4] = (diagnosticsUpdateMillis & 0x0000ff00) >> 8;
      byteArrayVehicleUptime[5] = (diagnosticsUpdateMillis & 0x00ff0000) >> 16;
      byteArrayVehicleUptime[6] = (diagnosticsUpdateMillis & 0xff000000) >> 24;
      byteArrayVehicleUptime[7] = 0x23;
      Serial.write(byteArrayVehicleUptime, 8);
      
      byte byteArrayDiagVoltage[8];
      byteArrayDiagVoltage[0] = 0x23;
      byteArrayDiagVoltage[1] = 0x44;
      byteArrayDiagVoltage[2] = 0x5F;
      byteArrayDiagVoltage[3] = 0x00;
      byteArrayDiagVoltage[4] = 0x00;
      byteArrayDiagVoltage[5] = analogReadingCellVoltage & 0x000000ff;
      byteArrayDiagVoltage[6] = (analogReadingCellVoltage & 0x0000ff00) >> 8;
      byteArrayDiagVoltage[7] = 0x23;
      Serial.write(byteArrayDiagVoltage, 8);

      diagnosticsUpdateBaseMillis = millis();
    }
  }

  if(millis() - NetworkResponseBaseMillis >= NetworkResponseInterval)
  {
    if(Serial.available() >= 8)
    {
      DataCheckProcess();
    }
    NetworkResponseBaseMillis = millis();
  }
}
